import { test as base, expect } from '@playwright/test';
import {HelperMethods} from "./HelperMethods";


type MyFixtures = {
    helpers: HelperMethods;
};

export const test = base.extend<MyFixtures>({
    helpers: async ({ page }, use) => {
        const helpers = new HelperMethods(page);

        await use(helpers);
    },
});

export { expect };