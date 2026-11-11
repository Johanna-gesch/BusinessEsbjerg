import {type Dispatch, type SetStateAction, useState} from "react";
import apiClient from "@/services/ApiClient.ts";
import type {User} from "@/api/Api.ts";

const MyApi = apiClient.api;

type CreateUserFormProps = {
    setUsers: Dispatch<SetStateAction<User[]>>;
};

export function CreateUserForm({setUsers}: CreateUserFormProps) {

    const [usernameField, setUsernameField] = useState("");
    const [passwordField, setPasswordField] = useState("");
    const [isAdmin, setIsAdmin] = useState(false);

    async function handleCreateUser() {
        try {
            await MyApi.createUser.userCreateUser({
                username: usernameField,
                password: passwordField,
                isAdmin: isAdmin
            });

            const updatedUsers = await MyApi.getUsers.userGetUsers();

            setUsers(updatedUsers);

            setUsernameField("");
            setPasswordField("");
        } catch (error) {
            console.error("Could not create user:", error);
        }
    }

    return (
        <div>
            Create User:
            <input
                placeholder={"username"}
                value={usernameField}
                onChange={(e) => setUsernameField(e.target.value)}
            />
            <input
                type={"password"}
                placeholder={"password"}
                value={passwordField}
                onChange={e => setPasswordField(e.target.value)}
            />
            <select
                value={isAdmin ? "admin" : "user"}
                onChange={(e) => setIsAdmin(e.target.value === "admin")}
            >
                <option value="user">User</option>
                <option value="admin">Admin</option>
            </select>
            <button
                type={"button"}
                onClick={handleCreateUser}
            >
                Save
            </button>

        </div>
    )
}