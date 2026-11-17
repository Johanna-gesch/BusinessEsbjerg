import {useNavigate} from "react-router-dom";
import {useState} from "react";
import {LogOutButton} from "@/Components/LogOutButtn.tsx";
import {UserRole} from "@/api/Api.ts";
import type {LoggedInUser} from "@/Interfaces/LoggedInUser.tsx";

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

                {user.role == UserRole.Admin && (
                    <button onClick={() => navigate("/admin")}>
                        Admin
                    </button>
                )}
            </header>
        </div>
    )
}