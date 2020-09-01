import { routerReducer } from "react-router-redux";
import { combineReducers } from "redux";
import { StateType } from "typesafe-actions";
import { uiRoot } from "./UIRootReducer";
import { dataCenters } from "../shared/DataCentersStore/DataCentersReducer";

export const dataCenterRootReducer = combineReducers(
    {
        uiRoot,
        routing: routerReducer,
        dataCenters
    }
);

export type RootState = StateType<typeof dataCenterRootReducer>;
