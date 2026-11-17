import {UserRole} from "@/api/Api.ts";

export interface LoggedInUser {
    userId: string;
    username: string;
    role: UserRole;
}
