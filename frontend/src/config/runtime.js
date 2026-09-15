const trimTrailingSlash = (value) => value.replace(/\/+$/, '');

const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim();

/**
 * In development we mirror the backend's HTTP launch profile. In production,
 * a relative /api default keeps the build deployable behind the same reverse
 * proxy without accidentally calling a developer machine.
 */
export const API_BASE_URL = trimTrailingSlash(
  configuredApiBaseUrl || (import.meta.env.DEV ? 'http://localhost:5065/api' : '/api'),
);

const configuredTimeout = Number(import.meta.env.VITE_API_TIMEOUT_MS);
export const API_TIMEOUT_MS =
  Number.isFinite(configuredTimeout) && configuredTimeout > 0 ? configuredTimeout : 15000;
