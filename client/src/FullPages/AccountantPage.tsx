import {useNavigate} from "react-router-dom";
import {AccountOverview} from "@/Components/AccountComponents/AccountOverview.tsx";


export function AccountantPage(){
    const navigate = useNavigate();

    return (
        <main className="page-shell accountant-page">
            <header className="page-header">
                <div>
                    <p className="eyebrow">Team overview</p>
                    <h1>Accounting</h1>
                </div>
                <button className="button button-secondary" onClick={() => navigate("/dashboard")}>
                Back to Dashboard
                </button>
            </header>

            <AccountOverview/>

        </main>
    );
}
