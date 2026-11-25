
import apiClient from "@/services/ApiClient.ts";
import { type Dispatch, type SetStateAction, useEffect, useState } from "react";
import type { User } from "@/api/Api.ts";
import { EditUserForm } from "./EditUserForm.tsx";

const MyApi = apiClient.api;
const USERS_PER_PAGE = 10;

type UsersListProps = {
    users: User[];
    setUsers: Dispatch<SetStateAction<User[]>>;
};

export function UserList({ users, setUsers }: UsersListProps) {

    const [editingUser, setEditingUser] = useState<User | null>(null);
    const [page, setPage] = useState(1);

    const pageCount = Math.max(1, Math.ceil(users.length / USERS_PER_PAGE));
    const currentPage = Math.min(page, pageCount);
    const visibleUsers = users.slice(
        (currentPage - 1) * USERS_PER_PAGE,
        currentPage * USERS_PER_PAGE,
    );

    useEffect(() => {
        if (page > pageCount) {
            setPage(pageCount);
        }
    }, [page, pageCount]);

    useEffect(() => {
        setPage(1);
    }, [users.length]);

    function handleDeleteUser(userId: string) {
        if (!window.confirm("Are you sure you want to delete this user?")) {
            return;
        }

        MyApi.deleteUser.userDeleteUser({
            userId: userId
        })
            .then(() => {
                setUsers(prevUsers =>
                    prevUsers.filter(user => user.userId !== userId)
                );
            })
            .catch(error => {
                console.error("Failed to delete user:", error);
            });
    }

    function handleUpdateUser(updatedUser: User) {
        setUsers(prevUsers =>
            prevUsers.map(user =>
                user.userId === updatedUser.userId
                    ? updatedUser
                    : user
            )
        );
    }

    return (
        <div className="user-list">
            {visibleUsers.map(user => (
                <div
                    className="user-row"
                    key={user.userId}
                    data-testid="user-row"
                    data-user-id={user.userId}
                >
                    <div className="user-row-main">
                        <span className="user-avatar" aria-hidden="true">{user.username.slice(0, 1).toUpperCase()}</span>
                        <span className="user-name">{user.username}</span>

                        <div className="user-row-actions">
                        <button className="button button-secondary button-small" onClick={() => setEditingUser(user)}>
                            Edit
                        </button>

                        <button className="button button-danger button-small" onClick={() => handleDeleteUser(user.userId)}>
                            Delete
                        </button>
                        </div>
                    </div>
                </div>
            ))}

            {pageCount > 1 && (
                <nav className="user-pagination" aria-label="User list pages">
                    <span className="pagination-summary">
                        Page {currentPage} of {pageCount}
                    </span>
                    <div className="pagination-actions">
                        <button
                            className="button button-secondary button-small"
                            type="button"
                            disabled={currentPage === 1}
                            onClick={() => setPage(currentPage - 1)}
                        >
                            Previous
                        </button>
                        <button
                            className="button button-secondary button-small"
                            type="button"
                            disabled={currentPage === pageCount}
                            onClick={() => setPage(currentPage + 1)}
                        >
                            Next
                        </button>
                    </div>
                </nav>
            )}

            {editingUser && (
                <EditUserForm
                    user={editingUser}
                    onClose={() => setEditingUser(null)}
                    onUpdated={handleUpdateUser}
                />
            )}
        </div>
    );
}
