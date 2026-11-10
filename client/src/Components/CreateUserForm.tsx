import {useState} from "react";
import apiClient from "@/services/ApiClient.ts";

export function CreateUserForm() {

    const [usernameField, setUsernameField] = useState("");
    const [passwordField, setPasswordField] = useState("");
    const [isAdmin, setIsAdmin] = useState(false);

    function handleCreateUser(){
        apiClient.api.createUser.userCreateUser({
            username: usernameField,
            password: passwordField,
            isAdmin: isAdmin
        })
            .then(() => setUsernameField(""))
            .then(() => setPasswordField(""))
            .then(() => setIsAdmin(isAdmin))
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


            <button
                onClick={handleCreateUser}
            >
                Save
            </button>

        </div>
    )
}