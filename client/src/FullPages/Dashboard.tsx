import {useNavigate} from "react-router-dom";
import { useState} from "react";
import {LogOutButton} from "@/Components/LogOutButtn.tsx";
import { UserRole} from "@/api/Api.ts";
import type {LoggedInUser} from "@/Interfaces/LoggedInUser.tsx";
import {WorkScheduleCard} from "@/Components/WorkScheduleComponents/WorkScheduleCard.tsx";
import {EarningsComponent} from "@/Components/EarningsComponent/EarningsComponent.tsx";
import { AccountList } from "@/Components/AccountComponents/AccountList.tsx";

export function Dashboard() {
    const navigate = useNavigate();

    const [user] = useState<LoggedInUser | null>(() => {
        const savedUser = localStorage.getItem("loggedInUser");
        return savedUser ? JSON.parse(savedUser) : null;
    });

    if (!user) {
        return <p>You must be logged in.</p>;
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

                <button onClick={() => navigate("/sharedcalendar")}>
                    Shared Calendar
                </button>
            </header>

            <WorkScheduleCard userId={user.userId} />

            <EarningsComponent user={user} />

            <h2>Holiday transactions</h2>

            <AccountList userId={user.userId} />
        </div>
    );
}