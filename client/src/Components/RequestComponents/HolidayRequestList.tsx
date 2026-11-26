import { useEffect, useState } from "react";
import {
    HolidayRequestStatus,
    type HolidayRequestDto,
} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";

const MAX_REASON_LENGTH = 255; // same limit as in RequestService

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

export function HolidayRequestList() {
    const [requests, setRequests] = useState<HolidayRequestDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [actionError, setActionError] = useState("");
    const [updatingRequestId, setUpdatingRequestId] = useState<string | null>(null);

    // Which request the accountant is currently writing a rejection reason for
    const [rejectingId, setRejectingId] = useState<string | null>(null);
    const [rejectionReason, setRejectionReason] = useState("");

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

    function openRejectForm(requestId: string) {
        setRejectingId(requestId);
        setRejectionReason("");
        setActionError("");
    }

    function closeRejectForm() {
        setRejectingId(null);
        setRejectionReason("");
    }

    async function handleDecision(
        requestId: string,
        status: HolidayRequestStatus.Approved | HolidayRequestStatus.Rejected,
        reason?: string,
    ) {
        setUpdatingRequestId(requestId);
        setActionError("");

        try {
            await apiClient.api.updateRequestStatus.requestUpdateRequestStatus({
                requestId,
                status,
                rejectionReason: reason, // undefined (not sent) when approving
            });

            setRequests((currentRequests) =>
                currentRequests.map((request) =>
                    request.id === requestId
                        ? { ...request, status, rejectionReason: reason ?? null }
                        : request,
                ),
            );
            closeRejectForm();
        } catch (err) {
            console.error("Could not update holiday request:", err);
            setActionError("Could not update the request.");
        } finally {
            setUpdatingRequestId(null);
        }
    }

    if (loading) {
        return <p>Loading holiday requests...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <section>
            <h2>Holiday requests</h2>

            {actionError && <p role="alert">{actionError}</p>}

            {requests.length === 0 ? (
                <p>No holiday requests found.</p>
            ) : (
                <table>
                    <thead>
                    <tr>
                        <th>Employee</th>
                        <th>Start date</th>
                        <th>End date</th>
                        <th>Status</th>
                        <th>Created</th>
                        <th>Rejection reason</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {requests.map((request) => {
                        const isRejecting = rejectingId === request.id;
                        const isUpdating = updatingRequestId === request.id;

                        return (
                            <tr key={request.id}>
                                <td>{request.userName}</td>
                                <td>{formatDate(request.startDate)}</td>
                                <td>{formatDate(request.endDate)}</td>
                                <td>{getStatusText(request.status)}</td>
                                <td>{formatDate(request.createdAt)}</td>
                                <td>{request.rejectionReason ?? ""}</td>
                                <td>
                                    {request.status !== HolidayRequestStatus.Pending ? (
                                        "—"
                                    ) : isRejecting ? (
                                        <div>
                                            <textarea
                                                aria-label="Rejection reason"
                                                placeholder="Why is the request rejected? The employee will see this."
                                                value={rejectionReason}
                                                maxLength={MAX_REASON_LENGTH}
                                                rows={3}
                                                onChange={(e) => setRejectionReason(e.target.value)}
                                            />
                                            <button
                                                disabled={isUpdating || rejectionReason.trim() === ""}
                                                onClick={() =>
                                                    handleDecision(
                                                        request.id,
                                                        HolidayRequestStatus.Rejected,
                                                        rejectionReason.trim(),
                                                    )
                                                }
                                            >
                                                Confirm rejection
                                            </button>
                                            <button disabled={isUpdating} onClick={closeRejectForm}>
                                                Cancel
                                            </button>
                                        </div>
                                    ) : (
                                        <>
                                            <button
                                                disabled={isUpdating}
                                                onClick={() =>
                                                    handleDecision(
                                                        request.id,
                                                        HolidayRequestStatus.Approved,
                                                    )
                                                }
                                            >
                                                Approve
                                            </button>
                                            <button
                                                disabled={isUpdating}
                                                onClick={() => openRejectForm(request.id)}
                                            >
                                                Reject
                                            </button>
                                        </>
                                    )}
                                </td>
                            </tr>
                        );
                    })}
                    </tbody>
                </table>
            )}
        </section>
    );
}