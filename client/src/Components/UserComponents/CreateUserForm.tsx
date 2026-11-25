import {type Dispatch, type SetStateAction, useState} from "react";
import apiClient from "@/services/ApiClient.ts";
import {type User, UserRole} from "@/api/Api.ts";

const MyApi = apiClient.api;

type CreateUserFormProps = {
    setUsers: Dispatch<SetStateAction<User[]>>;
};

export function CreateUserForm({setUsers}: CreateUserFormProps) {

    const [usernameField, setUsernameField] = useState("");
    const [passwordField, setPasswordField] = useState("");
    const [role, setRole] = useState<UserRole>(UserRole.User);

    async function handleCreateUser() {
        try {
            await MyApi.createUser.userCreateUser({
                username: usernameField,
                password: passwordField,
                role: role,
            });

            const updatedUsers = await MyApi.getUsers.userGetUsers();

            setUsers(updatedUsers);

            setUsernameField("");
            setPasswordField("");
            setRole(UserRole.User);
        } catch (error) {
            console.error("Could not create user:", error);
        }
    }

    return (
        <form
            className="surface-card admin-form create-user-form"
            data-testid="create-user-form"
            onSubmit={e => {
                e.preventDefault();
                void handleCreateUser();
            }}
        >
            <h2>Create user</h2>

            <input
                aria-label="Username"
                placeholder="username"
                value={usernameField}
                onChange={e => setUsernameField(e.target.value)}
            />

            <input
                aria-label="Password"
                type="password"
                placeholder="password"
                value={passwordField}
                onChange={e => setPasswordField(e.target.value)}
            />

            <select
                value={role}
                onChange={e => setRole(Number(e.target.value) as UserRole)}
            >
                <option value={UserRole.User}>User</option>
                <option value={UserRole.Admin}>Admin</option>
                <option value={UserRole.Accountant}>Accountant</option>
            </select>

            <button className="button button-primary" type="submit">
                Save
            </button>
        </form>
    );
}
