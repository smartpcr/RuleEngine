import { ActionType } from "typesafe-actions";
import * as actions from "./DataCentersActions";
import {DataCentersStoreData, dataCentersInitialState} from "./DataCentersStoreData";
import { DataCenterActionTypes } from "./DataCenterActionTypes";


export type DataCenterActions = ActionType<typeof actions>;

export function dataCenters(
    state: DataCentersStoreData = dataCentersInitialState,
    action: DataCenterActions
): DataCentersStoreData {
    switch (action.type) {
        case DataCenterActionTypes.LOAD_DATACENTERLIST_ERROR:
            return { ...state, error: action.payload };

        case DataCenterActionTypes.LOAD_DATACENTERLIST_SUCCESS:
            return { ...state, items: action.payload };

        default:
            return state;
    }
}
