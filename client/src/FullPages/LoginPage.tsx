import {useState} from "react";
//import {useNavigate} from "react-router-dom";
import apiClient from "@/services/ApiClient.ts";
import {getUserFromToken} from "@/auth/getUserFromToken.ts";


export default function LoginPage() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const navigate = useState();


    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");

        try {

            const response = await apiClient.api.login.userLogin({
                username,
                password,
            });

            const token = response.token;

            //save the token so we can use it for an api call.
            localStorage.setItem("token", token);

            apiClient.api.setSecurityData(token);

            const user = getUserFromToken(token)
            if (!user) {
                localStorage.removeItem("token");
                setError("Could not read user information from login token");
                return;
            }

            // save user so the App.tsx can get it
            localStorage.setItem("loggedInUser", JSON.stringify(user));

            navigate("/users");
        } catch (err) {
            setError("Login failed")
        }
    }
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