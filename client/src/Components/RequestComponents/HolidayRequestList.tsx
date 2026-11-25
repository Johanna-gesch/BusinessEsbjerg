import { useEffect, useState } from "react";
import {
    HolidayRequestStatus,
    type HolidayRequestDto,
} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";

function formatDate(value: string): string {
    return value.slice(0, 10).split("-").reverse().join("/");
}

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

export function RequestList() {
    const [requests, setRequests] = useState<HolidayRequestDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let active = true;

        apiClient.api.getAllRequests
            .requestGetAllRequests()
            .then((result) => {
                if (active) {
                    setRequests(result);
                }
            })
            .catch((err) => {
                console.error("Could not load holiday requests:", err);

                if (active) {
                    setError("Could not load holiday requests.");
                }
            })
            .finally(() => {
                if (active) {
                    setLoading(false);
                }
            });

        return () => {
            active = false;
        };
    }, []);

    if (loading) {
        return <p>Loading holiday requests...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <section>
            <h2>Holiday requests</h2>

            {requests.length === 0 ? (
                <p>No holiday requests found.</p>
            ) : (
                <table>
                    <thead>
                    <tr>
                        <th>Employee ID</th>
                        <th>Start date</th>
                        <th>End date</th>
                        <th>Status</th>
                        <th>Created</th>
                        <th>Rejection reason</th>
                    </tr>
                    </thead>
                    <tbody>
                    {requests.map((request) => (
                        <tr key={request.id}>
                            <td>{request.userId}</td>
                            <td>{formatDate(request.startDate)}</td>
                            <td>{formatDate(request.endDate)}</td>
                            <td>{getStatusText(request.status)}</td>
                            <td>{formatDate(request.createdAt)}</td>
                            <td>{request.rejectionReason ?? ""}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}
        </section>
    );
}