import { DataCenter } from "../models/DataCenter";
import { acquireToken } from "../../aad/AuthProvider";
import { getValue } from "../../config";

export async function getDataCenters(): Promise<DataCenter[]> {
    try {
        const adToken: string = await acquireToken();
        const apiHost = getValue("UI:API_URL");
        const request = new Request(`${apiHost}/datacenters`, {
            method: "GET",
            headers: { Authorization: `Bearer ${adToken}` }
        });
        const response = await fetch(request);
        if (response.status === 200) {
            return (await response.json()).value;
        } else {
            throw new Error(
                `ErrorCode: ${response.status}, ErrorText: ${response.statusText}`
            );
        }
    } catch (err) {
        throw new Error(
            `ErrorCode: 0, ErrorText: Failed to fetch data centers`
        );
    }
}
