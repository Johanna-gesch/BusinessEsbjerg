
import { useEffect, useState } from "react";
import {
    type HolidayRequest,
    HolidayRequestStatus,
} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";

const MyApi = apiClient.api;

function getStatusText(status: HolidayRequestStatus): string {
    switch (status) {
        case HolidayRequestStatus.Pending:
            return "Pending";
        case HolidayRequestStatus.Approved:
            return "Approved";
        case HolidayRequestStatus.Rejected:
            return "Rejected";
        case HolidayRequestStatus.Cancelled:
            return "Cancelled";
        default:
            return "Unknown";
    }
}

export function MyHolidayRequests() {
    const [requests, setRequests] = useState<HolidayRequest[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [cancellingId, setCancellingId] = useState<string | null>(null);

    async function loadRequests() {
        try {
            const result =
                await MyApi.getRequestsForUser.requestGetRequestsForUser();

            setRequests(result);
        } catch (e) {
            console.error("Could not load holiday requests", e);
            setError("Could not load your holiday requests.");
        } finally {
            setLoading(false);
        }
    }

    async function cancelRequest(requestId: string) {
        const confirmed = window.confirm(
            "Are you sure you want to cancel this holiday request?"
        );

        if (!confirmed) return;

        setError("");
        setCancellingId(requestId);

        try {
            await MyApi.cancelRequest.requestCancelRequest({
                requestId,
            });

            await loadRequests();
        } catch (e) {
            console.error("Could not cancel holiday request", e);
            setError("Could not cancel your holiday request.");
        } finally {
            setCancellingId(null);
        }
    }

    useEffect(() => {
        loadRequests();
    }, []);

    if (loading) return <p>Loading requests...</p>;

    return (
        <section>
            <h2>My holiday requests</h2>

            {error && <p role="alert">{error}</p>}

            {requests.length === 0 && (
                <p>You have not submitted any holiday requests.</p>
            )}

            {requests.length > 0 && (
                <table>
                    <thead>
                    <tr>
                        <th>Start date</th>
                        <th>End date</th>
                        <th>Submitted</th>
                        <th>Status</th>
                        <th>Reason</th>
                        <th>Actions</th>
                    </tr>
                    </thead>

                    <tbody>
                    {requests.map(request => (
                        <tr key={request.id}>
                            <td>{request.startDate}</td>
                            <td>{request.endDate}</td>
                            <td>
                                {new Date(request.createdAt).toLocaleDateString()}
                            </td>
                            <td>{getStatusText(request.status)}</td>
                            <td>
                                {request.rejectionReason ??
                                    request.cancellationReason ??
                                    "-"}
                            </td>
                            <td>
                                {request.status === HolidayRequestStatus.Pending && (
                                    <button
                                        onClick={() => cancelRequest(request.id)}
                                        disabled={cancellingId === request.id}
                                    >
                                        {cancellingId === request.id
                                            ? "Cancelling..."
                                            : "Cancel"}
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}
        </section>
    );
}