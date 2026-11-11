import apiClient from "@/services/ApiClient.ts";
import {type Dispatch, type SetStateAction, useState} from "react";
import type {User} from "@/api/Api.ts";

const MyApi = apiClient.api;

type UsersListProps = {
    users: User[];
    setUsers: Dispatch<SetStateAction<User[]>>;
};

export function UserList({users, setUsers}: UsersListProps) {

    const [editingUserId, setEditingUserId] = useState<string | null>(null);

    function handleEditOrSaveUser(user: User) {

        console.log("Clicked edit/save:", user.userId);
        console.log("Currently editing:", editingUserId);

        if (editingUserId === user.userId) {
            MyApi.updateUser.userUpdateUser({
                userId: user.userId,
                username: user.username,
            })
                .then(() => setEditingUserId(null))
                .catch(error => {
                    console.error("Failed to update user:", error);
                });
        } else {
            setEditingUserId(user.userId);
            console.log("Setting editing user:", user.userId);
        }
    }

    function handleDeleteUser(userId: string) {
        if (!window.confirm("Are you sure you want to delete this user?")) {
            return;
        }

        MyApi.deleteUser.userDeleteUser({
            userId: userId
        })
            .then(() => {
                setUsers(users.filter(user => user.userId !== userId));
            })
            .catch(error => {
                console.error("Failed to delete user:", error);
            });
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
                        {editingUserId === user.userId ? (
                            <input
                                value={user.username}
                                onChange={e => {
                                    setUsers(users.map(u =>
                                    u.userId === user.userId
                                        ? {...u, username: e.target.value}
                                        : u
                                    ));
                                }}
                            />
                        ) : (
                            <>{user.username}</>
                        )}
                        <button onClick={() => handleEditOrSaveUser(user)}>
                            {editingUserId === user.userId ? "Save" : "Edit"}
                        </button>

                        <button onClick={() => handleDeleteUser(user.userId)}>
                            Delete
                        </button>
                    </div>
                </div>
            ))}
        </div>
    )

}