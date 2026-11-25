import {useNavigate} from "react-router-dom";
import {SharedCalendar} from "@/Components/CalendarComponents/SharedCalendar.tsx";

/** Shows the shared calendar */
export function CalendarPage() {
    const navigate = useNavigate();

    return (
        <main className="page-shell calendar-page">
            <header className="page-header">
                <div>
                    <p className="eyebrow">People and time off</p>
                    <h1>Shared calendar</h1>
                </div>
                <button className="button button-secondary" onClick={() => navigate ("/dashboard")}>
                Back to Dashboard
                </button>
            </header>

            <SharedCalendar/>
        </main>
    )
}
