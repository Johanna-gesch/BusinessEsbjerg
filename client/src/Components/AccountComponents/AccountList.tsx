import apiClient from "@/services/ApiClient.ts";
import { useEffect, useState } from "react";
import type { HolidayAccount } from "@/api/Api.ts";
import { AccountCard } from "@/Components/AccountComponents/AccountCard.tsx";

type AccountListProps = {
    userId: string;
};

export function AccountList({ userId }: AccountListProps) {
    const [accounts, setAccounts] = useState<HolidayAccount[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let active = true;

        setLoading(true);
        setError("");

        apiClient.api.getAccountsForUser
            .accountGetAccountsForUser({ userId })
            .then(response => {
                if (active) {
                    setAccounts(response);
                }
            })
            .catch(error => {
                console.error("Failed to retrieve accounts:", error);

                if (active) {
                    setError("Could not get holiday accounts.");
                }
            })
            .finally(() => {
                if (active) {
                    setLoading(false);
                }
            });

        return () => {
            active = false;
        };
    }, [userId]);

    if (loading) {
        return <p>Loading holiday accounts...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    if (accounts.length === 0) {
        return <p>There is no holiday account yet.</p>;
    }

    return (
        <div className="account-list">
            {accounts.map(account => (
                <AccountCard
                    key={account.id}
                    account={account}
                />
            ))}
        </div>
    );
}
