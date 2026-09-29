import { createPortal } from 'react-dom';
import { useEffect, useState } from 'react';
export default function AssistantLauncher({ children }) {
  const [slot, setSlot] = useState(null);
  useEffect(() => {
    const sync = () => setSlot(document.getElementById('messages-help-slot'));
    sync();
    const observer = new MutationObserver(sync);
    observer.observe(document.body, { childList: true, subtree: true });
    return () => observer.disconnect();
  }, []);
  return slot ? createPortal(children, slot) : children;
}
