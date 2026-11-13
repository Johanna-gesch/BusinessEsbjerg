import {useNavigate} from "react-router-dom";
import {useState} from "react";
import {SalaryHistory} from "@/Components/SalaryHistory.tsx";

interface LoggedInUser {
    userId: string;
    username: string;
    isAdmin: boolean;
}

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
            <header className="dashboard-header">
                <h1>HI {user.username}!</h1>

                {user.isAdmin && (
                    <button onClick={() => navigate("/admin")}>
                        Admin
                    </button>
                )}
            </header>
            <SalaryHistory userId={user.userId} />
        </div>
    )
}