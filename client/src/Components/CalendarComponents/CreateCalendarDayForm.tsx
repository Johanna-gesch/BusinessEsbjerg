import apiClient from "@/services/ApiClient.ts";
import {type FormEvent, useState} from "react";


const MyApi = apiClient.api;

// Reads the message our API sends back when a request fails
function getErrorMessage(error: unknown): string {
    const problem = (error as { error?: { detail?: string } })?.error;
    return problem?.detail ?? "Could not save public holiday";
    }

/** Form where an admin creates a public holiday, a day nobody works. */
export function CreateCalendarDayForm(){
    const[date, setDate] = useState("");
    const [name, setName] = useState("");
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    async function handleSubmit(e: React.FormEvent){
        e.preventDefault();
        setMessage("");
        setError("");

        // Simple checks here, the server validates everything again.
        if(!date){
            setError("Choose a date");
            return;
        }
        if(!name.trim()){
            setError("Choose a name");
            return;
        }

        try{
            const saved = await MyApi.createCalendarDay.calendarDayCreateCalendarDay({date, name});
            setMessage(`Saved ${saved.name} on ${saved.date}`);
            setDate("");
            setName("");
        } catch (error) {
            setError(getErrorMessage(error));
        }
    }

    return (
        <form onSubmit={handleSubmit}>
            <h2>Create public holiday</h2>

            <div>
                <label>Date</label>
                <input
                    type="date"
                    value={date}
                    onChange={e => setDate(e.target.value)}
                />
            </div>

            <div>
                <label>Name</label>
                <input
                    type="text"
                    placeholder="Public holiday"
                    value={name}
                    onChange={e => setName(e.target.value)}
                />
            </div>

            <button type="submit">Save</button>

            {message && <p>{message}</p>}
            {error && <p>{error}</p>}
        </form>
    );
}