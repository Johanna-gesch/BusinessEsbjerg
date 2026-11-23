import {useNavigate} from "react-router-dom";
import {SharedCalendar} from "@/Components/CalendarComponents/SharedCalendar.tsx";


export function CalendarPage() {
    const navigate = useNavigate();

    return (
        <div>
            <h1>Shared calendar</h1>

            <button onClick={() => navigate ("/dashboard")}>
                Back to Dashboard
            </button>

            <SharedCalendar/>
        </div>
    )
}