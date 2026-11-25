import { test, expect } from '../MyFixtures';


// BEFORE EACH
test.beforeEach(async ({ helpers }) => {
    await helpers.loginAsAdmin();
    await helpers.goToAdminPage();
});

// LOGIN
test('User can log in', async ({ page }) => {
    page.on('console', msg => {
        console.log('[Browser]', msg.text());
    });
});

// CREATE
test('Admin can create a user', async ({ page, helpers }) => {

    const username = `e2e_create_${Date.now()}`;

    try {

        const createUserForm = page.getByTestId('create-user-form');

        await createUserForm.getByPlaceholder('username').fill(username);
        await createUserForm.getByPlaceholder('password').fill('TestPassword123');
        await createUserForm.getByRole('button', { name: 'Save' }).click();

        const createdUser = page.getByTestId('user-row')
            .filter({ hasText: username });

        await expect(createdUser).toBeVisible();

    } finally {
        // Cleanup
        await helpers.removeUser(username);
    }


});

// READ
test('Admin can read users', async ({ page }) => {

    await expect(
        page.getByTestId('user-row').first()
    ).toBeVisible();

    await expect(
        page.getByTestId('user-row')
    ).not.toHaveCount(0);
});

// UPDATE
test('Admin can update a user', async ({ page, helpers }) => {

    const username = `e2e_update_${Date.now()}`;
    const updatedUsername = `${username}_updated`;

    // Keep track of the current username for cleanup
    let currentUsername = username;

    try {
        // Arrange
        const createUserForm = page.getByTestId('create-user-form');

        await createUserForm.getByPlaceholder('username').fill(username);
        await createUserForm.getByPlaceholder('password').fill('TestPassword123');
        await createUserForm.getByRole('button', { name: 'Save' }).click();

        const rowBeforeEdit = page.getByTestId('user-row')
            .filter({ hasText: username });

        await expect(rowBeforeEdit).toBeVisible();

        // Get user ID
        const userId = await rowBeforeEdit.getAttribute('data-user-id');

        if (!userId) {
            throw new Error('Could not find user ID');
        }

        // Act
        const userRow = page.locator(`[data-user-id="${userId}"]`);

        await userRow.getByRole('button', { name: 'Edit' }).click();

        const usernameInput = page.getByLabel('Username');

        await usernameInput.fill(updatedUsername);

        const editForm = page.getByTestId('edit-user-form');

        await editForm.getByRole('button', { name: 'Save' }).click();

        await expect(
            page.getByTestId('user-row').filter({ hasText: updatedUsername })
        ).toBeVisible();

        // Update cleanup username
        currentUsername = updatedUsername;
    }
    finally {
        // Cleanup
        await helpers.removeUser(currentUsername);
    }
});


// DELETE

test('Admin can delete a user', async ({ page, helpers }) => {

    const username = `e2e_delete_${Date.now()}`;

    page.on('response', response => {
        if (response.request().method() === 'DELETE') {
            console.log(
                'DELETE RESPONSE:',
                response.status(),
                response.url()
            );
        }
    });

    try {
        // Arrange
        // Create a user for the delete test
        const createUserForm = page.getByTestId('create-user-form');

        await createUserForm.getByPlaceholder('username').fill(username);
        await createUserForm.getByPlaceholder('password').fill('TestPassword123');
        await createUserForm.getByRole('button', { name: 'Save' }).click();

        const userRow = page.getByTestId('user-row')
            .filter({ hasText: username });

        await expect(userRow).toBeVisible();

        // Act
        page.once('dialog', dialog => dialog.accept());

        await userRow.getByRole('button', { name: 'Delete' }).click();

        // Assert
        await expect(userRow).toHaveCount(0);
    }
    finally {
        // Cleanup
        await helpers.removeUser(username);
    }
});

