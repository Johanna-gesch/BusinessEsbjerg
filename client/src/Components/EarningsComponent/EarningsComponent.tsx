
import {useEffect, useState} from "react";
import apiClient from "@/services/ApiClient.ts";
import type {UserEarningsDto, HolidayAccount} from "@/api/Api.ts";
import {HolidayType} from "@/api/Api.ts";
import type {LoggedInUser} from "@/Interfaces/LoggedInUser.tsx";

type EarningsProps = {
    user: LoggedInUser;
};

type AvailableBalances = {
    week1_5Hours: number;
    week6Hours: number;
};

function getLocalDate(): string {
    const today = new Date();

    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, "0");
    const day = String(today.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}

export function EarningsComponent({user}: EarningsProps) {

    const [earnings, setEarnings] = useState<UserEarningsDto | null>(null);
    const [balances, setBalances] = useState<AvailableBalances | null>(null);
    const [loadingEarnings, setLoadingEarnings] = useState(true);
    const [earningsError, setEarningsError] = useState("");

    useEffect(() => {
        let active = true;

        const api = apiClient.api;
        const date = getLocalDate();

        Promise.all([
            api.getEarningsForUser
                .earningGetEarningsForUser({userId: user.userId}),

            api.getAccountsForUser
                .accountGetAccountsForUser({userId: user.userId})
        ])
            .then(async ([earningsResult, accounts]: [UserEarningsDto, HolidayAccount[]]) => {

                const balanceResults = await Promise.all(
                    accounts.map(async account => ({
                        type: account.type,
                        balance: await api.getAvailableBalance
                            .accountGetAvailableBalance({
                                accountId: account.id,
                                date
                            })
                    }))
                );

                if (!active) return;

                const result: AvailableBalances = {
                    week1_5Hours: 0,
                    week6Hours: 0
                };

                balanceResults.forEach(item => {
                    if (item.type === HolidayType.Week15) {
                        result.week1_5Hours = item.balance;
                    } else if (item.type === HolidayType.Week6) {
                        result.week6Hours = item.balance;
                    }
                });

                setEarnings(earningsResult);
                setBalances(result);
            })
            .catch(error => {
                console.error("Could not load holiday balance", error);

                if (active) {
                    setEarningsError("Could not load holiday balance");
                }
            })
            .finally(() => {
                if (active) {
                    setLoadingEarnings(false);
                }
            });

        return () => {
            active = false;
        };
    }, [user.userId]);

    return (
        <div className="earnings-section">
            <section className="holiday-balance">
                <h2>Your available holidays</h2>

                {loadingEarnings && <p className="empty-state">Loading holiday balance...</p>}

                {earningsError && <p className="form-message form-message-error">{earningsError}</p>}

                {!loadingEarnings && !earningsError && earnings && balances && (
                    <div className="holiday-balance-grid">

                        <div className="holiday-balance-card">
                            <h3>Holiday weeks 1–5</h3>

                            <p className="holiday-days">
                                {earnings.availableWeek1_5Days > 0
                                    ? (
                                        balances.week1_5Hours /
                                        (earnings.availableWeek1_5Hours / earnings.availableWeek1_5Days)
                                    ).toFixed(2)
                                    : "0.00"}
                                <span> days</span>
                            </p>

                            <p>Corresponding to:</p>

                            <p className="holiday-hours">
                                {balances.week1_5Hours.toFixed(2)} hours
                            </p>
                        </div>

                        <div className="holiday-balance-card">
                            <h3>Holiday week 6</h3>

                            <p className="holiday-days">
                                {earnings.availableWeek6Days > 0
                                    ? (
                                        balances.week6Hours /
                                        (earnings.availableWeek6Hours / earnings.availableWeek6Days)
                                    ).toFixed(2)
                                    : "0.00"}
                                <span> days</span>
                            </p>

                            <p>Corresponding to:</p>

                            <p className="holiday-hours">
                                {balances.week6Hours.toFixed(2)} hours
                            </p>
                        </div>

                    </div>
                )}
            </section>
        </div>
    );
}