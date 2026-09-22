import { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import {
  findHeritageDemoAnswer,
  HERITAGE_DEMO_FALLBACK_ID,
  heritageDemoSuggestions,
} from '../../data/heritageDemoQuestions';
import { routePaths } from '../../routes/routePaths';

/* ---------------------------------------------------------------------------
 * Icons — Lucide glyph paths inlined so the widget stays dependency-free.
 * Swap for `import { Sparkles, X, ArrowUp } from 'lucide-react'` if installed.
 * ------------------------------------------------------------------------- */

function Icon({ children, className = 'h-5 w-5', ...props }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden="true"
      focusable="false"
      {...props}
    >
      {children}
    </svg>
  );
}

const SparklesIcon = (props) => (
  <Icon {...props}>
    <path d="M9.937 15.5A2 2 0 0 0 8.5 14.063l-6.135-1.582a.5.5 0 0 1 0-.962L8.5 9.936A2 2 0 0 0 9.937 8.5l1.582-6.135a.5.5 0 0 1 .963 0L14.063 8.5A2 2 0 0 0 15.5 9.937l6.135 1.581a.5.5 0 0 1 0 .964L15.5 14.063a2 2 0 0 0-1.437 1.437l-1.582 6.135a.5.5 0 0 1-.963 0z" />
    <path d="M20 3v4" />
    <path d="M22 5h-4" />
    <path d="M4 17v2" />
    <path d="M5 18H3" />
  </Icon>
);

const HelpIcon = (props) => (
  <Icon {...props}>
    <circle cx="12" cy="12" r="10" />
    <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" />
    <path d="M12 17h.01" />
  </Icon>
);

const CloseIcon = (props) => (
  <Icon {...props}>
    <path d="M18 6 6 18" />
    <path d="m6 6 12 12" />
  </Icon>
);

const SendIcon = (props) => (
  <Icon {...props}>
    <path d="m5 12 7-7 7 7" />
    <path d="M12 19V5" />
  </Icon>
);

const SourceIcon = (props) => (
  <Icon {...props}>
    <path d="M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z" />
    <path d="M14 2v4a2 2 0 0 0 2 2h4" />
    <path d="M16 13H8" />
    <path d="M16 17H8" />
  </Icon>
);

const TagIcon = (props) => (
  <Icon {...props}>
    <path d="M12.586 2.586A2 2 0 0 0 11.172 2H4a2 2 0 0 0-2 2v7.172a2 2 0 0 0 .586 1.414l8.704 8.704a2.426 2.426 0 0 0 3.42 0l6.58-6.58a2.426 2.426 0 0 0 0-3.42z" />
    <circle cx="7.5" cy="7.5" r=".5" fill="currentColor" />
  </Icon>
);

const CheckIcon = (props) => (
  <Icon {...props}>
    <path d="M20 6 9 17l-5-5" />
  </Icon>
);

/* ---------------------------------------------------------------------------
 * Answer source — the one seam between the UI and where answers come from.
 *
 * Today it resolves against the static dataset in `data/heritageDemoQuestions`
 * (no API, RAG or DB call before login). To go live, replace the body of
 * `requestHeritageAnswer` with the real call, e.g.
 *
 *   const { data } = await api.post('/ai/heritage-assistant/ask', { question });
 *   return {
 *     answer: data.answer,
 *     source: data.sources,        // string or string[] — both render
 *     category: data.category,
 *     confidence: data.confidence,
 *   };
 *
 * Nothing below this block needs to change.
 * ------------------------------------------------------------------------- */

const SUGGESTIONS = heritageDemoSuggestions;

/** Mimics network latency so the typing indicator has something to cover. */
const REPLY_DELAY_MS = 900;

/**
 * Before-login boundary: the three demo answers stay open to everyone, and the
 * sign-in CTA only appears once a visitor has actually explored — two distinct
 * demo topics answered, or a question the preview dataset cannot cover.
 */
const MIN_TOPICS_BEFORE_CTA = 2;

function shouldShowSignInCta(messages) {
  const answers = messages.filter((message) => message.role === 'assistant' && message.demoId);
  if (answers.some((message) => message.demoId === HERITAGE_DEMO_FALLBACK_ID)) return true;

  const topics = new Set(answers.map((message) => message.demoId));
  topics.delete(HERITAGE_DEMO_FALLBACK_ID);
  return topics.size >= MIN_TOPICS_BEFORE_CTA;
}

