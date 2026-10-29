import { test, expect, type Page } from '@playwright/test';

// Demo login created by the API on first start (see docker-compose.yml).
const ADMIN_USER = 'admin';
const ADMIN_PASSWORD = 'Salon123!';

async function login(page: Page, password = ADMIN_PASSWORD) {
  await page.goto('/login');
  await page.getByTestId('username').fill(ADMIN_USER);
  await page.getByTestId('password').fill(password);
  await page.getByTestId('login-submit').click();
}

test.describe('public pages', () => {
  test('home page shows the salon text from the database', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByTestId('home-page')).toBeVisible();
    await expect(page.getByTestId('page-title')).not.toBeEmpty();
  });

  test('contact page loads', async ({ page }) => {
    await page.goto('/contact');
    await expect(page.getByTestId('contact-page')).toBeVisible();
  });

  test('navigation links go to the right pages', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: 'Contact' }).click();
    await expect(page).toHaveURL(/\/contact$/);
    await page.getByRole('link', { name: 'Booking' }).click();
    await expect(page).toHaveURL(/\/booking$/);
    await expect(page.getByTestId('booking-page')).toBeVisible();
  });

  test('booking page lists bookings with first names only and no private notes', async ({ page }) => {
    await page.goto('/booking');
    await expect(page.getByTestId('booking-row').first()).toBeVisible();
    await expect(page.getByTestId('booking-page')).not.toContainText('Smith');          // surname hidden
    await expect(page.getByTestId('booking-page')).not.toContainText('Prefers a quiet'); // private note hidden
  });
});

test.describe('admin area', () => {
  test('admin page redirects to the login page when not logged in', async ({ page }) => {
    await page.goto('/admin');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('wrong password shows an error', async ({ page }) => {
    await login(page, 'not-the-password');
    await expect(page.getByTestId('login-error')).toBeVisible();
    await expect(page).toHaveURL(/\/login$/);
  });

  test('correct login opens the admin page and logout closes it', async ({ page }) => {
    await login(page);
    await expect(page).toHaveURL(/\/admin$/);
    await expect(page.getByTestId('booking-form')).toBeVisible();
    await page.getByTestId('logout').click();
    await expect(page).toHaveURL('/');
    await page.goto('/admin');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('a booking added by the admin appears on the public Booking page', async ({ page }) => {
    const unique = `Zelda${Date.now()}`;
    await login(page);
    await page.getByTestId('client-name').fill(`${unique} Tester`);
    await page.getByTestId('service').fill('Test colour');
    await page.getByTestId('starts-at').fill('2099-01-15T10:30');
    await page.getByTestId('notes').fill('private demo note');
    await page.getByTestId('add-booking').click();
    await expect(page.getByTestId('booking-message')).toHaveText('Booking added.');
    await expect(page.getByTestId('admin-booking-table')).toContainText(`${unique} Tester`);

    await page.goto('/booking');
    await expect(page.getByTestId('booking-table')).toContainText(unique);            // first name shown
    await expect(page.getByTestId('booking-table')).not.toContainText('Tester');      // surname hidden

    // clean up: delete the booking again so the demo data stays tidy
    await page.goto('/admin');
    const row = page.getByTestId('admin-booking-row').filter({ hasText: unique });
    await row.getByTestId('delete-booking').click();
    await expect(row).toHaveCount(0);
  });

  test('an admin can delete a booking', async ({ page }) => {
    const unique = `Delete${Date.now()}`;
    await login(page);
    await page.getByTestId('client-name').fill(`${unique} Person`);
    await page.getByTestId('service').fill('To be deleted');
    await page.getByTestId('starts-at').fill('2099-02-01T09:00');
    await page.getByTestId('add-booking').click();
    const row = page.getByTestId('admin-booking-row').filter({ hasText: unique });
    await expect(row).toBeVisible();

    await row.getByTestId('delete-booking').click();
    await expect(row).toHaveCount(0);
  });

  test('editing the Home page text changes the public Home page', async ({ page }) => {
    const newTitle = `Welcome ${Date.now()}`;
    await login(page);
    await page.getByTestId('tab-content').click();
    await page.getByTestId('content-page').selectOption('home');
    const originalTitle = await page.getByTestId('content-title').inputValue();
    await page.getByTestId('content-title').fill(newTitle);
    await page.getByTestId('save-content').click();
    await expect(page.getByTestId('content-message')).toHaveText('Saved.');

    await page.goto('/');
    await expect(page.getByTestId('page-title')).toHaveText(newTitle);

    // put the original title back so the demo data stays tidy
    await page.goto('/login');
    await login(page);
    await page.getByTestId('tab-content').click();
    await page.getByTestId('content-title').fill(originalTitle);
    await page.getByTestId('save-content').click();
    await expect(page.getByTestId('content-message')).toHaveText('Saved.');
  });

  test('editing the Contact page text changes the public Contact page', async ({ page }) => {
    await login(page);
    await page.getByTestId('tab-content').click();
    await page.getByTestId('content-page').selectOption('contact');
    await expect(page.getByTestId('content-title')).not.toHaveValue('');
    const originalBody = await page.getByTestId('content-body').inputValue();
    const marker = `Updated hours ${Date.now()}`;
    await page.getByTestId('content-body').fill(`${originalBody}\n\n${marker}`);
    await page.getByTestId('save-content').click();
    await expect(page.getByTestId('content-message')).toHaveText('Saved.');

    await page.goto('/contact');
    await expect(page.getByTestId('contact-page')).toContainText(marker);

    // restore
    await page.goto('/login');
    await login(page);
    await page.getByTestId('tab-content').click();
    await page.getByTestId('content-page').selectOption('contact');
    await page.getByTestId('content-body').fill(originalBody);
    await page.getByTestId('save-content').click();
    await expect(page.getByTestId('content-message')).toHaveText('Saved.');
  });

  test('the admin API rejects requests without a login token', async ({ request }) => {
    const res = await request.get('/api/admin/bookings');
    expect(res.status()).toBe(401);
  });
});
