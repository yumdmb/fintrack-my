function trimTrailingSlash(value: string) {
    return value.endsWith('/') ? value.slice(0, -1) : value;
}

const fallbackApiBaseUrl = 'http://localhost:5232';

export const appConfig = {
    apiBaseUrl: trimTrailingSlash(import.meta.env.VITE_API_BASE_URL ?? fallbackApiBaseUrl),
};
