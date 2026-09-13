function decodeJwtPayload(token) {
  try {
    const [, payload] = String(token || '').split('.');
    if (!payload) return null;

    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');
    return JSON.parse(atob(padded));
  } catch {
    return null;
  }
}

export function isAccessTokenUsable(token, clockSkewSeconds = 15) {
  if (!token) return false;
  const payload = decodeJwtPayload(token);
  const expiration = Number(payload?.exp);
  if (!Number.isFinite(expiration)) return false;

  return expiration * 1000 > Date.now() + clockSkewSeconds * 1000;
}