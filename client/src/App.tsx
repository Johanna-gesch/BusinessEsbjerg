import "./index.css";
import { BrowserRouter, Routes, Route, useNavigate} from "react-router-dom";
import {AdminPage} from "@/FullPages/AdminPage.tsx";


export function App() {
    const navigate = useNavigate();
    return (
        <div>
            <h1>Business Esbjerg: Absence Resource Planning</h1>
            <button onClick={() => navigate("/admin")}>Admin</button>
        </div>
    );
}

export default App;
