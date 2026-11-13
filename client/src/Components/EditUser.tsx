
import apiClient from "@/services/ApiClient.ts";
import { useState } from "react";
import { SalaryType, type User } from "@/api/Api.ts";

const MyApi = apiClient.api;

type EditUserProps = {
    user: User;
    onClose: () => void;
    onUpdated: (updatedUser: User) => void;
};

type EditForm = {
    username: string;
    password: string;
    salaryType: SalaryType;
    salary: string;
};

export function EditUser({ user, onClose, onUpdated }: EditUserProps) {

    const [editForm, setEditForm] = useState<EditForm>({
        username: user.username,
        password: "",
        salaryType: user.salaryType,
        salary: String(user.salary)
    });

    function handleSaveUser(e: React.FormEvent) {
        e.preventDefault();

        MyApi.updateUser.userUpdateUser({
            userId: user.userId,
            username: editForm.username,
            salaryType: editForm.salaryType,
            salary: Number(editForm.salary)
        })
            .then(() => {
                onUpdated({
                    ...user,
                    username: editForm.username,
                    salaryType: editForm.salaryType,
                    salary: Number(editForm.salary)
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
            <div style={{
                backgroundColor: "white",
                padding: "30px",
                borderRadius: "10px",
                width: "400px",
                maxWidth: "90%",
                color: "black"
            }}>
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
                        <label>Salary Type</label>
                        <select
                            value={String(editForm.salaryType)}
                            onChange={e =>
                                setEditForm({
                                    ...editForm,
                                    salaryType: Number(e.target.value) as SalaryType
                                })
                            }
                        >
                            <option value={String(SalaryType.Hourly)}>
                                Hourly
                            </option>
                            <option value={String(SalaryType.Monthly)}>
                                Monthly
                            </option>
                        </select>
                    </div>

                    <div>
                        <label>Salary</label>
                        <input
                            type="number"
                            value={editForm.salary}
                            onChange={e =>
                                setEditForm({
                                    ...editForm,
                                    salary: e.target.value
                                })
                            }
                            required
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