const ERROR_ANSWER = {
  answer: 'Something went wrong while looking that up. Please try asking again.',
  tone: 'error',
};

function requestHeritageAnswer(question) {
  return new Promise((resolve) => {
    setTimeout(() => resolve(findHeritageDemoAnswer(question)), REPLY_DELAY_MS);
  });
}

let messageCounter = 0;
function nextId() {
  messageCounter += 1;
  return `msg-${messageCounter}`;
}

/* ---------------------------------------------------------------------------
 * Presentational pieces
 * ------------------------------------------------------------------------- */

/**
 * Subtle citation strip under an answer — source, category, confidence.
 * `source` accepts a string or an array, so a future RAG response listing
 * several sources renders here without a change.
 */
function AnswerMeta({ source, category, confidence }) {
  const sourceLabel = Array.isArray(source) ? source.filter(Boolean).join(' · ') : source;

  const items = [
    { key: 'source', label: 'Source', value: sourceLabel, Glyph: SourceIcon },
    { key: 'category', label: 'Category', value: category, Glyph: TagIcon },
    { key: 'confidence', label: 'Confidence', value: confidence, Glyph: CheckIcon },
  ].filter((item) => Boolean(item.value));

  if (!items.length) return null;

  return (
    <dl className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-1 border-t border-gray-200/70 pt-2.5 text-[11px] leading-tight text-gray-500">
      {items.map(({ key, label, value, Glyph }) => (
        <div key={key} className="flex items-center gap-1">
          <Glyph className="h-3 w-3 shrink-0" />
          <dt className="sr-only">{label}</dt>
          <dd>
            <span className="text-gray-400">{label}:</span>{' '}
            <span className="font-medium text-gray-600">{value}</span>
          </dd>
        </div>
      ))}
    </dl>
  );
}

/**
 * Sign-in invitation shown below the conversation. Deliberately styled as
 * another message-width block in the panel's own palette — not an overlay,
 * modal or banner.
 */
function SignInCallout({ onNavigate }) {
  const linkBase =
    'rounded-full px-4 py-2 text-xs font-semibold transition-all duration-200 focus:outline-none focus-visible:ring-2';

  return (
    <div className="mt-4 rounded-2xl border border-gray-100 bg-gradient-to-br from-fuchsia-50/60 via-white to-purple-50/60 p-4">
      <p className="text-sm font-semibold text-gray-900">Want to explore more of Bangladesh&rsquo;s heritage?</p>
      <p className="mt-1 text-xs leading-relaxed text-gray-500">
        Sign in to access the full ShilpoHub Heritage AI.
      </p>
      <div className="mt-3 flex flex-wrap gap-2">
        <Link
          to={routePaths.login}
          onClick={onNavigate}
          className={`${linkBase} bg-teal-900 text-white shadow-sm hover:-translate-y-0.5 hover:bg-teal-800 focus-visible:ring-teal-900/25`}
        >
          Sign In
        </Link>
        <Link
          to={routePaths.register}
          onClick={onNavigate}
          className={`${linkBase} border border-gray-200 bg-white text-gray-600 hover:-translate-y-0.5 hover:border-teal-300 hover:bg-teal-50 hover:text-teal-800 focus-visible:ring-teal-500/30`}
        >
          Create Account
        </Link>
      </div>
    </div>
  );
}

function MessageBubble({ message }) {
  if (message.role === 'user') {
    return (
      <li className="flex justify-end">
        <p className="max-w-[80%] rounded-2xl rounded-br-md bg-teal-900 px-4 py-2.5 text-sm leading-relaxed text-white shadow-sm">
          {message.text}
        </p>
      </li>
    );
  }

  const isError = message.tone === 'error';

  return (
    <li className="flex justify-start">
      <div
        className={`max-w-[88%] rounded-2xl rounded-bl-md p-4 ${
          isError ? 'bg-rose-50 ring-1 ring-rose-100' : 'bg-gray-100/80'
        }`}
      >
        <p className={`text-sm leading-relaxed ${isError ? 'text-rose-700' : 'text-gray-700'}`}>
          {message.text}
        </p>
        <AnswerMeta source={message.source} category={message.category} confidence={message.confidence} />
      </div>
    </li>
  );
}

