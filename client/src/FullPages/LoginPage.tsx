import {type FormEvent, useState} from "react";
import apiClient from "@/services/ApiClient.ts";
import {getUserFromToken} from "@/auth/getUserFromToken.ts";
import {useNavigate} from "react-router-dom";


export default function LoginPage() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const navigate = useNavigate();


    const handleLogin = async (e:FormEvent) => {
        e.preventDefault();
        setError("");

        try {
            const response = await apiClient.api.login.userLogin({ username, password });
            const token = response.token;

            // save the token and give it to the API-Client, so it protectet call works
            localStorage.setItem("token", response.token);
            apiClient.api.setSecurityData(response.token);

            const user = getUserFromToken(token)
            if (!user) {
                localStorage.removeItem("token");
                apiClient.api.setSecurityData(null);
                setError("Could not read user information from login token");
                return;
            }

            // save user so the Dashboard.tsx can get it
            localStorage.setItem("loggedInUser", JSON.stringify(user));

            navigate("/dashboard");
        } catch (err) {
            setError("Login failed")
        }
    };
    return (
        <main className="login-page">
            <section className="login-card">
                <div className="login-mark" aria-hidden="true">BE</div>
                <p className="eyebrow">Business Esbjerg</p>
                <h1>Holiday planner</h1>
                <p className="login-intro">Sign in to view your holiday balance and work schedule.</p>
                <form className="login-form" onSubmit={handleLogin}>
            <input
                aria-label="Username"
                type="text"
                placeholder="Username"
                value={username}
                onChange={(e) =>
                    setUsername(e.target.value)}
            />

            <input
                aria-label="Password"
                type="password"
                placeholder="Password"
                value={password}
                onChange={(e) =>
                    setPassword(e.target.value)}
            />

            <button className="button button-primary" type="submit">Login</button>

            {error && <p className="form-message form-message-error">{error}</p>}
                </form>
            </section>
        </main>
    );
}
