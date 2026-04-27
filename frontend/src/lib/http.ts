import axios from 'axios';
import { appConfig } from './config';

export const apiClient = axios.create({
    baseURL: appConfig.apiBaseUrl,
    timeout: 10000,
});

export function setApiAccessToken(token: string | null) {
    if (token) {
        apiClient.defaults.headers.common.Authorization = `Bearer ${token}`;
        return;
    }

    delete apiClient.defaults.headers.common.Authorization;
}
