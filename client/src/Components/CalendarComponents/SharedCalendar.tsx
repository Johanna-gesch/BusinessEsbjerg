import apiClient from "@/services/ApiClient.ts";
import {SharedCalendarDayStatus, type SharedCalendarEmployeeDto} from "@/api/Api.ts";
import {useEffect, useState} from "react";


const MyApi = apiClient.api;

// Builds yyyy-MM-dd by hand, so time zones can not move the date
function toIsoDate(year: number, month: number, day: number):string{
    return `${year}-${String(month + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
}

function statusClass(status: SharedCalendarDayStatus): string {
    switch (status) {
        case SharedCalendarDayStatus.Working:
            return "working";
        case SharedCalendarDayStatus.Holiday:
            return "holiday";
        case SharedCalendarDayStatus.Off:
            return "off";
        case SharedCalendarDayStatus.PublicHoliday:
            return "public-holiday";
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
        <section className="surface-card shared-calendar">
            <div className="calendar-toolbar">
                <button className="button button-secondary calendar-nav-button" aria-label="Previous month" onClick={() => changeMonth(-1)}>&lt;</button>
                <strong className="calendar-month-title">{monthTitle}</strong>
                <button className="button button-secondary calendar-nav-button" aria-label="Next month" onClick={() => changeMonth(1)}>&gt;</button>
            </div>

            <div className="calendar-legend">
                <span><i className="calendar-legend-swatch is-working" />At work</span>
                <span><i className="calendar-legend-swatch is-holiday" />On holiday</span>
                <span><i className="calendar-legend-swatch is-off" />Not working</span>
                <span><i className="calendar-legend-swatch is-public-holiday" />Public holiday</span>
            </div>

            {error && <p className="form-message form-message-error">{error}</p>}
            {!error && employees.length === 0 && <p className="empty-state">No employees to show.</p>}

            {employees.length > 0 && (
                <div className="calendar-table-scroll">
                <table className="data-table calendar-table">
                    <thead>
                    <tr>
                        <th className="calendar-employee-heading">Employee</th>
                        {headerDays.map(day => (
                            <th key={day.date}>
                                {Number(day.date.slice(8, 10))}
                            </th>
                        ))}
                    </tr>
                    </thead>
                    <tbody>
                    {employees.map(employee => (
                        <tr key={employee.userId}>
                            <td>{employee.username}</td>
                            {employee.days.map(day => (
                                <td
                                    key={day.date}
                                    title={`${day.date}: ${statusText(day.status)}`}
                                    className={`calendar-day is-${statusClass(day.status)}`}
                                />
                            ))}
                        </tr>
                    ))}
                    </tbody>
                </table>
                </div>
            )}
        </section>
    );
}
