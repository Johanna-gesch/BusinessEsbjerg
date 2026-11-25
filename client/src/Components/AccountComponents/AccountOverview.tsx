import apiClient from "@/services/ApiClient.ts";
import {useEffect, useState} from "react";
import type {EmployeebalanceDto} from "@/api/Api.ts";


const MyApi = apiClient.api;


// Rounds to 2 decimals so sum never show things with to many decimals 0.123123
function formatHours(hours: number): string {
    return `${Math.round(hours*100)/100} h`;
}

export function AccountOverview(){
    const [employees, setEmployees] = useState<EmployeebalanceDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let active = true;

        MyApi.getAccountOverview
            .accountGetAccountOverview()
            .then(result => {
                if (active) setEmployees(result);
    })
            .catch(e => {
            console.error("Could not load the account overview",e);
            if(active) setError("Could not load the account overview");
    })
            .finally(() => {
            if(active) setLoading(false);
    });
        return () => {
        active = false;
        };
    }, []);

    // The company total iis the summ of every row
    const totalWeek15 = employees.reduce((sum, e) => sum + e.week1_5Hours, 0);
    const totalWeek6 = employees.reduce((sum, e) => sum + e.week6Hours, 0);

    return (
        <section className="surface-card account-overview">
            <div className="section-heading">
                <div>
                    <p className="eyebrow">Team balances</p>
                    <h2>Holiday balances</h2>
                </div>
            </div>
            <p>Hours available today.</p>

            {loading && <p className="empty-state">Loading...</p>}
            {error && <p className="form-message form-message-error">{error}</p>}

            {!loading && !error && (
                <table className="data-table">
                    <thead>
                    <tr>
                        <th>Employee</th>
                        <th>Weeks 1–5</th>
                        <th>Week 6</th>
                        <th>Total</th>
                    </tr>
                    </thead>
                    <tbody>
                    {employees.map(e => (
                        <tr key={e.userId}>
                            <td>{e.username}</td>
                            <td>{formatHours(e.week1_5Hours)}</td>
                            <td>{formatHours(e.week6Hours)}</td>
                            <td>{formatHours(e.week1_5Hours + e.week6Hours)}</td>
                        </tr>
                    ))}
                    </tbody>
                    <tfoot>
                    <tr>
                        <th>Whole company</th>
                        <th>{formatHours(totalWeek15)}</th>
                        <th>{formatHours(totalWeek6)}</th>
                        <th>{formatHours(totalWeek15 + totalWeek6)}</th>
                    </tr>
                    </tfoot>
                </table>
            )}
        </section>
    );
}


