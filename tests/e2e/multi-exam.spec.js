// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Exam Selector', () => {
  test('shows all 4 exams', async ({ page }) => {
    await page.goto('/exam-prep/');
    await expect(page.locator('body')).toContainText('OS-102');
    await expect(page.locator('body')).toContainText('OS-201');
    await expect(page.locator('body')).toContainText('OS-300');
    await expect(page.locator('body')).toContainText('OS-301');
  });

  test('shows certification levels', async ({ page }) => {
    await page.goto('/exam-prep/');
    await expect(page.locator('body')).toContainText('Associate');
    await expect(page.locator('body')).toContainText('Specialist');
  });

  test('exam cards link to quiz pages', async ({ page }) => {
    await page.goto('/exam-prep/');
    // Click on OS-102 card
    await page.locator('a[href*="os-102"]').first().click();
    await expect(page).toHaveURL(/os-102\/quiz/);
  });
});

test.describe('OS-102 Exam', () => {
  test('quiz sections page loads with 8 sections', async ({ page }) => {
    await page.goto('/exam-prep/os-102/quiz');
    await expect(page.locator('body')).toContainText('Building Blocks');
    await expect(page.locator('body')).toContainText('Application Properties');
    await expect(page.locator('body')).toContainText('Data Entry');
    await expect(page.locator('body')).toContainText('Security');
  });

  test('quiz section page loads with questions', async ({ page }) => {
    await page.goto('/exam-prep/os-102/quiz/building-blocks');
    await expect(page.locator('body')).toContainText('Question');
    // Should have answer options
    await expect(page.locator('body')).toContainText('A)');
  });

  test('exam simulation page loads', async ({ page }) => {
    await page.goto('/exam-prep/os-102/exam');
    await expect(page.locator('body')).toContainText('OS-102');
  });

  test('progress page loads', async ({ page }) => {
    await page.goto('/exam-prep/os-102/progress');
    await expect(page.locator('body')).toContainText('OS-102');
  });

  test('flashcards page loads', async ({ page }) => {
    await page.goto('/exam-prep/os-102/flashcards');
    await expect(page.locator('body')).toContainText('OS-102');
  });
});

test.describe('OS-300 Exam', () => {
  test('quiz sections page loads with 6 sections', async ({ page }) => {
    await page.goto('/exam-prep/os-300/quiz');
    await expect(page.locator('body')).toContainText('Workspaces');
    await expect(page.locator('body')).toContainText('Dashboard Parameters');
    await expect(page.locator('body')).toContainText('Cube Views');
    await expect(page.locator('body')).toContainText('Spreadsheet');
    await expect(page.locator('body')).toContainText('Dashboard Components');
  });

  test('quiz section page loads', async ({ page }) => {
    await page.goto('/exam-prep/os-300/quiz/workspaces');
    await expect(page.locator('body')).toContainText('Question');
  });

  test('exam simulation page loads', async ({ page }) => {
    await page.goto('/exam-prep/os-300/exam');
    await expect(page.locator('body')).toContainText('OS-300');
  });
});

test.describe('OS-301 Exam', () => {
  test('quiz sections page loads with 4 sections', async ({ page }) => {
    await page.goto('/exam-prep/os-301/quiz');
    await expect(page.locator('body')).toContainText('Reconciliation Control Manager');
    await expect(page.locator('body')).toContainText('Transaction Matching');
    await expect(page.locator('body')).toContainText('Integration');
    await expect(page.locator('body')).toContainText('General Configuration');
  });

  test('quiz section page loads', async ({ page }) => {
    await page.goto('/exam-prep/os-301/quiz/rcm');
    await expect(page.locator('body')).toContainText('Question');
  });

  test('exam simulation page loads', async ({ page }) => {
    await page.goto('/exam-prep/os-301/exam');
    await expect(page.locator('body')).toContainText('OS-301');
  });
});

test.describe('OS-201 backward compatibility', () => {
  test('OS-201 quiz still works at new URL', async ({ page }) => {
    await page.goto('/exam-prep/os-201/quiz');
    await expect(page.locator('body')).toContainText('Cube');
    await expect(page.locator('body')).toContainText('Workflow');
    await expect(page.locator('body')).toContainText('Rules');
  });

  test('old /exam-prep/quiz redirects to os-201', async ({ page }) => {
    const response = await page.request.get('/exam-prep/quiz', { maxRedirects: 0 });
    expect(response.status()).toBe(301);
    expect(response.headers()['location']).toContain('/exam-prep/os-201/quiz');
  });

  test('old /exam-prep/exam redirects to os-201', async ({ page }) => {
    const response = await page.request.get('/exam-prep/exam', { maxRedirects: 0 });
    expect(response.status()).toBe(301);
    expect(response.headers()['location']).toContain('/exam-prep/os-201/exam');
  });

  test('old /exam-prep/progress redirects to os-201', async ({ page }) => {
    const response = await page.request.get('/exam-prep/progress', { maxRedirects: 0 });
    expect(response.status()).toBe(301);
    expect(response.headers()['location']).toContain('/exam-prep/os-201/progress');
  });
});

test.describe('Financial Close Guide as study book', () => {
  test('book listed in exam-prep guide', async ({ page }) => {
    await page.goto('/exam-prep/guide/');
    await expect(page.locator('body')).toContainText('Financial Close Guide');
  });

  test('chapter loads with content', async ({ page }) => {
    await page.goto('/exam-prep/guide/financial-close-guide/chapter-06-security');
    await expect(page.locator('body')).toContainText('Security');
    const bodyText = await page.locator('body').innerText();
    expect(bodyText.length).toBeGreaterThan(500);
  });
});
