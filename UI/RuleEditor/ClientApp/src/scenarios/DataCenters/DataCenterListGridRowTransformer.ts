import { DataCenter } from "../../shared/models/DataCenter";
import { DataCenterListGridRow } from "./DataCenterListGridRow";
import * as _ from "lodash";

export function getDataCenterListGridRows(list: DataCenter[]): DataCenterListGridRow[] {
    let rows: DataCenterListGridRow[] = [];
    if (list.length === 0) {
        return rows;
    }

    for (const dc of list) {
        rows.push(getDataCenterListGridRow(dc));
    }
    rows = sortDataCenterListGridColumn(rows, "dcName", false);
    return rows;
}

export function getDataCenterListGridRow(dataCenter: DataCenter): DataCenterListGridRow {
    const row = new DataCenterListGridRow();
    if (_.isEmpty(dataCenter)) {
        return row;
    }

    row.dcCode = dataCenter.code;
    row.dcName = dataCenter.name;
    return row;
}

export function sortDataCenterListGridColumn(
    items: DataCenterListGridRow[],
    columnSortKey: string,
    isSortedDescending: boolean
): DataCenterListGridRow[] {
    items.sort((a: DataCenterListGridRow, b: DataCenterListGridRow) => {
        const sortA = getSortString(a, columnSortKey);
        const sortB = getSortString(b, columnSortKey);
        if (sortA < sortB) {
            return isSortedDescending ? 1 : -1;
        } else if (sortA > sortB) {
            return isSortedDescending ? -1 : 1;
        } else {
            return 0;
        }
    });

    return items;
}

function getSortString(
    item: DataCenterListGridRow,
    columnSortKey: string
): string {
    switch (columnSortKey) {
        case "dcCode":
            return item.dcCode.toString();
        default:
            return item.dcName.toLowerCase();
    }
}
