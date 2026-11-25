import apiClient from "@/services/ApiClient.ts";
import {SharedCalendarDayStatus, type SharedCalendarEmployeeDto} from "@/api/Api.ts";
import {useEffect, useState} from "react";


const MyApi = apiClient.api;

// Builds yyyy-MM-dd by hand, so time zones can not move the date
function toIsoDate(year: number, month: number, day: number):string{
    return `${year}-${String(month + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
}

function statusColor(status: SharedCalendarDayStatus): string {
    switch (status) {
        case SharedCalendarDayStatus.Working:
            return "#a5d6a7";
        case SharedCalendarDayStatus.Holiday:
            return "#ffb74d";
        case SharedCalendarDayStatus.Off:
            return "#eeeeee";
        case SharedCalendarDayStatus.PublicHoliday:
            return "#90caf9";
    }
}

function statusText(status: SharedCalendarDayStatus): string {
    switch (status) {
        case SharedCalendarDayStatus.Working:
            return "At work";
        case SharedCalendarDayStatus.Holiday:
            return "On holiday";
        case SharedCalendarDayStatus.Off:
            return "Not working";
        case SharedCalendarDayStatus.PublicHoliday:
            return "Public Holiday";
    }
}
/**
 * Month view of the shared calendar. One row per employee and one color
 * cell per day. The arrows change month, and only the month on screen is fetched.
 */
export function SharedCalendar(){
    const today = new Date();
    const [year, setYear] = useState(today.getFullYear());
    const [month, setMonth] = useState(today.getMonth());
    const [employees, setEmployees] = useState<SharedCalendarEmployeeDto[]>([]);
    const [error, setError] = useState("");

    const daysInMonth = new Date(year, month +1, 0).getDate();

    // Fetches the calendar for the month on screen, again every time the month changes.
    useEffect(() => {
        let active = true;
        setError("");

        MyApi.getSharedCalendar
            .sharedCalendarGetSharedCalendar({
                start: toIsoDate(year, month, 1),
                end: toIsoDate(year, month, daysInMonth),
            })
        .then(result => {
            if(active) setEmployees(result);
        })
            .catch(e => {
                console.error("Could not load the shared calendar:", e);
                if(active) setError("Could not load the calendar");
            });
        return () => {
            active = false;
        };
    }, [year, month, daysInMonth]);

    function changeMonth(step: number){
        const date = new Date(year, month + step, 1);
        setYear(date.getFullYear());
        setMonth(date.getMonth());
    }

    // every employee has the same dates, so the header is taken from the first one.
    const headerDays = employees[0]?.days ?? [];

    const monthTitle = new Date(year, month, 1).toLocaleDateString("da-Dk", {
        month: "long",
        year: "numeric",
    });

    return (
        <div>
            <div>
                <button onClick={() => changeMonth(-1)}>&lt;</button>
                <strong style={{margin: "0 12px"}}>{monthTitle}</strong>
                <button onClick={() => changeMonth(1)}>&gt;</button>
            </div>

            <p>
                <span style={{background: statusColor(SharedCalendarDayStatus.Working), padding: "0 8px"}}>&nbsp;</span> At work{" "}
                <span style={{background: statusColor(SharedCalendarDayStatus.Holiday), padding: "0 8px"}}>&nbsp;</span> On holiday{" "}
                <span style={{background: statusColor(SharedCalendarDayStatus.Off), padding: "0 8px"}}>&nbsp;</span> Not working {" "}
                <span style={{background: statusColor(SharedCalendarDayStatus.PublicHoliday), padding: "0 8px"}}>&nbsp;</span> Public holiday{" "}
            </p>

            {error && <p>{error}</p>}
            {!error && employees.length === 0 && <p>No employees to show.</p>}

            {employees.length > 0 && (
                <table style={{borderCollapse: "collapse"}}>
                    <thead>
                    <tr>
                        <th style={{textAlign: "left", paddingRight: 12}}>Employee</th>
                        {headerDays.map(day => (
                            <th key={day.date} style={{width: 24, fontWeight: "normal"}}>
                                {Number(day.date.slice(8, 10))}
                            </th>
                        ))}
                    </tr>
                    </thead>
                    <tbody>
                    {employees.map(employee => (
                        <tr key={employee.userId}>
                            <td style={{paddingRight: 12}}>{employee.username}</td>
                            {employee.days.map(day => (
                                <td
                                    key={day.date}
                                    title={`${day.date}: ${statusText(day.status)}`}
                                    style={{
                                        background: statusColor(day.status),
                                        border: "1px solid #ffffff",
                                        height: 24,
                                    }}
                                />
                            ))}
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}