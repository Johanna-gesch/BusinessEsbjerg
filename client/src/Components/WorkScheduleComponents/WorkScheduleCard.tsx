import apiClient from "@/services/ApiClient.ts";
import {useEffect, useState} from "react";
import type {WorkScheduleDto} from "@/api/Api.ts";


const MyApi = apiClient.api;

type Props = {
    userId: string;
};

const days = [
    {label: "Monday", key: "monday"},
    {label: "Tuesday", key: "tuesday"},
    {label: "Wednesday", key: "wednesday"},
    {label: "Thursday", key: "thursday"},
    {label: "Friday", key: "friday"},

] as const;

export function WorkScheduleCard({ userId }: Props) {
    const [schedule, setSchedule] = useState<WorkScheduleDto | null>(null);
    const [error, setError] = useState<string | null>(null);


    useEffect(() => {
        const today = new Date().toLocaleDateString("sv-SE"); //gives yyyy-MM-DD

        MyApi.getEffectiveWorkSchedule.workScheduleGetEffectiveWorkSchedule({userId, date: today})
            .then((response) => {
                setSchedule(response);
                setError(null);
            })
            .catch(() => {
                setError("No work schedule found");
            });
    }, [userId]);

    if (error) return <p>{error}</p>
    if (!schedule) return <p>Loading work schedule...</p>;

    const total = days.reduce((sum, day) => sum + schedule[day.key], 0);
    return (
        <div>
            <h2>Work Schedule</h2>
            <p>Effective from {new Date(schedule.effectiveFrom).toLocaleDateString("da-DK")}</p>
            <table>
                <tbody>
                {days.map(day => (
                    <tr key={day.key}>
                        <td>{day.label}</td>
                        <td>{schedule[day.key]} hours</td>
                    </tr>
                ))}
                <tr>
                    <td><strong>Total per week</strong></td>
                    <td><strong>{total} hours</strong></td>
                </tr>
                </tbody>
            </table>
        </div>
    );
}