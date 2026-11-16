import apiClient from "@/services/ApiClient.ts";
import {useEffect, useState} from "react";
import {type SalaryHistory, SalaryType} from "@/api/Api.ts";

const MyApi = apiClient.api;

interface Props {
    userId: string;
}

export function SalaryHistory({ userId }: Props) {

    const [salHis, setSalHis] = useState<SalaryHistory[]>([]);

    useEffect(() => {
        MyApi.getSalaryHistoryForUser.salaryGetSalaryHistoryForUser({userId})
            .then(r => {
                setSalHis(r);
            })
    }, []);

    return (
        <div>
            <h2>Salary History</h2>

            {salHis.length === 0 ? (
                <p>No salary history available.</p>
            ) : (
                salHis.map(his => (
                    <div key={his.id}>
                        <p>
                            {new Date(his.effectiveFrom).toLocaleDateString("da-DK")},
                            {" kl. "}
                            {new Date(his.effectiveFrom).toLocaleTimeString("da-DK", {
                                hour: "2-digit",
                                minute: "2-digit"
                            })}
                            {": "}
                            {his.salary.toLocaleString("da-DK", {
                                style: "currency",
                                currency: "DKK"
                            })}
                            {his.salaryType === SalaryType.Hourly
                                ? " / hour"
                                : " / month"}
                        </p>
                    </div>
                ))
            )}
        </div>
    )

}