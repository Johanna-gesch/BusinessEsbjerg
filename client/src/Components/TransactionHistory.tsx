
import {
    type HolidayTransaction,
    HolidayTransactionType,
    HolidayType
} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";
import {useEffect, useState} from "react";

type TransactionHistoryProps = {
    accountId: string;
    userId: string;
    accountType: HolidayType;
};

const MyApi = apiClient.api;

function getTypeName(type: HolidayTransactionType): string {
    switch (type) {
        case HolidayTransactionType.Earned:
            return "Earned";
        case HolidayTransactionType.Used:
            return "Used";
        case HolidayTransactionType.TransferredIn:
            return "TransferredIn";
        case HolidayTransactionType.TransferredOut:
            return "TransferredOut";
        case HolidayTransactionType.PaidOut:
            return "PaidOut";
        case HolidayTransactionType.Adjustment:
            return "Adjustment";
    }
}

function formatDate(date: string): string {
    return date.split("-").reverse().join("-");
}

function formatHours(hours: number): string {
    return new Intl.NumberFormat("da-DK", {
        maximumFractionDigits: 2,
    }).format(hours);
}

export function TransactionHistory({
                                       accountId,
                                       userId,
                                       accountType
                                   }: TransactionHistoryProps) {
    const [transactions, setTransactions] = useState<HolidayTransaction[]>([]);
    const [accruedHours, setAccruedHours] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let active = true;

        setLoading(true);
        setError("");

        Promise.all([
            MyApi.getTransactionHistory
                .transactionGetTransactionHistory({accountId}),

            MyApi.getEarningsForUser
                .earningGetEarningsForUser({userId})
        ])
            .then(([transactionResult, earningsResult]) => {
                if (!active) return;

                setTransactions(transactionResult);

                const earnedHours =
                    accountType === HolidayType.Week15
                        ? earningsResult.availableWeek1_5Hours
                        : earningsResult.availableWeek6Hours;

                setAccruedHours(earnedHours);
            })
            .catch(error => {
                console.error("Could not load transaction history:", error);

                if (active) {
                    setError("Kunne ikke hente transaktionerne.");
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
    }, [accountId, userId, accountType]);

    if (loading) {
        return <p>Loading Transactions...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <section>
            <h2>TransactionHistory</h2>

            <p>Total accrued: {formatHours(accruedHours)} timer</p>

            {transactions.length === 0 ? (
                <p>There are no transactions yet.</p>
            ) : (
                <table>
                    <thead>
                    <tr>
                        <th>Date</th>
                        <th>Type</th>
                        <th>Description</th>
                        <th>Hours</th>
                    </tr>
                    </thead>
                    <tbody>
                    {transactions.map(transaction => (
                        <tr key={transaction.id}>
                            <td>{formatDate(transaction.transactionDate)}</td>
                            <td>{getTypeName(transaction.transactionType)}</td>
                            <td>{transaction.description ?? ""}</td>
                            <td>{formatHours(transaction.amountHours)}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}
        </section>
    );
}
