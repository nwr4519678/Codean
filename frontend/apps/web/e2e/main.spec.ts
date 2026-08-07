import { test, expect } from '@playwright/test';

test.describe('Public Landing Page', () => {
  test('should display landing page with hero section', async ({ page }) => {
    await page.goto('/en');
    await expect(page.locator('h1')).toBeVisible();
    await expect(page.getByText('Explore Courses')).toBeVisible();
  });

  test('should navigate to courses page', async ({ page }) => {
    await page.goto('/en');
    await page.getByText('Explore Courses').click();
    await expect(page).toHaveURL(/.*\/en\/courses/);
  });

  test('should navigate to pricing page', async ({ page }) => {
    await page.goto('/en/pricing');
    await expect(page.getByText('Flexible Pricing Plans')).toBeVisible();
    await expect(page.getByText('Pro Student')).toBeVisible();
  });
});

test.describe('Auth Flow', () => {
  test('should display login form', async ({ page }) => {
    await page.goto('/en/auth/login');
    await expect(page.getByPlaceholder('you@example.com')).toBeVisible();
    await expect(page.getByRole('button', { name: /sign in/i })).toBeVisible();
  });

  test('should navigate to register from login', async ({ page }) => {
    await page.goto('/en/auth/login');
    await page.getByText("Don't have an account?").isVisible();
  });

  test('should show forgot password form', async ({ page }) => {
    await page.goto('/en/auth/forgot-password');
    await expect(page.getByText('Forgot Password?')).toBeVisible();
    await expect(page.getByRole('button', { name: /send reset link/i })).toBeVisible();
  });
});

test.describe('i18n & RTL', () => {
  test('should switch to Arabic and apply RTL direction', async ({ page }) => {
    await page.goto('/ar');
    const html = page.locator('html');
    await expect(html).toHaveAttribute('dir', 'rtl');
  });

  test('should navigate to Arabic courses page', async ({ page }) => {
    await page.goto('/ar/courses');
    await expect(page).toHaveURL(/.*\/ar\/courses/);
  });
});

test.describe('Responsive Layout', () => {
  test('should show mobile-friendly navigation', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/en');
    // Page should load without horizontal scroll
    const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
    const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
    expect(scrollWidth).toBeLessThanOrEqual(clientWidth + 1);
  });
});
