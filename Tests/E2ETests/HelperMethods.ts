
import { expect, type Page } from '@playwright/test';

export class HelperMethods {
    private readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    // LOGIN
    async loginAsAdmin() {
        await this.page.goto('/');

        await this.page.getByPlaceholder('Username').fill('playwright_admin');
        await this.page.getByPlaceholder('Password').fill('PlaywrightPassword123');

        await this.page.getByRole('button', { name: 'Login' }).click();

        // Login takes us to Dashboard
        await expect(this.page).toHaveURL(/dashboard/);

        // Verify that the admin button is visible
        await expect(this.page.getByRole('button', { name: 'Admin' })).toBeVisible();
    }

    // GO TO ADMIN PAGE
    async goToAdminPage() {
        await this.page.getByRole('button', { name: 'Admin' }).click();

        await expect(this.page).toHaveURL(/admin/);
    }

    // CLEANUP
    async removeUser(username: string) {
        const userRow = this.page.getByTestId('user-row')
            .filter({ hasText: username });

        if (await userRow.count() === 0) {
            console.log(`CLEANUP: ${username} does not exist`);
            return;
        }

        console.log(`CLEANUP: Deleting ${username}`);

        // Listen for the DELETE response before clicking
        const deleteResponsePromise = this.page.waitForResponse(
            response => response.request().method() === 'DELETE'
        );

        this.page.once('dialog', async dialog => {
            await dialog.accept();
        });

        await userRow.getByRole('button', { name: 'Delete' }).click();

        const response = await deleteResponsePromise;

        console.log('CLEANUP DELETE STATUS:', response.status());
        console.log('CLEANUP DELETE URL:', response.url());

        if (!response.ok()) {
            console.log('CLEANUP DELETE BODY:', await response.text());
        }

        await expect(userRow).toHaveCount(0);
    }
}
