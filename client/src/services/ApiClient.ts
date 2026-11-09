
import { Api } from "@/api/Api.ts";

class ApiClient {
    public api;

    constructor() {
        this.api = new Api<string>({
            securityWorker: (token) => {
                if (!token) {
                    return {};
                }

                return {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                };
            },
        });

        this.api.setSecurityData(localStorage.getItem("token"));
    }
}

export default new ApiClient();
