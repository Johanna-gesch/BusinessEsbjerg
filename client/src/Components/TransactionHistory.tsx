import {type HolidayTransaction, HolidayTransactionType} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";
import {useEffect, useState} from "react";


type TransactionHistoryProps = {
    accountId: string;
};

const MyApi = apiClient.api;

function getTypeName(type: HolidayTransactionType): string{
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
                                   }: TransactionHistoryProps) {
    const [transactions, setTransactions] = useState<HolidayTransaction[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let active = true;

        setLoading(true);
        setError("");

        MyApi.getTransactionHistory
            .transactionGetTransactionHistory({ accountId })
            .then((result) => {
                if (active) {
                    setTransactions(result);
                }
            })
            .catch((err) => {
                console.error("Could not load transaction history:", err);
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
    }, [accountId]);

    const balance = transactions.reduce(
        (total, transaction) => total + transaction.amountHours,
        0,
    );

    if (loading) {
        return <p>Loading Transactions...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <section className="transaction-history">
            <h2>Transaction history</h2>

            <p>Balance: {formatHours(balance)} timer</p>

            {transactions.length === 0 ? (
                <p>There is no transactions yet.</p>
            ) : (
                <table className="data-table">
                    <thead>
                    <tr>
                        <th>Date</th>
                        <th>Type</th>
                        <th>Description</th>
                        <th>Hours</th>
                    </tr>
                    </thead>
                    <tbody>
                    {transactions.map((transaction) => (
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
