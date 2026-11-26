import type { HolidayAccount } from "@/api/Api.ts";
import { TransactionHistory } from "@/Components/TransactionHistory.tsx";

type AccountCardProps = {
    account: HolidayAccount;
};

export function AccountCard({ account }: AccountCardProps) {

    const accountTitle = account.type === 0
        ? "First five holiday weeks"
        : "Sixth holiday week";

    const accountDescription = account.type === 0
        ? "Your ordinary holiday account."
        : "Your additional sixth holiday week account.";

    return (
        <article className="surface-card account-card">
            <h3>{accountTitle}</h3>
            <p>{accountDescription}</p>
        </article>
    );
}
