import apiClient from "@/services/ApiClient.ts";
import {useNavigate} from "react-router-dom";

export function LogOutButton() {
    const navigate = useNavigate();

    function handleLogout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    apiClient.api.setSecurityData(null);
    navigate("/", {replace: true});
}


return (
    <button className="button button-quiet" type="button" onClick={handleLogout}>
        Log Out
    </button>
);
}
