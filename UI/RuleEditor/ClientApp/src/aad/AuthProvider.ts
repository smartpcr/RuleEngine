import {
    AuthenticationContext,
    adalFetch,
    withAdalLogin,
    AdalConfig
} from "react-adal";
import { getValue } from "src/config";

export const adalConfig: AdalConfig = {
    tenant:
        getValue("AadSettings:TenantId") ||
        "72f988bf-86f1-41af-91ab-2d7cd011db47",
    clientId:
        getValue("AadSettings:ClientId") ||
        "87e94073-6809-4746-b283-4d266aea8510",
    endpoints: {
        api:
            getValue("AadSettings:ClientId") ||
            "87e94073-6809-4746-b283-4d266aea8510"
    },
    cacheLocation: "localStorage"
};

export const adResourceId = getValue("UI:AD_RESOURCE_ID");

export const authContext = new AuthenticationContext(adalConfig);

export const adalApiFetch = (
    fetch: (input: string, init: any) => Promise<any>,
    url: string,
    options: any
) =>
    adalFetch(
        authContext,
        adalConfig.endpoints ? adalConfig.endpoints.api : "",
        fetch,
        url,
        options
    );

export const withAdalLoginApi = withAdalLogin(
    authContext,
    adalConfig.endpoints ? adalConfig.endpoints.api : ""
);

export async function acquireToken(): Promise<string> {
    return new Promise<string>((resolve, reject) => {
        authContext.acquireToken(adResourceId, (error, token) => {
            if (error || !token) {
                if (error === "User login is required") {
                    authContext.login();
                }
                reject(error || "No token found");
                return;
            }
            resolve(token);
        });
    });
}
