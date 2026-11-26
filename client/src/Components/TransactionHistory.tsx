
import {
    type HolidayTransaction, type HolidayTransactionOverviewDto,
    HolidayTransactionType,
    HolidayType
} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";
import {useEffect, useState} from "react";

type TransactionHistoryProps = {
    userId: string;
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
        default:
            return "Unknown";
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

export function TransactionHistory({ userId }: TransactionHistoryProps) {
    const [transactions, setTransactions] = useState<HolidayTransactionOverviewDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    useEffect(() => {
        let active = true;

        setLoading(true);
        setError("");

        MyApi.getTransactionHistoryForUser
            .transactionGetTransactionHistoryForUser()
            .then(result => {
                if (active) {
                    setTransactions(result);
                }
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
    }, [userId]);

    if (loading) {
        return <p>Loading transactions...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <section className="transaction-history">
            <h2>Transaction history</h2>

            {transactions.length === 0 ? (
                <p>There are no transactions yet.</p>
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