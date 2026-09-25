import { useEffect, useState } from 'react';
import ConfirmDialog from './ConfirmDialog';
import { CONFIRM_EVENT } from '../../lib/confirm';

// Renders the dialog for confirmAction(); one instance for the whole app.
export default function ConfirmHost() {
  const [request, setRequest] = useState(null);

  useEffect(() => {
    const onConfirm = (event) => setRequest(event.detail);
    window.addEventListener(CONFIRM_EVENT, onConfirm);
    return () => window.removeEventListener(CONFIRM_EVENT, onConfirm);
  }, []);

  const finish = (answer) => {
    request?.resolve(answer);
    setRequest(null);
  };

  return (
    <ConfirmDialog
      open={Boolean(request)}
      title={request?.title}
      message={request?.message}
      confirmLabel={request?.confirmLabel}
      cancelLabel={request?.cancelLabel}
      onConfirm={() => finish(true)}
      onCancel={() => finish(false)}
    />
  );
}
