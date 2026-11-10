import {Api, type User} from "@/api/Api.ts";
import {type Dispatch, type SetStateAction, useEffect} from "react";

export const MyApi = new Api();

type UsersListProps = {
    users: User[];
    setUsers: Dispatch<SetStateAction<User[]>>;
};

export function UserList({users, setUsers}: UsersListProps) {

    useEffect(() => {
        MyApi.getUsers.userGetUsers().then(r => {
            setUsers(r);
        });
    }, []);

    return (
        <div>
            {users.map(user => (
                <div
                    key={user.userId}
                >
                    <div>
                        {user.username}
                    </div>
                </div>
            ))}
        </div>
    )

}