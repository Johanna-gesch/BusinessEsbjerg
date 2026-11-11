import {useEffect, useState} from "react";
import type {User} from "@/api/Api.ts";
import {UserList} from "@/Components/UserList.tsx";
import {CreateUserForm} from "@/Components/CreateUserForm.tsx";
import apiClient from "@/services/ApiClient.ts";
import {LogOutButton} from "@/Components/LogOutButtn.tsx";

const MyApi = apiClient.api;

export function AdminPage() {

    const [users, setUsers] = useState<User[]>([]);

    useEffect(() => {
        console.log("AdminPage mounted - fetching users");

        MyApi.getUsers.userGetUsers()
            .then(r => {
                console.log("Users received:", r);
                setUsers(r);
            })
            .catch(error => {
                console.error("Failed to fetch users:", error);
            });
    }, []);

    return (
        <div>
            <h1>ADMIN PAGE</h1>
            <LogOutButton/>
            <UserList users={users} setUsers={setUsers} />
            <CreateUserForm setUsers={setUsers} />
        </div>
    )
}