import { DataCenter } from "../models/DataCenter";

export class DataCentersStoreData {
    public items: DataCenter[] = [];
    public loadingDataCenterDetails: boolean = false;
    public error?: Error;
}

export const dataCentersInitialState: DataCentersStoreData = {
    items: [],
    loadingDataCenterDetails: false,
    error: undefined
};
