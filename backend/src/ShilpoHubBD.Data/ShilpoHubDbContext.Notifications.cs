using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Messaging;
using ShilpoHubBD.Domain.Entities.Logistics;
using ShilpoHubBD.Domain.Entities.TouristBooking;
using ShilpoHubBD.Domain.Entities.Contracts;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.CustomOrders;
using ShilpoHubBD.Domain.Entities.Community;
using ShilpoHubBD.Domain.Entities.Procurement;
using ShilpoHubBD.Domain.Entities.Certificate;

namespace ShilpoHubBD.Data;

public partial class ShilpoHubDbContext
{
    // Notifications are saved in the same transaction as the activity: a failed
    // action cannot produce a successful-looking notification.
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var notifications = await BuildNotificationsAsync(cancellationToken);
        UserNotifications.AddRange(notifications);
        ChangeTracker.DetectChanges();
        AssignMissingGuidKeys();
        try { return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken); }
        catch
        {
            foreach (var item in notifications) Entry(item).State = EntityState.Detached;
            throw;
        }
    }

    private async Task<List<UserNotification>> BuildNotificationsAsync(CancellationToken ct)
    {
        ChangeTracker.DetectChanges();
        var changes = ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified).ToList();
        var output = new List<UserNotification>();
        void Add(Guid userId, string title, string body, string category, string? path = null)
        {
            if (userId == Guid.Empty || output.Any(n => n.UserId == userId && n.Title == title && n.Body == body && n.TargetPath == path)) return;
            output.Add(new UserNotification { UserId = userId, Title = title, Body = body, Category = category, TargetPath = path });
        }
        static string Words(string value) => Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
        foreach (var entry in changes)
        {
            if (entry.Entity is UserNotification) continue;
            if (entry.Metadata.ClrType.Name.EndsWith("Event", StringComparison.Ordinal)) continue;
            var added = entry.State == EntityState.Added;
            if (entry.Entity is Message message && added)
            {
                var recipients = await ConversationParticipants.Where(p => p.ConversationId == message.ConversationId && p.UserId != message.SenderId)
                    .Select(p => p.UserId).ToListAsync(ct);
                recipients.AddRange(ChangeTracker.Entries<ConversationParticipant>().Where(p => p.State == EntityState.Added && p.Entity.ConversationId == message.ConversationId && p.Entity.UserId != message.SenderId).Select(p => p.Entity.UserId));
                foreach (var recipient in recipients.Distinct()) Add(recipient, "New message", "You have a new message. Open your inbox to reply.", "Messages", "/dashboard/messages");
                continue;
            }
            if (entry.Entity is CommunityQuestion question && added)
            {
                var producerId = await Products.Where(p => p.Id == question.ProductId).Select(p => p.ProducerId).FirstOrDefaultAsync(ct);
                if (producerId != question.UserId) Add(producerId, "New customer question", "A customer asked a question about one of your products. Open Q&A to answer it.", "Community", "/producer/questions");
                continue;
            }
            if (entry.Entity is CommunityAnswer answer && added)
            {
                var asker = await CommunityQuestions.Where(q => q.Id == answer.QuestionId).Select(q => q.UserId).FirstOrDefaultAsync(ct);
                if (asker != answer.UserId) Add(asker, "Your question was answered", "A producer replied to your product question.", "Community", "/customer/community/qa");
                continue;
            }
            if (entry.Entity is ProcurementRequest deal && !added && entry.Property(nameof(ProcurementRequest.InspectionStatus)).IsModified)
            {
                if (deal.InspectionStatus == ProcurementInspectionStatus.Pending)
                {
                    var admins = await UserRoles.Where(r => r.Role.Name == "SuperAdmin").Select(r => r.UserId).ToListAsync(ct);
                    foreach (var admin in admins) Add(admin, "Bulk deal awaiting inspection", $"{deal.ReferenceNumber}: the advance is paid. Inspect the deal.", "Approvals", "/admin/procurements");
                }
                else if (deal.InspectionStatus is ProcurementInspectionStatus.Approved or ProcurementInspectionStatus.Rejected)
                {
                    var outcome = deal.InspectionStatus == ProcurementInspectionStatus.Approved ? "approved" : "rejected (advance refunded)";
                    Add(deal.BusinessPartnerId, "Deal inspection result", $"{deal.ReferenceNumber} was {outcome} at inspection.", "Partnerships", "/business-partner/procurement");
                    Add(deal.ProducerId, "Deal inspection result", $"{deal.ReferenceNumber} was {outcome} at inspection.", "Partnerships", "/producer/procurements");
                }
                continue;
            }
            if (entry.Entity is ExpertiseCertificate cert && added)
            {
                Add(cert.ProducerId, "You earned an expertise certificate", $"An admin issued you a {cert.Level} expertise certificate ({cert.CertificateNumber}).", "Approvals", "/producer/expertise");
                continue;
            }
            if (entry.Entity is OrderComplaint complaint)
            {
                if (added) Add(complaint.ProducerId, "Customer complaint", "A customer reported a problem with one of your products. Respond so they can settle it.", "Orders", "/producer/complaints");
                else if (complaint.Status == OrderComplaintStatus.Resolved) Add(complaint.CustomerId, "Complaint answered", "The producer responded to your complaint. Confirm you are satisfied or reopen it.", "Orders", "/customer/complaints");
                else if (complaint.Status == OrderComplaintStatus.Satisfied) Add(complaint.ProducerId, "Complaint settled", "The customer confirmed they are satisfied.", "Orders", "/producer/complaints");
                else if (complaint.Status == OrderComplaintStatus.Open) Add(complaint.ProducerId, "Complaint reopened", "The customer reopened a complaint. Please look at it again.", "Orders", "/producer/complaints");
                continue;
            }
            if (entry.Entity is UserProfile submitted && submitted.Status == UserProfileStatus.Pending
                && (added || entry.Property(nameof(UserProfile.UpdatedAt)).IsModified))
            {
                var admins = await UserRoles.Where(r => r.Role.Name == "SuperAdmin").Select(r => r.UserId).ToListAsync(ct);
                foreach (var admin in admins) Add(admin, "Profile awaiting approval", $"{submitted.LegalName} submitted a profile with NID for review.", "Approvals", "/admin/profile-approvals");
                continue;
            }
            if (entry.Entity is UserRole role && added)
            {
                Add(role.UserId, "Workspace access updated", "A role has been added to your account. Review your available workspaces in the account menu.", "Account", "/dashboard/profile");
                continue;
            }
            if (entry.Entity is OrderItem orderItem && (added || entry.Property(nameof(OrderItem.ProducerStatus)).IsModified))
            {
                var producerId = orderItem.Product?.ProducerId ?? await Products.Where(p => p.Id == orderItem.ProductId).Select(p => p.ProducerId).FirstOrDefaultAsync(ct);
                var customerId = orderItem.Order?.UserId ?? await Orders.Where(o => o.Id == orderItem.OrderId).Select(o => o.UserId).FirstOrDefaultAsync(ct);
                if (added) Add(producerId, "New order to fulfill", "A customer ordered one of your products. Review the order and arrange fulfillment.", "Orders", "/producer/orders");
                else Add(customerId, "Fulfillment updated", $"The producer updated your order item to {Words(orderItem.ProducerStatus.ToString()).ToLowerInvariant()}.", "Orders", $"/customer/orders/{orderItem.OrderId}");
                continue;
            }
            // Status-bearing records notify their actual user relationships. This
            // covers applications, collaborations, learning and verification flows
            // across all workspaces without exposing records to unrelated accounts.
            var status = entry.Properties.FirstOrDefault(p => p.Metadata.Name is "Status" or "ApprovalStatus");
            if (status is null || (!added && (!status.IsModified || Equals(status.OriginalValue, status.CurrentValue)))) continue;
            var recipientsForStatus = entry.Metadata.GetForeignKeys()
                .Where(f => f.PrincipalEntityType.ClrType == typeof(User))
                .SelectMany(f => f.Properties)
                .Where(p => p.Name is not ("ApprovedByUserId" or "ReviewedByUserId" or "VerifiedByUserId" or "HandmadeVerifiedByUserId" or "DecisionByUserId"))
                .Select(p => entry.Property(p.Name).CurrentValue).OfType<Guid>().ToHashSet();
            var label = Words(entry.Metadata.ClrType.Name);
            var state = Words(status.CurrentValue?.ToString() ?? "updated").ToLowerInvariant();
            var category = "Activity";
            string? path = null;
            if (entry.Entity is Product product)
            {
                category = "Approvals";
                path = "/producer/products";
                recipientsForStatus.Clear();
                recipientsForStatus.Add(product.ProducerId);
                if (product.ApprovalStatus == ProductApprovalStatus.Pending)
                {
                    var admins = await UserRoles.Where(r => r.Role.Name == "SuperAdmin").Select(r => r.UserId).ToListAsync(ct);
                    foreach (var admin in admins) Add(admin, "Product awaiting review", "A producer submitted a product for marketplace approval.", "Approvals", "/admin/marketplace/approval");
                }
            }
            else if (entry.Entity is Order order)
            {
                category = "Orders";
                path = $"/customer/orders/{order.Id}";
                if (order.Status is OrderStatus.ReturnRequested or OrderStatus.Returned or OrderStatus.Refunded)
                {
                    var sellers = await OrderItems.Where(i => i.OrderId == order.Id).Select(i => i.Product.ProducerId).Distinct().ToListAsync(ct);
                    var title = order.Status == OrderStatus.ReturnRequested ? "Customer return request" : "Return completed";
                    var body = order.Status == OrderStatus.ReturnRequested
                        ? $"A customer asked to return order {order.OrderNumber}. Review it and accept or reject."
                        : $"Order {order.OrderNumber} was returned to you and the customer refund is settled.";
                    foreach (var seller in sellers) Add(seller, title, body, "Orders", "/producer/returns");
                }
            }
            else if (entry.Entity is Booking booking)
            {
                Add(booking.TouristId, "Booking updated", $"Your booking is {state}.", "Bookings", "/tourism/bookings");
                Add(booking.ProducerId, "Booking updated", $"A booking for your service is {state}.", "Bookings");
                continue;
            }
            else if (entry.Entity is Contract contract)
            {
                Add(contract.ProducerId, "Contract updated", $"Your contract is {state}.", "Partnerships", "/producer/contracts");
                Add(contract.BusinessPartnerId, "Contract updated", $"Your contract is {state}.", "Partnerships", "/business-partner/contracts");
                continue;
            }
            else if (entry.Entity is CustomOrderRequest customOrder)
            {
                if (added) Add(customOrder.ProducerId, "New custom order request", "A customer asked you for a custom piece. Review the request and send a quote.", "Orders", "/producer/custom-orders");
                else Add(customOrder.CustomerId, "Custom order updated", $"Your custom order request is {state}.", "Orders", "/customer/custom-order");
                continue;
            }
            else if (entry.Entity is CourseEnrollment) { category = "Learning"; path = "/dashboard/academy"; }
            else if (entry.Entity is Shipment shipment)
            {
                category = "Deliveries";
                var owner = await LogisticsPartnerProfiles.Where(p => p.Id == shipment.LogisticsPartnerProfileId).Select(p => p.UserId).FirstOrDefaultAsync(ct);
                Add(owner, "Shipment updated", $"Shipment {shipment.TrackingNumber} is {state}.", category, "/logistics-partner/shipments");
                if (shipment.OrderId is Guid orderId)
                {
                    var customer = await Orders.Where(o => o.Id == orderId).Select(o => o.UserId).FirstOrDefaultAsync(ct);
                    Add(customer, "Delivery updated", $"Your shipment is {state}.", category, $"/customer/orders/{orderId}");
                    var sellers = await OrderItems.Where(i => i.OrderId == orderId).Select(i => i.Product.ProducerId).Distinct().ToListAsync(ct);
                    foreach (var seller in sellers) Add(seller, "Parcel update", $"Shipment {shipment.TrackingNumber} for your order is {state}.", category, "/producer/orders");
                }
                else
                {
                    var custom = await CustomOrderRequests.Where(c => c.TrackingNumber == shipment.TrackingNumber)
                        .Select(c => new { c.CustomerId, c.ProducerId }).FirstOrDefaultAsync(ct);
                    if (custom is not null)
                    {
                        Add(custom.CustomerId, "Custom order delivery update", $"Your custom order shipment {shipment.TrackingNumber} is {state}.", category, "/customer/custom-order");
                        Add(custom.ProducerId, "Custom order parcel update", $"Shipment {shipment.TrackingNumber} for your custom order is {state}.", category, "/producer/custom-orders");
                    }
                }
                continue;
            }
            foreach (var recipient in recipientsForStatus)
                Add(recipient, $"{label} {(added ? "created" : "updated")}", $"Your {label.ToLowerInvariant()} is {state}.", category, path);
        }
        return output;
    }
}
