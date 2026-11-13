import apiClient from "@/services/ApiClient.ts";
import {useEffect, useState} from "react";
import type {HolidayAccount} from "@/api/Api.ts";
import {AccountCard} from "@/Components/AccountComponents/AccountCard.tsx";

const MyApi = apiClient.api;

type AccountListProps = {
    userId: string;
};

export function AccountList({ userId }: AccountListProps) {
    const [accounts, setAccounts] = useState<HolidayAccount[]>([]);

    useEffect(() => {
        MyApi.getAccountsForUser.accountGetAccountsForUser({ userId: userId })
            .then(response => {
                setAccounts(response);
            })
            .catch(error => {
                console.error("Failed to retrieve accounts:", error);
            });
    }, [userId]);

    return (
        <div>
            {accounts.map(account => (
               <AccountCard
                   key={account.id}
                   account={account}
                />
            ))}
        </div>
    );
}