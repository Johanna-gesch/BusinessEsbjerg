
import { test, expect, type Page } from '@playwright/test';

// HELPER METHOD
async function loginAsAdmin(page: Page) {

    await page.goto('/');

    await page.getByPlaceholder('Username').fill('playwright_admin');
    await page.getByPlaceholder('Password').fill('PlaywrightPassword123');

    await page.getByRole('button', { name: 'Login' }).click();

    await expect(page).toHaveURL(/admin/);
}

// LOGIN
test('User can log in', async ({ page }) => {
    await loginAsAdmin(page);
    page.on('console', msg => {
        console.log('[Browser]', msg.text());
    });
});

// CREATE
test('Admin can create a user', async ({ page }) => {
    await loginAsAdmin(page);

    const username = `e2e_create_${Date.now()}`;

    await page.getByPlaceholder('username').fill(username);
    await page.getByPlaceholder('password').fill('TestPassword123');

    await page.getByRole('button', { name: 'Save' }).click();

    const createdUser = page.getByTestId('user-row')
        .filter({ hasText: username });

    await expect(createdUser).toBeVisible();
});

// READ
test('Admin can read users', async ({ page }) => {
    await loginAsAdmin(page);

    await expect(
        page.getByTestId('user-row').first()
    ).toBeVisible();

    await expect(
        page.getByTestId('user-row')
    ).not.toHaveCount(0);
});

// UPDATE
test('Admin can update a user', async ({ page }) => {
    await loginAsAdmin(page);

    const username = `e2e_update_${Date.now()}`;
    const updatedUsername = `${username}_updated`;

    await page.getByPlaceholder('username').fill(username);
    await page.getByPlaceholder('password').fill('TestPassword123');
    await page.getByRole('button', { name: 'Save' }).click();

    // Find rækken ud fra brugernavnet
    const rowBeforeEdit = page.getByTestId('user-row')
        .filter({ hasText: username });

    await expect(rowBeforeEdit).toBeVisible();

    // Gem rækkens stabile ID
    const userId = await rowBeforeEdit.getAttribute('data-user-id');

    if (!userId) {
        throw new Error('Could not find user ID');
    }

    // Find nu rækken ud fra ID i stedet for brugernavnet
    const userRow = page.locator(`[data-user-id="${userId}"]`);

    // Klik Edit
    await userRow.getByRole('button', { name: 'Edit' }).click();

    // Find og rediger inputfeltet
    const usernameInput = userRow.locator('input');

    await expect(usernameInput).toBeVisible();
    await usernameInput.fill(updatedUsername);

    // Gem ændringen
    await userRow.getByRole('button', { name: 'Save' }).click();

    // Kontrollér det nye brugernavn
    await expect(
        page.getByTestId('user-row').filter({ hasText: updatedUsername })
    ).toBeVisible();
});

// DELETE
test('Admin can delete a user', async ({ page }) => {
    await loginAsAdmin(page);

    const username = `e2e_delete_${Date.now()}`;

    // Create a user for the delete test
    await page.getByPlaceholder('username').fill(username);
    await page.getByPlaceholder('password').fill('TestPassword123');
    await page.getByRole('button', { name: 'Save' }).click();

    const userRow = page.getByTestId('user-row')
        .filter({ hasText: username });

    await expect(userRow).toBeVisible();

    // Accept the confirmation dialog
    page.once('dialog', dialog => dialog.accept());

    await userRow.getByRole('button', { name: 'Delete' }).click();

    await expect(userRow).toHaveCount(0);
});
