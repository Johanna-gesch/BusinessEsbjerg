import { test, expect, type Page } from '@playwright/test';

test.describe('User management', () => {

    // Helper method: Creates a user through the frontend
    async function createUser(page: Page, username: string) {
        await page.goto('/');

        // Log alle requests fra browseren
        page.on('request', request => {
            console.log(
                'REQUEST:',
                request.method(),
                request.url()
            );
        });

        // Log alle responses fra serveren
        page.on('response', response => {
            console.log(
                'RESPONSE:',
                response.status(),
                response.url()
            );
        });

        page.on('response', async response => {
            if (response.url().includes('/GetUsers')) {
                console.log(
                    'GET USERS RESPONSE:',
                    await response.text()
                );
            }
        });

        await page.getByPlaceholder('username').fill(username);
        await page.getByPlaceholder('password').fill('TestPassword123');

        await page.getByRole('button', { name: 'Save' }).click();

        await expect(page.getByPlaceholder('username')).toHaveValue('');
    }

    test('should create a new user', async ({ page }) => {
        const username = `e2e-user-${Date.now()}`;

        await createUser(page, username);

        // Reload because UserList does not automatically refresh
        // after creating a user.
        await page.reload();

        await expect(
            page.getByText(username, { exact: true })
        ).toBeVisible();
    });


    test('should create an admin user', async ({ page }) => {
        const username = `e2e-admin-${Date.now()}`;

        await page.goto('/');

        // Listen for the API request
        const requestPromise = page.waitForRequest(request =>
            request.method() === 'POST' &&
            (request.postData()?.includes(username) ?? false)
        );

        await page.getByPlaceholder('username').fill(username);
        await page.getByPlaceholder('password').fill('TestPassword123');

        await page.locator('select').selectOption('admin');

        await page.getByRole('button', { name: 'Save' }).click();

        const request = await requestPromise;

        // Verify that the frontend sends isAdmin = true
        const requestBody = JSON.parse(request.postData()!);

        expect(requestBody.isAdmin).toBe(true);

        await expect(page.getByPlaceholder('username')).toHaveValue('');
    });


    test('should update an existing user', async ({ page }) => {
        const username = `e2e-user-${Date.now()}`;
        const updatedUsername = `${username}-updated`;

        // Arrange: Create a user first
        await createUser(page, username);

        // Reload to get the newly created user
        await page.reload();

        // Act: Find the user and click Edit
        const user = page.getByText(username, { exact: true });

        await expect(user).toBeVisible();

        await user.locator('..')
            .getByRole('button', { name: 'Edit' })
            .click();

        // Change username
        await page.locator('input:not([placeholder])')
            .fill(updatedUsername);

        // Save changes
        await page.getByRole('button', { name: 'Save' })
            .first()
            .click();

        // Assert: Updated username is displayed
        await expect(
            page.getByText(updatedUsername, { exact: true })
        ).toBeVisible();
    });

});
