// remove PII info from metrics

export function maskEmail(s: string) {
    const emailRegex = /([\w\.\-]+)@(([\w\.\-]+\.)+\w+)/gi;
    let match = emailRegex.exec(s);
    while (match) {
        s = s.replace(
            match[0],
            `${getHashString(match[1])}@${getHashString(match[2])}.MASKED`
        );
        match = emailRegex.exec(s);
    }

    return s;
}

export function getHashString(s: string): string {
    return getHashCode(s).toString(16);
}

export function getHashCode(s: string): number {
    let hash = 0;
    if (!s) {
        return hash;
    }
    for (let i = 0; i < s.length; ++i) {
        // tslint:disable:no-bitwise
        hash = (hash << 5) - hash + s.charCodeAt(i);
        hash |= 0; // to int32
    }

    return hash;
}
