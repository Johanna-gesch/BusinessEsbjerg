
import apiClient from "@/services/ApiClient.ts";
import { useState } from "react";
import { type User } from "@/api/Api.ts";

const MyApi = apiClient.api;

type EditUserProps = {
    user: User;
    onClose: () => void;
    onUpdated: (updatedUser: User) => void;
};

type EditForm = {
    username: string;
    password: string;
};

export function EditUserForm({ user, onClose, onUpdated }: EditUserProps) {

    const [editForm, setEditForm] = useState<EditForm>({
        username: user.username,
        password: "",
    });

    function handleSaveUser(e: React.FormEvent) {
        e.preventDefault();

        MyApi.updateUser.userUpdateUser({
            userId: user.userId,
            username: editForm.username,
            password: editForm.password
        })
            .then(() => {
                onUpdated({
                    ...user,
                    username: editForm.username,
                });

                onClose();
            })
            .catch(error => {
                console.error("Failed to update user:", error);
            });
    }

    return (
        <div style={{
            position: "fixed",
            inset: 0,
            backgroundColor: "rgba(0, 0, 0, 0.5)",
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            zIndex: 1000
        }}>
            <div>
                <h2>Edit User</h2>

                <form onSubmit={handleSaveUser}>

                    <div>
                        <label>Username</label>
                        <input
                            type="text"
                            value={editForm.username}
                            onChange={e =>
                                setEditForm({
                                    ...editForm,
                                    username: e.target.value
                                })
                            }
                            required
                        />
                    </div>

                    <div>
                        <label>Password</label>
                        <input
                            type="password"
                            value={editForm.password}
                            placeholder="Leave empty to keep current password"
                            onChange={e =>
                                setEditForm({
                                    ...editForm,
                                    password: e.target.value
                                })
                            }
                        />
                    </div>

                    <div>
                        <button type="button" onClick={onClose}>
                            Cancel
                        </button>

                        <button type="submit">
                            Save
                        </button>
                    </div>

                </form>
            </div>
        </div>
    );
}
