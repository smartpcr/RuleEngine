export function getValue<T = string>(key: string): T {
    // tslint:disable-next-line:no-string-literal
    const cfg = global["config"] || {};
    return cfg[key];
}
