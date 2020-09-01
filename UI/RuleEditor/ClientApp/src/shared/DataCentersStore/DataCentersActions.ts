import { Dispatch } from "redux";
import { action } from "typesafe-actions";
import { DataCenter } from "../models/DataCenter";
import { getDataCenters } from "./DataCenterApi";
import { DataCenterActionTypes } from "./DataCenterActionTypes";

export function loadDataCentersError(error: Error) {
  return action(DataCenterActionTypes.LOAD_DATACENTERLIST_ERROR, error);
}

export function loadDataCentersSuccess(dataCenters: DataCenter[]) {
  return action(DataCenterActionTypes.LOAD_DATACENTERLIST_SUCCESS, dataCenters);
}

export function loadDataCenters() {
  return async (dispatch: Dispatch) => {
      try {
          const list = await getDataCenters();
          dispatch(loadDataCentersSuccess(list));
      } catch (err) {
          dispatch(loadDataCentersError(err));
      }
  };
}