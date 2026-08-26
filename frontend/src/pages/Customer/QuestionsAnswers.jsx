import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, QnASection, AsyncState } from '../../components/ui';
import { useProducts } from '../../hooks/useProducts';
import { useProductQuestions, useQuestionMutations } from '../../hooks/useQuestions';

export default function QuestionsAnswers() {
  const [searchParams, setSearchParams] = useSearchParams();
  const productId = searchParams.get('productId') || '';
  const [showAsk, setShowAsk] = useState(false);
  const [question, setQuestion] = useState('');

  const productsQuery = useProducts({ pageSize: 50 });
  const questionsQuery = useProductQuestions(productId);
  const { ask } = useQuestionMutations(productId);

  const handleAsk = (event) => {
    event.preventDefault();
    ask.mutate(question, {
      onSuccess: () => {
        setQuestion('');
        setShowAsk(false);
      },
    });
  };

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Community', path: routePaths.customerCommunity },
          { label: 'Questions & Answers' },
        ]}
        title="Questions & Answers"
        description="Get answers straight from verified producers about materials, technique and authenticity."
        action={
          <div className="flex items-center gap-3">
            <Link to={routePaths.customerForum} className="text-sm font-medium text-link hover:underline">
              Discussion Forum →
            </Link>
            {productId && (
              <Button variant="primary" onClick={() => setShowAsk((prev) => !prev)}>
                {showAsk ? 'Cancel' : 'Ask a Question'}
              </Button>
            )}
          </div>
        }
      />

      <div className="mb-6 max-w-sm">
        <label className="mb-1.5 block text-sm font-medium text-body/70">Choose a product to view its Q&amp;A</label>
        <select
          value={productId}
          onChange={(event) => setSearchParams(event.target.value ? { productId: event.target.value } : {})}
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Select a product…</option>
          {productsQuery.data?.items.map((product) => (
            <option key={product.id} value={product.id}>
              {product.name}
            </option>
          ))}
        </select>
      </div>

      {productId && showAsk && (
        <form onSubmit={handleAsk} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <textarea
            required
            rows={3}
            placeholder="What would you like to know about this product?"
            value={question}
            onChange={(event) => setQuestion(event.target.value)}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
          <Button type="submit" variant="primary" disabled={ask.isPending}>
            {ask.isPending ? 'Posting…' : 'Post Question'}
          </Button>
        </form>
      )}

      {!productId ? (
        <p className="rounded-xl border border-border bg-surface p-6 text-center text-sm text-body/60">
          Pick a product above to see questions and answers about it.
        </p>
      ) : (
        <AsyncState isLoading={questionsQuery.isLoading} isError={questionsQuery.isError} error={questionsQuery.error}>
          <div className="space-y-5">
            {questionsQuery.data?.items.map((qa) => (
              <QnASection
                key={qa.id}
                qa={{
                  craft: '',
                  askedBy: qa.askerName,
                  time: new Date(qa.createdAt).toLocaleDateString(),
                  question: qa.body,
                  answers: qa.answers.map((answer) => ({
                    id: answer.id,
                    author: answer.authorName,
                    isProducer: answer.isProducerAnswer,
                    body: answer.body,
                    votes: 0,
                  })),
                }}
              />
            ))}
            {questionsQuery.data?.items.length === 0 && (
              <p className="text-center text-sm text-body/60">No questions yet for this product — be the first to ask.</p>
            )}
          </div>
        </AsyncState>
      )}
    </div>
  );
}
