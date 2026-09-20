import { useCallback, useEffect, useRef, useState } from 'react';
import { useAuth } from '../../hooks/useAuth';

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

/* ---------------------------------------------------------------------------
 * Demo knowledge base — replace `resolveAnswer` with a real API call later.
 * ------------------------------------------------------------------------- */

const SUGGESTIONS = [
  'What is Jamdani?',
  'How is Shital Pati traditionally made?',
  'What can I do on ShilpoHub?',
];

const KNOWLEDGE_BASE = [
  {
    match: ['jamdani'],
    answer:
      'Jamdani is a hand-woven muslin textile from the Dhaka region, traditionally made on a pit loom by two weavers working side by side. The motifs are never printed or embroidered — each one is picked into the weft by hand as the cloth grows, which is why a single sari can take months. UNESCO inscribed the art of Jamdani weaving on its Representative List of Intangible Cultural Heritage in 2013.',
    sources: ['craft.json', 'unesco-heritage.md'],
  },
  {
    match: ['shital pati', 'shitalpati', 'sital pati'],
    answer:
      'Shital Pati is a cooling mat woven from the soft inner bark of the murta plant, most closely associated with Sylhet. Artisans boil and split the cane into fine strips, sun-dry them, then weave the strips into dense geometric patterns. The finished mat stays cool against the skin, which is where the name comes from.',
    sources: ['craft.json', 'regional-crafts.json'],
  },
  {
    match: ['what can i do', 'shilpohub', 'platform', 'features'],
    answer:
      'ShilpoHub connects you to Bangladeshi craft in a few ways: browse and buy directly from verified artisans in the marketplace, trace a product back to its maker and region, enrol in Academy courses taught by master artisans, and explore heritage trails that map crafts to the places they come from.',
    sources: ['help-center.md', 'platform-guide.json'],
  },
];

const FALLBACK_ANSWER = {
  answer:
    'I could not find that in the ShilpoHub knowledge base yet. Try asking about a specific craft — Jamdani, Nakshi Kantha, Shital Pati — or about how the marketplace and Academy work.',
  sources: ['help-center.md'],
};

function resolveAnswer(question) {
  const normalized = question.trim().toLowerCase();
  const hit = KNOWLEDGE_BASE.find((entry) => entry.match.some((term) => normalized.includes(term)));
  return hit || FALLBACK_ANSWER;
}

let messageCounter = 0;
function nextId() {
  messageCounter += 1;
  return `msg-${messageCounter}`;
}

/* ---------------------------------------------------------------------------
 * Presentational pieces
 * ------------------------------------------------------------------------- */

function SourceTags({ sources }) {
  if (!sources?.length) return null;

  return (
    <ul className="mt-2 flex flex-wrap gap-1.5">
      {sources.map((source) => (
        <li key={source}>
          <span className="inline-flex items-center gap-1 rounded-lg bg-white px-2 py-1 text-[11px] font-medium text-gray-500 ring-1 ring-gray-200">
            <SourceIcon className="h-3 w-3" />
            {source}
          </span>
        </li>
      ))}
    </ul>
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

  return (
    <li className="flex justify-start">
      <div className="max-w-[88%] rounded-2xl rounded-bl-md bg-gray-100/80 p-4">
        <p className="text-sm leading-relaxed text-gray-700">{message.text}</p>
        <SourceTags sources={message.sources} />
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
  const replyTimer = useRef(null);

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

  useEffect(() => () => clearTimeout(replyTimer.current), []);

  const ask = useCallback((question) => {
    const text = question.trim();
    if (!text) return;

    setMessages((current) => [...current, { id: nextId(), role: 'user', text }]);
    setInputValue('');
    setIsTyping(true);

    clearTimeout(replyTimer.current);
    replyTimer.current = setTimeout(() => {
      const { answer, sources } = resolveAnswer(text);
      setMessages((current) => [...current, { id: nextId(), role: 'assistant', text: answer, sources }]);
      setIsTyping(false);
    }, 900);
  }, []);

  const handleSubmit = (event) => {
    event.preventDefault();
    ask(inputValue);
  };

  const canSend = inputValue.trim().length > 0 && !isTyping;

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
