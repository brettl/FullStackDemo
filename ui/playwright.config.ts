import { defineConfig, devices } from '@playwright/test';

// The tests run against the real app started by `docker compose up` (UI on port 5300).
// Set BASE_URL to point somewhere else.
export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  fullyParallel: false,   // the tests share one database, so run them one at a time
  workers: 1,
  reporter: [['list']],
  use: {
    baseURL: process.env.BASE_URL ?? 'http://localhost:5300',
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
