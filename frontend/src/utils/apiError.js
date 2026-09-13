const firstValidationMessage = (errors) => {
  if (!errors || typeof errors !== 'object') return null;

  for (const value of Object.values(errors)) {
    if (Array.isArray(value) && value.length > 0) return String(value[0]);
    if (typeof value === 'string' && value.trim()) return value;
  }

  return null;
};

export function getApiErrorMessage(error, fallback = 'Something went wrong. Please try again.') {
  const response = error?.response;
  const data = response?.data;

  if (!response) {
    if (error?.code === 'ECONNABORTED') return 'The request timed out. Please try again.';
    if (error?.message === 'Network Error') {
      return 'Unable to reach the server. Check your connection and try again.';
    }
    return error?.message || fallback;
  }

  const validationMessage = firstValidationMessage(data?.errors);
  if (validationMessage) return validationMessage;

  if (typeof data?.title === 'string' && data.title.trim()) return data.title;
  if (typeof data?.message === 'string' && data.message.trim()) return data.message;
  if (typeof data?.error === 'string' && data.error.trim()) return data.error;

  switch (response.status) {
    case 400:
      return 'The request was not valid. Please review the information and try again.';
    case 401:
      return 'Your session is no longer valid. Please sign in again.';
    case 403:
      return 'You do not have permission to perform this action.';
    case 404:
      return 'The requested item could not be found.';
    case 409:
      return 'This action conflicts with the current state of the item.';
    case 422:
      return 'Some information could not be processed. Please review the form and try again.';
    case 429:
      return 'Too many requests. Please wait a moment and try again.';
    default:
      if (response.status >= 500) return 'The server could not complete the request. Please try again.';
      return fallback;
  }
}