function TypingIndicator() {
  return (
    <li className="flex justify-start">
      <div className="flex items-center gap-1.5 rounded-2xl rounded-bl-md bg-gray-100/80 px-4 py-3.5">
        <span className="sr-only">AI Answers is typing</span>
        {[0, 150, 300].map((delay) => (
          <span
            key={delay}
            className="h-1.5 w-1.5 animate-bounce rounded-full bg-gray-400"
            style={{ animationDelay: `${delay}ms` }}
          />
        ))}
      </div>
    </li>
  );
}

/* ---------------------------------------------------------------------------
 * Widget
 * ------------------------------------------------------------------------- */

export default function AIAssistantWidget() {
  const { isAuthenticated, isHydrated } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const [isRendered, setIsRendered] = useState(false);
  const [isEntered, setIsEntered] = useState(false);
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState('');
  const [isTyping, setIsTyping] = useState(false);

  const inputRef = useRef(null);
  const scrollRef = useRef(null);
  // Only the newest question may write a reply; anything older is discarded.
  const requestIdRef = useRef(0);
  const isMountedRef = useRef(true);

  const hasConversation = messages.length > 0;

  // Mount first, animate in on the next frame; animate out before unmounting.
  useEffect(() => {
    if (isOpen) {
      setIsRendered(true);
      const frame = requestAnimationFrame(() => setIsEntered(true));
      return () => cancelAnimationFrame(frame);
    }

    setIsEntered(false);
    const timer = setTimeout(() => setIsRendered(false), 200);
    return () => clearTimeout(timer);
  }, [isOpen]);

  useEffect(() => {
    if (isEntered) inputRef.current?.focus();
  }, [isEntered]);

  useEffect(() => {
    if (!isOpen) return undefined;

    const handleKeyDown = (event) => {
      if (event.key === 'Escape') setIsOpen(false);
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen]);

  useEffect(() => {
    const node = scrollRef.current;
    if (node) node.scrollTop = node.scrollHeight;
  }, [messages, isTyping]);

  // Re-arm on every mount so React 19 StrictMode's mount/unmount/mount cycle
  // does not leave the widget permanently unable to render a reply.
  useEffect(() => {
    isMountedRef.current = true;
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  const ask = useCallback(async (question) => {
    const text = question.trim();
    if (!text) return;

    requestIdRef.current += 1;
    const requestId = requestIdRef.current;

    setMessages((current) => [...current, { id: nextId(), role: 'user', text }]);
    setInputValue('');
    setIsTyping(true);

    let reply;
    try {
      reply = await requestHeritageAnswer(text);
    } catch {
      reply = ERROR_ANSWER;
    }

    if (!isMountedRef.current || requestId !== requestIdRef.current) return;

    setMessages((current) => [
      ...current,
      {
        id: nextId(),
        role: 'assistant',
        text: reply.answer,
        demoId: reply.id,
        source: reply.source,
        category: reply.category,
        confidence: reply.confidence,
        tone: reply.tone,
      },
    ]);
    setIsTyping(false);
  }, []);

  const handleSubmit = (event) => {
    event.preventDefault();
    ask(inputValue);
  };

  const canSend = inputValue.trim().length > 0 && !isTyping;
  // Hold the CTA back until the reply it follows is on screen.
  const showSignInCta = !isTyping && shouldShowSignInCta(messages);

  // Pre-login helper only: stay hidden until the session is known, and for signed-in
  // visitors, who get the in-app support surfaces instead.
  if (!isHydrated || isAuthenticated) return null;

  return (
    <>
      {/* Floating action button */}
      <button
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        aria-expanded={isOpen}
        aria-controls="ai-assistant-panel"
        aria-label={isOpen ? 'Close AI Answers' : 'Open AI Answers'}
        className="fixed bottom-6 right-6 z-50 flex h-14 w-14 items-center justify-center rounded-2xl bg-teal-900 text-white shadow-xl transition-all duration-200 hover:scale-105 hover:bg-teal-800 focus:outline-none focus-visible:ring-4 focus-visible:ring-teal-900/25 active:scale-95"
      >
        {isOpen ? <CloseIcon className="h-6 w-6" /> : <HelpIcon className="h-6 w-6" />}
      </button>

      {/* Chat popover */}
      {isRendered && (
        <div
          id="ai-assistant-panel"
          role="dialog"
          aria-label="AI Answers assistant"
          className={`fixed bottom-24 left-4 right-4 z-50 origin-bottom-right transition-all duration-200 ease-out sm:left-auto sm:right-6 sm:w-[390px] ${
            isEntered ? 'translate-y-0 scale-100 opacity-100' : 'translate-y-3 scale-95 opacity-0'
          }`}
        >
          <div className="flex h-[min(600px,70vh)] flex-col overflow-hidden rounded-3xl border border-gray-100 bg-white shadow-2xl">
            {/* Header */}
            <header className="relative flex shrink-0 flex-col items-center px-6 pb-5 pt-7">
              <button
                type="button"
                onClick={() => setIsOpen(false)}
                aria-label="Close AI Answers"
                className="absolute right-4 top-4 rounded-full p-1.5 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 focus:outline-none focus-visible:ring-2 focus-visible:ring-gray-300"
              >
                <CloseIcon className="h-4 w-4" />
              </button>

              <div className="relative">
                <span
                  aria-hidden="true"
                  className="absolute -inset-2 rounded-full bg-gradient-to-br from-fuchsia-400 via-purple-400 to-pink-400 opacity-30 blur-lg"
                />
                <span className="relative flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-fuchsia-500 via-purple-500 to-pink-500 p-[2px]">
                  <span className="flex h-full w-full items-center justify-center rounded-[14px] bg-white">
                    <SparklesIcon className="h-5 w-5 text-purple-600" />
                  </span>
                </span>
              </div>

              <h2 className="mt-3 text-lg font-bold tracking-tight text-gray-900">AI Answers</h2>
              <p className="mt-0.5 text-xs text-gray-500">Powered by ShilpoHub Knowledge Base</p>
            </header>

            {/* Messages */}
            <div ref={scrollRef} className="flex-1 overflow-y-auto px-5 pb-5">
              <ul className="space-y-3">
                {!hasConversation && (
                  <li className="rounded-2xl bg-gray-100/80 p-4 text-sm leading-relaxed text-gray-700">
                    Hello, I can find answers from the{' '}
                    <button
                      type="button"
                      onClick={() => ask('What can I do on ShilpoHub?')}
                      className="font-semibold text-teal-600 underline decoration-teal-600/30 underline-offset-2 transition-colors hover:text-teal-700 hover:decoration-teal-700"
                    >
                      ShilpoHub help center
                    </button>
                    . How can I help?
                  </li>
                )}

                {messages.map((message) => (
                  <MessageBubble key={message.id} message={message} />
                ))}

                {isTyping && <TypingIndicator />}
              </ul>

              {showSignInCta && <SignInCallout onNavigate={() => setIsOpen(false)} />}

              {!hasConversation && (
                <div className="mt-4 flex flex-wrap gap-2">
                  {SUGGESTIONS.map((suggestion) => (
                    <button
                      key={suggestion}
                      type="button"
                      onClick={() => ask(suggestion)}
                      className="rounded-full border border-gray-200 bg-white px-3.5 py-1.5 text-xs font-medium text-gray-600 transition-all hover:-translate-y-0.5 hover:border-teal-300 hover:bg-teal-50 hover:text-teal-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500/30"
                    >
                      {suggestion}
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Input footer */}
            <div className="mt-auto shrink-0">
              <div
                aria-hidden="true"
                className="h-px w-full bg-gradient-to-r from-fuchsia-500 via-purple-500 to-pink-500"
              />
              <form onSubmit={handleSubmit} className="flex items-center gap-2 bg-white px-4 py-3">
                <label htmlFor="ai-assistant-input" className="sr-only">
                  Ask a question
                </label>
                <input
                  id="ai-assistant-input"
                  ref={inputRef}
                  type="text"
                  value={inputValue}
                  onChange={(event) => setInputValue(event.target.value)}
                  placeholder="Ask a question..."
                  autoComplete="off"
                  className="min-w-0 flex-1 rounded-xl border border-gray-200 bg-gray-50 px-3.5 py-2.5 text-sm text-gray-800 placeholder:text-gray-400 focus:border-teal-500 focus:bg-white focus:outline-none focus:ring-2 focus:ring-teal-500/15"
                />
                <button
                  type="submit"
                  disabled={!canSend}
                  aria-label="Send question"
                  className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl transition-all duration-200 focus:outline-none focus-visible:ring-4 focus-visible:ring-teal-900/20 ${
                    canSend
                      ? 'bg-teal-900 text-white shadow-md hover:bg-teal-800 active:scale-95'
                      : 'cursor-not-allowed bg-gray-100 text-gray-400'
                  }`}
                >
                  <SendIcon className="h-4 w-4" />
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
