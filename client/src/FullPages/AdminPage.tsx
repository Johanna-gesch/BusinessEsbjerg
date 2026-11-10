import { useState } from "react";
import type {User} from "@/api/Api.ts";
import {UserList} from "@/Components/UserList.tsx";


export function AdminPage() {
    const [users, setUsers] = useState<User[]>([]);

    return (
        <div>
            <UserList users={users} setUsers={setUsers} />
        </div>
    )
}