import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// In dev (`npm run dev`) the /api calls are forwarded to the API container's published port.
export default defineConfig({
  plugins: [react()],
  server: { port: 5173, proxy: { '/api': 'http://localhost:5301' } },
});
