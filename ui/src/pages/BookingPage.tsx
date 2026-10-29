import { useEffect, useState } from 'react';
import { api, type PublicBooking } from '../api';

/** Public, read-only list of upcoming bookings (first name, service and time only). */
export default function BookingPage() {
  const [bookings, setBookings] = useState<PublicBooking[] | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    api.publicBookings().then(setBookings).catch((e: Error) => setError(e.message));
  }, []);

  if (error) return <p role="alert">{error}</p>;
  if (!bookings) return <p>Loading…</p>;

  return (
    <section data-testid="booking-page">
      <h1>Upcoming bookings</h1>
      <p>Here is what is already booked. To book a time, please call or visit us (see Contact).</p>
      {bookings.length === 0 ? (
        <p data-testid="no-bookings">Nothing is booked yet.</p>
      ) : (
        <table data-testid="booking-table">
          <thead>
            <tr><th>When</th><th>Name</th><th>Service</th></tr>
          </thead>
          <tbody>
            {bookings.map((b, i) => (
              <tr key={i} data-testid="booking-row">
                <td>{new Date(b.startsAt).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' })}</td>
                <td>{b.firstName}</td>
                <td>{b.service}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
