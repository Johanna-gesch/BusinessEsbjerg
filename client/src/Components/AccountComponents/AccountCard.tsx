import type {HolidayAccount} from "@/api/Api.ts";

type AccountCardProps = {
    account: HolidayAccount;
};

export function AccountCard({ account}: AccountCardProps){

    const accountTitle = account.type === 0
        ? "First five holiday weeks"
        : "Sixth holiday week";

    return (
        <div>
            <h3>{accountTitle}</h3>

            <p>
                {account.type === 0
                    ? "Your ordinary holiday account."
                    : "Your additional sixth holiday week account."}
            </p>
        </div>
    );
}