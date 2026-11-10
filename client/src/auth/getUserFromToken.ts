
import type { User } from "@/api/Api";

export function getUserFromToken(token: string): User | null {
    try {
        // Et JWT består af tre dele adskilt af punktummer.
        const payload = token.split(".")[1];

        if (!payload) {
            return null;
        }

        // JWT-payload bruger Base64URL-kodning.
        const base64 = payload
            .replace(/-/g, "+")
            .replace(/_/g, "/");

        const padded = base64.padEnd(
            base64.length + (4 - (base64.length % 4)) % 4,
            "="
        );

        const claims = JSON.parse(atob(padded));

        const userId =
            claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
            ?? claims.sub;

        const userName =
            claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
            ?? claims.unique_name;

        if (!userId || !userName) {
            return null;
        }

        const isAdmin = String(claims.isAdmin).toLowerCase() === "true";


        return {
            userId: String(userId),
            username: String(userName),
            passwordHash: "",
            isAdmin,
        };
    } catch {
        return null;
    }
}
