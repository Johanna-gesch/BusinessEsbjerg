
import apiClient from "@/services/ApiClient.ts";
import { type Dispatch, type SetStateAction, useState } from "react";
import type { User } from "@/api/Api.ts";
import { EditUser } from "./EditUser";

const MyApi = apiClient.api;

type UsersListProps = {
    users: User[];
    setUsers: Dispatch<SetStateAction<User[]>>;
};

export function UserList({ users, setUsers }: UsersListProps) {

    const [editingUser, setEditingUser] = useState<User | null>(null);

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
        <div>
            {users.map(user => (
                <div
                    key={user.userId}
                    data-testid="user-row"
                    data-user-id={user.userId}
                >
                    <div>
                        <span>{user.username}</span>

                        <button onClick={() => setEditingUser(user)}>
                            Edit
                        </button>

                        <button onClick={() => handleDeleteUser(user.userId)}>
                            Delete
                        </button>
                    </div>
                </div>
            ))}

            {editingUser && (
                <EditUser
                    user={editingUser}
                    onClose={() => setEditingUser(null)}
                    onUpdated={handleUpdateUser}
                />
            )}
        </div>
    );
}
