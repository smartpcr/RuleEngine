import { acquireToken } from "../../aad/AuthProvider";
import {getValue} from "../../config";

export class Api {
    private adToken!: string;
    private apiHost!: string;

    public constructor(private resource: string) {
        this.apiHost = getValue("UI:API_URL");
    }

    public async get<T>(path?: string): Promise<T>  {
        const request = await this.createRequest(path);
        const response = await fetch(request);
        if (response.status === 200) {
            return (await response.json()).value;
        } else {
            throw new Error(`ErrorCode: ${response.status}, ErrorText: ${response.statusText}`);
        }
    }

    private async createRequest(path?: string): Promise<Request> {
        this.adToken = await acquireToken();
        const url = path ? `${this.apiHost}/${path}` : `${this.apiHost}/${this.resource}`;
        const request: Request = new Request(url, {
            method: "GET",
            headers: { Authorization: `Bearer ${this.adToken}` }
        });
        return request;
    }
}