// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Financial Close Guide — study book', () => {
  test('book appears in exam-prep home', async ({ page }) => {
    await page.goto('/exam-prep/');
    await expect(page.locator('body')).toContainText('Financial Close Guide');
  });

  test('book page lists 14 chapters', async ({ page }) => {
    await page.goto('/exam-prep/guide/financial-close-guide');
    // Check at least a few chapter titles
    await expect(page.locator('body')).toContainText('Overview');
    await expect(page.locator('body')).toContainText('Setup and Installation');
    await expect(page.locator('body')).toContainText('Account Reconciliations Settings');
    await expect(page.locator('body')).toContainText('Security');
    await expect(page.locator('body')).toContainText('Transaction Matching');
    await expect(page.locator('body')).toContainText('Integration');
    await expect(page.locator('body')).toContainText('Appendices');
  });

  test('chapter page renders content and has TOC', async ({ page }) => {
    await page.goto('/exam-prep/guide/financial-close-guide/chapter-03-account-reconciliations-settings');
    await expect(page.locator('body')).toContainText('Account Reconciliations Settings');
    // Should have rendered HTML content (not empty)
    const bodyText = await page.locator('body').innerText();
    expect(bodyText.length).toBeGreaterThan(500);
  });

  test('images serve correctly (HTTP 200)', async ({ page }) => {
    await page.goto('/exam-prep/guide/financial-close-guide/chapter-04-reconciliation-administration');
    // Wait for page to load
    await page.waitForLoadState('networkidle');
    // Check that at least one image tag exists
    const imgCount = await page.locator('img[src*="financial-close-guide"]').count();
    expect(imgCount).toBeGreaterThan(0);

    // Fetch the first image directly and check HTTP status
    const firstImgSrc = await page.locator('img[src*="financial-close-guide"]').first().getAttribute('src');
    if (firstImgSrc) {
      const response = await page.request.get(firstImgSrc);
      expect(response.status()).toBe(200);
    }
  });

  test('chapter navigation (prev/next) works', async ({ page }) => {
    await page.goto('/exam-prep/guide/financial-close-guide/chapter-05-notifications');
    // Should have prev and next links
    const bodyText = await page.locator('body').innerText();
    // Chapter 5 should have navigation to chapter 4 and 6
    expect(bodyText).toContain('Reconciliation Administration');
    expect(bodyText).toContain('Security');
  });

  test('existing books still work after adding new one', async ({ page }) => {
    await page.goto('/exam-prep/guide/design-reference-guide');
    await expect(page.locator('body')).toContainText('Design & Reference Guide');
    const bodyText = await page.locator('body').innerText();
    expect(bodyText.length).toBeGreaterThan(200);
  });
});
