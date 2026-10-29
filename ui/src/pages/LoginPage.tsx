import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, setToken } from '../api';

export default function LoginPage({ onLogin }: { onLogin: () => void }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const result = await api.login(username, password);
      setToken(result.token);
      onLogin();
      navigate('/admin');
    } catch {
      setError('Wrong username or password.');
    }
  };

  return (
    <section>
      <h1>Staff login</h1>
      <form onSubmit={submit} className="card" data-testid="login-form">
        <label>Username
          <input data-testid="username" value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" />
        </label>
        <label>Password
          <input data-testid="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" />
        </label>
        {error && <p role="alert" data-testid="login-error" className="error">{error}</p>}
        <button type="submit" data-testid="login-submit">Log in</button>
      </form>
    </section>
  );
}
