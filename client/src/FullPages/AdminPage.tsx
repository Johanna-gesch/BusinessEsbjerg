import { useState } from "react";
import type {User} from "@/api/Api.ts";
import {UserList} from "@/Components/UserList.tsx";
import {CreateUserForm} from "@/Components/CreateUserForm.tsx";
import {LogOutButton} from "@/Components/LogOutButtn.tsx";


export function AdminPage() {
    const [users, setUsers] = useState<User[]>([]);

    return (
        <div>
            <LogOutButton/>
            <UserList users={users} setUsers={setUsers} />
            <CreateUserForm />
        </div>
    )
}