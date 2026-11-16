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
        <form onSubmit={handleLogin}>
            <input
                type="text"
                placeholder="Username"
                value={username}
                onChange={(e) =>
                    setUsername(e.target.value)}
            />

            <input
                type="password"
                placeholder="Password"
                value={password}
                onChange={(e) =>
                    setPassword(e.target.value)}
            />

            <button type="submit">Login</button>

            {error && <p>{error}</p>}
        </form>
    );
}