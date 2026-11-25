
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
        <div className="modal-backdrop">
            <div className="modal-card">
                <h2>Edit User</h2>

                <form
                    className="edit-user-form"
                    data-testid={"edit-user-form"}
                    onSubmit={handleSaveUser}
                >

                    <div>
                        <label htmlFor="edit-username">Username</label>
                        <input
                            id="edit-username"
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
                        <button className="button button-secondary" type="button" onClick={onClose}>
                            Cancel
                        </button>

                        <button className="button button-primary" type="submit">
                            Save
                        </button>
                    </div>

                </form>
            </div>
        </div>
    );
}
