import {useNavigate} from "react-router-dom";
import {useEffect, useState} from "react";
import {LogOutButton} from "@/Components/LogOutButtn.tsx";
import {type HolidayAccount, HolidayType, UserRole} from "@/api/Api.ts";
import type {LoggedInUser} from "@/Interfaces/LoggedInUser.tsx";
import {WorkScheduleCard} from "@/Components/WorkScheduleComponents/WorkScheduleCard.tsx";
import apiClient from "@/services/ApiClient.ts";
import {TransactionHistory} from "@/Components/TransactionHistory.tsx";

export function Dashboard() {
    const navigate = useNavigate();

    const [user] = useState<LoggedInUser | null>(() => {
        const savedUser = localStorage.getItem("loggedInUser");
        return savedUser ? JSON.parse(savedUser) : null;
    });

    const [accounts, setAccounts] = useState<HolidayAccount []>([]);
    const [loadingAccounts, setLoadingAccounts] = useState(true);
    const[accountsError, setAccountError] = useState("");

    useEffect(() => {
        if (!user) return;

        let active = true

        apiClient.api.getAccountsForUser
            .accountGetAccountsForUser({ userId: user.userId })
            .then((result) => {
                if (active) {
                    setAccounts(result);
                }
            })
        .catch((error) => {
            console.error("Could not load holiday accounts", error);
            if (active) {
                setAccountError("could not get holiday accounts");
            }
        })
            .finally(() => {
                if (active) {
                    setLoadingAccounts(false);
                }
            });
        return () => {
            active = false;
        };

    }, [user?.userId]);
    if(!user){
        return <p>You must be logged in.</p>
    }

    return (
        <div>
            <LogOutButton />

            <header className="dashboard-header">
                <h1>HI {user.username}!</h1>

                {user.role === UserRole.Admin && (
                    <button onClick={() => navigate("/admin")}>
                        Admin
                    </button>
                )}

                <button onClick={()=> navigate("/sharedcalendar")}>
                    Shared Calendar
                </button>

                {(user.role === UserRole.Accountant) && (
                    <button onClick={() => navigate("/accountant")}>
                        Accounting
                    </button>
                )}
            </header>

            <WorkScheduleCard userId={user.userId} />

            <h2>Holiday transactions</h2>

            {loadingAccounts && <p>Loading holiday account...</p>}
            {accountsError && <p>{accountsError}</p>}

            {!loadingAccounts && !accountsError && accounts.length === 0 && (
                <p>There is no holiday account yet.</p>
            )}

            {accounts.map((account) => (
                <section key={account.id}>
                    <h3>
                        {account.type === HolidayType.Week15
                            ? "Ferieuge 1–5"
                            : "Ferieuge 6"}
                    </h3>

                    <TransactionHistory accountId={account.id} />
                </section>
            ))}
        </div>
    );
}