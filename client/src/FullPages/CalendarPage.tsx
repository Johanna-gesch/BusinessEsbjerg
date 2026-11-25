import {useNavigate} from "react-router-dom";
import {SharedCalendar} from "@/Components/CalendarComponents/SharedCalendar.tsx";
import {HolidayRequestForm} from "@/Components/RequestComponents/HolidayRequestForm.tsx";

/** Shows the shared calendar */
export function CalendarPage() {
    const navigate = useNavigate();

    return (
        <div>
            <h1>Shared calendar</h1>

            <button onClick={() => navigate ("/dashboard")}>
                Back to Dashboard
            </button>

            <SharedCalendar/>

            <HolidayRequestForm />
        </div>
    )
}