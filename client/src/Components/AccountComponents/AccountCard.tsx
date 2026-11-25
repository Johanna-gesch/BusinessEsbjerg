import type { HolidayAccount } from "@/api/Api.ts";
import { TransactionHistory } from "@/Components/TransactionHistory.tsx";

type AccountCardProps = {
    account: HolidayAccount;
    userId: string;
};

export function AccountCard({ account, userId}: AccountCardProps) {

    const accountTitle = account.type === 0
        ? "First five holiday weeks"
        : "Sixth holiday week";

    return (
        <article className="surface-card account-card">
            <h3>{accountTitle}</h3>

            <p>
                {account.type === 0
                    ? "Your ordinary holiday account."
                    : "Your additional sixth holiday week account."}
            </p>

            <TransactionHistory
                accountId={account.id}
                userId={userId}
                accountType={account.type}
            />
        </article>
    );
}
