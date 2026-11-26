import {useNavigate} from "react-router-dom";
import { useState} from "react";
import {LogOutButton} from "@/Components/LogOutButtn.tsx";
import { UserRole} from "@/api/Api.ts";
import type {LoggedInUser} from "@/Interfaces/LoggedInUser.tsx";
import {WorkScheduleCard} from "@/Components/WorkScheduleComponents/WorkScheduleCard.tsx";
import {EarningsComponent} from "@/Components/EarningsComponent/EarningsComponent.tsx";
import { AccountList } from "@/Components/AccountComponents/AccountList.tsx";
import {MyHolidayRequests} from "@/Components/RequestComponents/MyHolidayRequests.tsx";
import {TransactionHistory} from "@/Components/TransactionHistory.tsx";

export function Dashboard() {
    const navigate = useNavigate();

    const [user] = useState<LoggedInUser | null>(() => {
        const savedUser = localStorage.getItem("loggedInUser");
        return savedUser ? JSON.parse(savedUser) : null;
    });

    if (!user) {
        return <main className="page-shell"><p className="notice-card">You must be logged in.</p></main>;
    }

    return (
        <main className="page-shell dashboard-page">
            <header className="dashboard-header">
                <h1>HI {user.username}!</h1>

                <nav className="dashboard-actions" aria-label="Main navigation">
                    {user.role === UserRole.Admin && (
                        <button className="button button-secondary" onClick={() => navigate("/admin")}>
                            Admin
                        </button>
                    )}

                    <button className="button button-secondary" onClick={() => navigate("/sharedcalendar")}>
                        Shared Calendar
                    </button>

                    {(user.role === UserRole.Accountant) && (
                        <button className="button button-secondary" onClick={() => navigate("/accountant")}>
                            Accounting
                        </button>
                    )}

                    <LogOutButton />
                </nav>
            </header>

            <WorkScheduleCard userId={user.userId} />

            <EarningsComponent user={user} />

            <section className="dashboard-transactions">
                <h2>Holiday transactions</h2>
                <AccountList userId={user.userId} />

                <TransactionHistory userId={user.userId} />
            </section>

            <MyHolidayRequests />
        </main>
    );
}
