import { useEffect, useState } from 'react';
import { api, type Content } from '../api';

/** Shows the Home or Contact page. The text comes from the database through the API. */
export default function ContentPage({ pageKey }: { pageKey: 'home' | 'contact' }) {
  const [content, setContent] = useState<Content | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    setContent(null);
    api.content(pageKey).then(setContent).catch((e: Error) => setError(e.message));
  }, [pageKey]);

  if (error) return <p role="alert">{error}</p>;
  if (!content) return <p>Loading…</p>;

  return (
    <article data-testid={`${pageKey}-page`}>
      <h1 data-testid="page-title">{content.title}</h1>
      {content.body.split(/\n\s*\n/).map((paragraph, i) => (
        <p key={i} style={{ whiteSpace: 'pre-line' }}>{paragraph}</p>
      ))}
    </article>
  );
}
