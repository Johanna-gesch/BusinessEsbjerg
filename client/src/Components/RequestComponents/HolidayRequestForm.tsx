
import { useState } from "react";
import apiClient from "@/services/ApiClient.ts";

const MyApi = apiClient.api;

export function HolidayRequestForm() {
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();

        setMessage("");
        setError("");

        if (!startDate || !endDate) {
            setError("Please select both dates.");
            return;
        }

        if (endDate < startDate) {
            setError("The end date cannot be before the start date.");
            return;
        }

        setLoading(true);

        try {
            await MyApi.createRequest.requestCreateRequest({
                startDate,
                endDate,
            });

            setMessage("Your holiday request has been submitted.");
            setStartDate("");
            setEndDate("");
        } catch (e) {
            console.error("Could not submit holiday request:", e);

            if (e instanceof Response) {
                try {
                    const body = await e.clone().json();

                    setError(
                        body.detail ??
                        body.message ??
                        body.title ??
                        "Could not submit your holiday request."
                    );
                } catch {
                    setError("Could not submit your holiday request.");
                }
            } else {
                setError("Could not submit your holiday request.");
            }
        } finally {
            setLoading(false);
        }
    }

    return (
        <section>
            <h2>Request holiday</h2>

            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="startDate">Start date</label>
                    <input
                        id="startDate"
                        type="date"
                        value={startDate}
                        onChange={e => setStartDate(e.target.value)}
                        required
                    />
                </div>

                <div>
                    <label htmlFor="endDate">End date</label>
                    <input
                        id="endDate"
                        type="date"
                        value={endDate}
                        onChange={e => setEndDate(e.target.value)}
                        min={startDate || undefined}
                        required
                    />
                </div>

                <button type="submit" disabled={loading}>
                    {loading ? "Submitting..." : "Submit request"}
                </button>
            </form>

            {message && <p>{message}</p>}
            {error && <p role="alert">{error}</p>}
        </section>
    );
}