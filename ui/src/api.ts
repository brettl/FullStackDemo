// Small helper around fetch(): one place that knows the API shapes and attaches the login token.

export type Content = { key: string; title: string; body: string };
export type PublicBooking = { firstName: string; service: string; startsAt: string };
export type AdminBooking = { id: number; clientName: string; service: string; startsAt: string; notes: string | null };

const TOKEN_KEY = 'salon-token';

export const getToken = () => localStorage.getItem(TOKEN_KEY);
export const setToken = (token: string | null) =>
  token ? localStorage.setItem(TOKEN_KEY, token) : localStorage.removeItem(TOKEN_KEY);

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  const token = getToken();
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(path, { ...init, headers: { ...headers, ...(init.headers as Record<string, string>) } });
  if (!res.ok) {
    const message = await res.json().then((j) => j.error as string).catch(() => `Request failed (${res.status})`);
    throw new Error(message || `Request failed (${res.status})`);
  }
  return res.status === 204 ? (undefined as T) : res.json();
}

export const api = {
  content: (key: string) => request<Content>(`/api/content/${key}`),
  publicBookings: () => request<PublicBooking[]>('/api/bookings'),
  login: (username: string, password: string) =>
    request<{ token: string; username: string }>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    }),
  adminBookings: () => request<AdminBooking[]>('/api/admin/bookings'),
  addBooking: (b: { clientName: string; service: string; startsAt: string; notes: string }) =>
    request<AdminBooking>('/api/admin/bookings', { method: 'POST', body: JSON.stringify(b) }),
  deleteBooking: (id: number) => request<void>(`/api/admin/bookings/${id}`, { method: 'DELETE' }),
  saveContent: (key: string, title: string, body: string) =>
    request<Content>(`/api/admin/content/${key}`, { method: 'PUT', body: JSON.stringify({ title, body }) }),
};
