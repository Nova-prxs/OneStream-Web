// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Smoke tests — container is alive', () => {
  test('portal landing page loads', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/OneStream/i);
  });

  test('exam-prep home lists study books', async ({ page }) => {
    await page.goto('/exam-prep/');
    await expect(page.locator('body')).toContainText('Design & Reference Guide');
  });

  test('existing OS-201 quiz sections page loads', async ({ page }) => {
    await page.goto('/exam-prep/quiz');
    await expect(page.locator('body')).toContainText('Cube');
    await expect(page.locator('body')).toContainText('Workflow');
  });
});
