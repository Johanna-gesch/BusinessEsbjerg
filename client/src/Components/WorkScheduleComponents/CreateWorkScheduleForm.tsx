import apiClient from "@/services/ApiClient.ts";
import type {User} from "@/api/Api.ts";
import {useState} from "react";


const MyApi = apiClient.api;

type Props = {
    users: User[];
};

const days = [
    {label: "Monday", key: "monday"},
    {label: "Tuesday", key: "tuesday"},
    {label: "Wednesday", key: "wednesday"},
    {label: "Thursday", key: "thursday"},
    {label: "Friday", key: "friday"},

] as const;

type DayKey = typeof days[number]["key"];

function getErrorMessage(error: unknown): string {
    const problem = (error as {error?:{detail?: string}})?.error;
    return problem?.detail ?? "Could not save work schedule";
}

export function CreateWorkScheduleForm({users}: Props){
    const [userId, setUserId] = useState("")
    const [effectiveFrom, setEffectiveFrom] = useState("")

    // Default hours from the customer: Monday-Thursday 7.5, Friday 7.
    // Kept as strings so the user can type freely in the number inputs.
    const [hours, setHours] = useState<Record<DayKey, string>>({
        monday: "7,5",
        tuesday: "7,5",
        wednesday: "7,5",
        thursday: "7,5",
        friday: "7,5",
    });

    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    const totalPerWeek = days.reduce((sum, day) => sum + (Number(hours[day.key]) || 0), 0);

    async function handleSubmit (e: React.FormEvent){
        e.preventDefault();
        setMessage("");
        setError("");

        // Simple checks, the server validates everything again.
        if(!userId){
            setError("Chose a user");
        }
        if(!effectiveFrom){
            setError("Choose a user");
        }
        if(!effectiveFrom){
            setError("Choose a start date");
        }

        try{
            const saved = await MyApi.createWorkSchedule.workScheduleCreateWorkSchedule({
                userId,
                effectiveFrom,
                monday: Number(hours.monday),
                tuesday: Number(hours.tuesday),
                wednesday: Number(hours.wednesday),
                thursday: Number(hours.thursday),
                friday: Number(hours.friday),
            });
            setMessage(`Saved work schedule from ${saved.effectiveFrom}`);
        } catch (error) {
            setError(getErrorMessage(error));
        }
    }

    return (
        <form onSubmit={handleSubmit}>
            <h2>Create work schedule</h2>

            <div>
                <label>User</label>
                <select value={userId} onChange={e => setUserId(e.target.value)}>
                    <option value="">Choose user...</option>
                    {users.map(user => (
                        <option key={user.userId} value={user.userId}>
                            {user.username}
                        </option>
                    ))}
                </select>
            </div>

            <div>
                <label>Effective from</label>
                <input
                    type="date"
                    value={effectiveFrom}
                    onChange={e => setEffectiveFrom(e.target.value)}
                />
            </div>

            {days.map(day => (
                <div key={day.key}>
                    <label>{day.label}</label>
                    <input
                        type="number"
                        min="0"
                        max="24"
                        step="0.5"
                        value={hours[day.key]}
                        onChange={e => setHours({...hours, [day.key]: e.target.value})}
                    />
                </div>
            ))}

            <p>Total per week: {totalPerWeek} hours</p>

            <button type="submit">Save</button>

            {message && <p>{message}</p>}
            {error && <p>{error}</p>}
        </form>
    );

}