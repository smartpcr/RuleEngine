import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import * as chalk from "chalk";
import { getHashString, maskEmail } from "./ScrubTools";
import { getValue } from "../config";
import { authContext } from "../aad/AuthProvider";

export enum MetricSeverityLevel {
    Verbose = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

let commonAppInsights: ApplicationInsights;

export function initAppInsights(): void {
    commonAppInsights = new ApplicationInsights({
        config: {
            instrumentationKey: getValue("AppInsightsSettings:InstrumentationKey") || "6e0abf0e-f451-43b6-8e57-e64025b73269",
            maxAjaxCallsPerView: -1
        }
    });
    loadAppInsights(commonAppInsights);
}

function loadAppInsights(ai: ApplicationInsights) {
    ai.loadAppInsights();
    let username = "unknown";
    authContext.getUser((err, user) => {
        if (err) {
            console.log(chalk.yellow(err));
        } else {
            username = user ? user.userName : "unknown";
        }
        ai.setAuthenticatedUserContext(
            getHashString(username),
            username,
            true
        );
    });

    ai.addTelemetryInitializer(envelope => {
        if (envelope.tags) {
            envelope.tags["ai.device.roleName"] = envelope.tags[
                "ai.cloud.role"
            ] = "dc-health";
        }
        if (envelope.baseData) {
            maskPersonalInfo(envelope.baseData);
        }
    });
}

export function trackPageView(
    pageName: string,
    values?: { [key: string]: unknown }
) {
    const { props, measures } = convertToPropsAndMeasurements(values);
    commonAppInsights.trackPageView({
        name: pageName,
        properties: props,
        measurements: measures
    });
}

export function trackTrace(
    message: string,
    severity: MetricSeverityLevel,
    values?: { [key: string]: unknown }
) {
    const props = convertToProps(values);
    commonAppInsights.trackTrace({
        message,
        properties: props,
        severityLevel: severity.valueOf()
    });
}

export function trackEvent(name: string, values?: { [key: string]: unknown }) {
    const { props, measures } = convertToPropsAndMeasurements(values);
    commonAppInsights.trackEvent({
        name,
        properties: props,
        measurements: measures
    });
}

export async function flush(): Promise<void> {
    commonAppInsights.flush();

    return new Promise<void>(resolve => {
        setTimeout(() => {
            resolve();
        }, 500); // give some time for all network request to be sent and not cancelled by the browser
    });
}

function convertToPropsAndMeasurements(values?: { [key: string]: unknown }) {
    let props: { [key: string]: string } | undefined = {};
    let measures: { [key: string]: number } | undefined = {};

    if (values) {
        // tslint:disable-next-line:no-for-in forin
        for (const key in values) {
            const value = values[key];
            if (typeof value === "number") {
                measures[key] = value;
            } else if (value) {
                addToProps(key, value, props);
            }
        }
    }

    if (Object.keys(props).length === 0) {
        props = undefined;
    }
    if (Object.keys(measures).length === 0) {
        measures = undefined;
    }

    return { props, measures };
}

function convertToProps(values?: { [key: string]: unknown }) {
    const props: { [key: string]: string } | undefined = {};
    if (values) {
        // tslint:disable-next-line:no-for-in forin
        for (const key in values) {
            const value = values[key];
            if (value) {
                addToProps(key, value, props);
            }
        }
    }

    return props;
}

function addToProps(
    key: string,
    value: unknown,
    props: { [key: string]: string }
) {
    if (value) {
        const strValue = `${value}`;
        props[key] = maskEmail(strValue);
    }
}

// tslint:disable-next-line:no-any
function maskPersonalInfo(telemetry: { [key: string]: any } | undefined) {
    if (!telemetry) {
        return;
    }
    if (telemetry.url) {
        telemetry.url = maskEmail(telemetry.url);
    }
    // tslint:disable-next-line:no-for-in forin
    for (const key in telemetry.properties) {
        telemetry.properties[key] = maskEmail(
            telemetry.properties[key]
        );
    }
}
