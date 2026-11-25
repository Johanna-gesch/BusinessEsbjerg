import {useEffect, useState} from "react";
import apiClient from "@/services/ApiClient.ts";
import type {User, UserEarningsDto} from "@/api/Api.ts";
import type {LoggedInUser} from "@/Interfaces/LoggedInUser.tsx";

type EarningsProps = {
    user: LoggedInUser;
};

export function EarningsComponent({user}: EarningsProps) {

    const [earnings, setEarnings] = useState<UserEarningsDto | null>(null);
    const [loadingEarnings, setLoadingEarnings] = useState(true);
    const [earningsError, setEarningsError] = useState("");

    useEffect(() => {
        if (!user) return;

        let active = true;

        apiClient.api.getEarningsForUser
            .earningGetEarningsForUser({ userId: user.userId })
            .then((result) => {
                if (active) {
                    setEarnings(result);
                }
            })
            .catch((error) => {
                console.error("Could not load holiday earnings", error);

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
    }, [user?.userId]);

    return (
        <div>
            <section className="holiday-balance">
                <h2>Your available holidays</h2>

                {loadingEarnings && <p>Loading holiday balance...</p>}

                {earningsError && <p>{earningsError}</p>}

                {!loadingEarnings && !earningsError && earnings && (
                    <div className="holiday-balance-grid">

                        <div className="holiday-balance-card">
                            <h3>Holiday weeks 1–5</h3>

                            <p className="holiday-days">
                                {earnings.availableWeek1_5Days.toFixed(2)}
                                <span> days</span>
                            </p>
                            <p>Corresponding to:</p>
                            <p className="holiday-hours">
                                {earnings.availableWeek1_5Hours.toFixed(2)} hours
                            </p>
                        </div>

                        <div className="holiday-balance-card">
                            <h3>Holiday week 6</h3>

                            <p className="holiday-days">
                                {earnings.availableWeek6Days.toFixed(2)}
                                <span> days</span>
                            </p>
                            <p>Corresponding to:</p>
                            <p className="holiday-hours">
                                {earnings.availableWeek6Hours.toFixed(2)} hours
                            </p>
                        </div>

                    </div>
                )}
            </section>
        </div>
    )
}
