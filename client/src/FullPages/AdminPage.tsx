import {useNavigate} from "react-router-dom";
import {useEffect, useState} from "react";
import type {User} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";
import {LogOutButton} from "@/Components/LogOutButtn.tsx";
import {UserList} from "@/Components/UserList.tsx";
import {CreateUserForm} from "@/Components/CreateUserForm.tsx";

const MyApi = apiClient.api;

export function AdminPage() {

    const navigate = useNavigate();

    const [users, setUsers] = useState<User[]>([]);

    useEffect(() => {
        MyApi.getUsers.userGetUsers()
            .then(r => {
                setUsers(r);
            })
            .catch(error => {
                console.error("Failed to fetch users:", error);
            });
    }, []);

    return (
        <div>
            <h1>ADMIN PAGE</h1>

            <button onClick={() => navigate("/dashboard")}>
                Back to Dashboard
            </button>

            <LogOutButton />

            <UserList users={users} setUsers={setUsers} />
            <CreateUserForm setUsers={setUsers} />
        </div>
    );
}