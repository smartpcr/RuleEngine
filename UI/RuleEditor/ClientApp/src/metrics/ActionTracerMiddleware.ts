import { MetricSeverityLevel, trackTrace } from "./Metrics";
import _ from "lodash";

// follow-up: come fix this with proper typedefs once actions are typed
// tslint:disable-next-line:no-any
export const actionTracerMiddleware = (store: any) => (next: any) => (action: any) => {
    try {
        const actionType = <string>action.type;
        if (!_(actionType).startsWith("@@")) { // exclude react actions
            let dimensions = {};
            const actionPayload = action.payload;
            if (actionPayload) {
                if (Array.isArray(actionPayload)) {
                    const payloadArrayKey = `${actionType}_payloadArray`;
                    const payloadArrayVal = actionPayload.length;
                    dimensions[payloadArrayKey] = payloadArrayVal;
                } else {
                    dimensions = extractDimensionsFromItem(actionPayload, dimensions, true);
                }

                const customDimensions = actionPayload.ciDimensions;
                if (customDimensions) {
                    const ciDimensionsKeys = Object.getOwnPropertyNames(customDimensions);
                    ciDimensionsKeys.forEach(c => {
                        dimensions[c] = customDimensions[c];
                    });
                }
            }
            trackTrace(`ACTION_${actionType}`, MetricSeverityLevel.Verbose, dimensions);
        }
    } catch {
        // silently swallow w/o disturbing the main functionality
    }

    next(action);
};

function extractDimensionsFromItem(item: (typeof Object), dimensions: IMetricDimensions, shouldTraverseObjects?: boolean, prefix?: string): IMetricDimensions {
    if (item) {
        const actionKeys = Object.getOwnPropertyNames(item);
        actionKeys.forEach(k => {
            const val = item[k];
            if (Array.isArray(val)) {
                const dimKey = `${k}.length`;
                const dimVal = val.length;
                insertDimension(dimKey, dimVal, dimensions, prefix);
            } else if (typeof val === "string" || typeof val === "boolean" || typeof val === "number") {
                insertDimension(k, val, dimensions, prefix);
            } else if (typeof val === "object") {
                if (shouldTraverseObjects && !k.includes("ciDimensions")) {
                    extractDimensionsFromItem(val, dimensions, false, k);
                }
            }
        });
    }

    return dimensions;
}

function insertDimension(key: string, value: string | number | boolean, dimensions: IMetricDimensions, prefix?: string) {
    if (prefix) {
        key = `${prefix}.${key}`;
    }
    dimensions[key] = value;
}

export interface IMetricDimensions {
    [key: string]: unknown;
}
