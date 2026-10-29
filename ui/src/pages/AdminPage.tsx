import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { api, type AdminBooking } from '../api';

/** Admin area: manage bookings, and edit the Home and Contact text. */
export default function AdminPage() {
  const [tab, setTab] = useState<'bookings' | 'content'>('bookings');
  return (
    <section>
      <h1>Admin</h1>
      <div className="tabs" role="tablist">
        <button role="tab" aria-selected={tab === 'bookings'} data-testid="tab-bookings" onClick={() => setTab('bookings')}>Bookings</button>
        <button role="tab" aria-selected={tab === 'content'} data-testid="tab-content" onClick={() => setTab('content')}>Page content</button>
      </div>
      {tab === 'bookings' ? <BookingsAdmin /> : <ContentAdmin />}
    </section>
  );
}

function BookingsAdmin() {
  const [bookings, setBookings] = useState<AdminBooking[]>([]);
  const [clientName, setClientName] = useState('');
  const [service, setService] = useState('');
  const [startsAt, setStartsAt] = useState('');
  const [notes, setNotes] = useState('');
  const [message, setMessage] = useState('');

  const load = useCallback(() => api.adminBookings().then(setBookings), []);
  useEffect(() => { load().catch((e: Error) => setMessage(e.message)); }, [load]);

  const add = async (e: FormEvent) => {
    e.preventDefault();
    setMessage('');
    try {
      await api.addBooking({ clientName, service, startsAt, notes });
      setClientName(''); setService(''); setStartsAt(''); setNotes('');
      setMessage('Booking added.');
      await load();
    } catch (err) {
      setMessage((err as Error).message);
    }
  };

  const remove = async (id: number) => {
    await api.deleteBooking(id);
    await load();
  };

  return (
    <div>
      <form onSubmit={add} className="card" data-testid="booking-form">
        <h2>Add a booking</h2>
        <label>Client name
          <input data-testid="client-name" value={clientName} onChange={(e) => setClientName(e.target.value)} required />
        </label>
        <label>Service
          <input data-testid="service" value={service} onChange={(e) => setService(e.target.value)} required />
        </label>
        <label>Date and time
          <input data-testid="starts-at" type="datetime-local" value={startsAt} onChange={(e) => setStartsAt(e.target.value)} required />
        </label>
        <label>Notes (private)
          <input data-testid="notes" value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>
        <button type="submit" data-testid="add-booking">Add booking</button>
        {message && <p role="status" data-testid="booking-message">{message}</p>}
      </form>

      <h2>All bookings</h2>
      <table data-testid="admin-booking-table">
        <thead><tr><th>When</th><th>Client</th><th>Service</th><th>Notes</th><th></th></tr></thead>
        <tbody>
          {bookings.map((b) => (
            <tr key={b.id} data-testid="admin-booking-row">
              <td>{new Date(b.startsAt).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' })}</td>
              <td>{b.clientName}</td>
              <td>{b.service}</td>
              <td>{b.notes}</td>
              <td><button className="danger" data-testid="delete-booking" onClick={() => remove(b.id)}>Delete</button></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function ContentAdmin() {
  const [pageKey, setPageKey] = useState<'home' | 'contact'>('home');
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [message, setMessage] = useState('');

  useEffect(() => {
    setMessage('');
    api.content(pageKey).then((c) => { setTitle(c.title); setBody(c.body); }).catch((e: Error) => setMessage(e.message));
  }, [pageKey]);

  const save = async (e: FormEvent) => {
    e.preventDefault();
    try {
      await api.saveContent(pageKey, title, body);
      setMessage('Saved.');
    } catch (err) {
      setMessage((err as Error).message);
    }
  };

  return (
    <form onSubmit={save} className="card" data-testid="content-form">
      <h2>Edit page text</h2>
      <label>Page
        <select data-testid="content-page" value={pageKey} onChange={(e) => setPageKey(e.target.value as 'home' | 'contact')}>
          <option value="home">Home</option>
          <option value="contact">Contact</option>
        </select>
      </label>
      <label>Title
        <input data-testid="content-title" value={title} onChange={(e) => setTitle(e.target.value)} required />
      </label>
      <label>Text (leave a blank line between paragraphs)
        <textarea data-testid="content-body" rows={8} value={body} onChange={(e) => setBody(e.target.value)} />
      </label>
      <button type="submit" data-testid="save-content">Save</button>
      {message && <p role="status" data-testid="content-message">{message}</p>}
    </form>
  );
}
