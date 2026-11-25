import {useNavigate} from "react-router-dom";
import {useEffect, useState} from "react";
import type {User} from "@/api/Api.ts";
import apiClient from "@/services/ApiClient.ts";
import {UserList} from "@/Components/UserComponents/UserList.tsx";
import {CreateUserForm} from "@/Components/UserComponents/CreateUserForm.tsx";
import {CreateWorkScheduleForm} from "@/Components/WorkScheduleComponents/CreateWorkScheduleForm.tsx";
import {CreateCalendarDayForm} from "@/Components/CalendarComponents/CreateCalendarDayForm.tsx";

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
        <main className="page-shell admin-page">
            <header className="page-header">
                <div>
                    <p className="eyebrow">Workspace</p>
                    <h1>Admin page</h1>
                </div>
                <button className="button button-secondary" onClick={() => navigate("/dashboard")}>
                Back to Dashboard
                </button>
            </header>

            <div className="admin-form-stack">
                <CreateWorkScheduleForm users={users} />
                <CreateCalendarDayForm />
                <CreateUserForm setUsers={setUsers} />
            </div>

            <section className="admin-users-section surface-card">
                <div className="section-heading">
                    <div>
                        <p className="eyebrow">People</p>
                        <h2>Manage users</h2>
                    </div>
                    <span className="count-pill">{users.length} users</span>
                </div>
                <UserList users={users} setUsers={setUsers} />
            </section>
        </main>
    );
}
