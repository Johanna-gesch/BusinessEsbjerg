import {useNavigate} from "react-router-dom";
import {AccountOverview} from "@/Components/AccountComponents/AccountOverview.tsx";


export function AccountantPage(){
    const navigate = useNavigate();

    return (
        <div>
            <h1> Accounting </h1>

            <button onClick={() => navigate("/dashboard")}>
                Back to Dashboard
            </button>

            <AccountOverview/>

        </div>
    );
}