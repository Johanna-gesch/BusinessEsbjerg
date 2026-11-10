import apiClient from "@/services/ApiClient.ts";
import {type Dispatch, type SetStateAction, useEffect, useState} from "react";
import type {User} from "@/api/Api.ts";


type UsersListProps = {
    users: User[];
    setUsers: Dispatch<SetStateAction<User[]>>;
};

export function UserList({users, setUsers}: UsersListProps) {

    const [editingUserId, setEditingUserId] = useState<string | null>(null);

    useEffect(() => {
        apiClient.api.getUsers.userGetUsers().then(r => {
            setUsers(r);
        });
    }, [setUsers]);

    function handleEditOrSaveUser(user: User) {
        if (editingUserId === user.userId) {
            apiClient.api.updateUser.userUpdateUser({
                userId: user.userId,
                username: user.username,
            })
                .then(() => setEditingUserId(null))
                .then(() => {
                    apiClient.api.getUsers.userGetUsers().then(r => {
                        setUsers(r);
                    });
                });
        } else {
            setEditingUserId(user.userId);
        }
    }

    return (
        <div>
            {users.map(user => (
                <div
                    key={user.userId}
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
                    </div>
                </div>
            ))}
        </div>
    )

}