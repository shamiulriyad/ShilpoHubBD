using System.Security.Cryptography;
using System.Text;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.ProductSearch;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Services.Reviews;

/// <summary>
/// Feeds the review vector index. Mirrors <see cref="ProductSearch.ProductIndexService"/> exactly (pending/ack,
/// hash-based upsert-vs-payload-only decision) but for product reviews, kept on its own state table and its own
/// Qdrant collection on the Python side so review vectors never mix with product search or heritage/tourism data.
/// </summary>
public class ReviewIndexService : IReviewIndexService
{
    public const int MaxAttempts = 5;
    private const int MaxBatch = 100;

    private readonly IReviewIndexRepository _repository;

    public ReviewIndexService(IReviewIndexRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReviewIndexBatchDto> GetPendingAsync(int limit, CancellationToken cancellationToken)
    {
        var states = await _repository.GetPendingAsync(Math.Clamp(limit, 1, MaxBatch), MaxAttempts, cancellationToken);
        var batch = new ReviewIndexBatchDto { PendingTotal = await _repository.CountPendingAsync(MaxAttempts, cancellationToken) };
        if (states.Count == 0)
        {
            return batch;
        }

        var ids = states.Select(s => s.ReviewId).ToList();
        var reviews = await _repository.GetReviewsForIndexAsync(ids, cancellationToken);
        var analyses = await _repository.GetAnalysesAsync(ids, cancellationToken);

        foreach (var state in states)
        {
            if (state.Status == IndexStatuses.Deleted || !reviews.TryGetValue(state.ReviewId, out var review) || review.Product is null)
            {
                batch.Items.Add(new ReviewIndexItemDto { ReviewId = state.ReviewId, Version = state.Version, Action = "delete" });
                continue;
            }

            analyses.TryGetValue(state.ReviewId, out var analysis);

            var text = BuildText(review, analysis);
            var hash = Hash(text);
            batch.Items.Add(new ReviewIndexItemDto
            {
                ReviewId = review.Id,
                Version = state.Version,
                // Same review text as last time (only its AI-analysis metadata changed): refresh only the
                // filterable snapshot, no embedding call.
                Action = hash == state.TextHash ? "payload" : "upsert",
                TextHash = hash,
                Text = text,
                Payload = BuildPayload(review, analysis),
            });
        }

        return batch;
    }

    public async Task AckAsync(ReviewIndexAckRequest request, CancellationToken cancellationToken)
    {
        var states = await _repository.GetStatesAsync(request.Items.Select(i => i.ReviewId).Distinct().ToList(), cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var item in request.Items)
        {
            if (!states.TryGetValue(item.ReviewId, out var state))
            {
                continue;
            }

            if (!item.Success)
            {
                state.Status = IndexStatuses.Failed;
                state.AttemptCount++;
                state.LastError = item.Error is { Length: > 1000 } ? item.Error[..1000] : item.Error;
                state.UpdatedAt = now;
                continue;
            }

            // Changed again while the worker was busy: stay Dirty so the newer version is picked up next round.
            if (state.Version != item.Version)
            {
                continue;
            }

            if (state.Status == IndexStatuses.Deleted)
            {
                _repository.RemoveState(state);
                continue;
            }

            state.Status = IndexStatuses.Synced;
            state.TextHash = item.TextHash ?? state.TextHash;
            state.EmbeddingModel = item.EmbeddingModel ?? state.EmbeddingModel;
            state.IndexedAt = now;
            state.AttemptCount = 0;
            state.LastError = null;
            state.UpdatedAt = now;
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- document building -------------------------------------------------------------------------------------

    private static string BuildText(Review review, ReviewAiAnalysis? analysis)
    {
        var sb = new StringBuilder();
        sb.Append(review.Comment.Trim());
        if (analysis is { IssueSummary.Length: > 0 })
        {
            sb.Append(' ').Append(analysis.IssueSummary.Trim());
        }

        return sb.ToString();
    }

    /// <summary>The metadata required to identify what this vector is about; PostgreSQL is re-read for anything else.</summary>
    private static Dictionary<string, object?> BuildPayload(Review review, ReviewAiAnalysis? analysis) => new()
    {
        ["review_id"] = review.Id,
        ["product_id"] = review.ProductId,
        ["producer_id"] = review.Product?.ProducerId,
        ["created_at"] = review.CreatedAt,
        ["rating"] = review.Rating,
        ["is_negative"] = analysis?.IsNegative,
        ["is_product_related"] = analysis?.IsProductRelated,
        ["complaint_type"] = analysis?.ComplaintType.ToString(),
        ["severity"] = analysis?.Severity.ToString(),
        ["confidence"] = analysis?.Confidence,
    };

    private static string Hash(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
