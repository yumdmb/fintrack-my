import axios from 'axios';
import { apiClient } from './http';

export type UserRole = 'admin' | 'accountant' | 'staff';

export interface CurrentUserResponse {
    id: string;
    email: string;
    companyId: string;
    companyName: string;
    roles: UserRole[];
}

export interface AuthResponse {
    accessToken: string;
    expiresAt: string;
    user: CurrentUserResponse;
}

export interface SignInRequest {
    email: string;
    password: string;
}

export interface InvoiceLineItemRequest {
    description: string;
    quantity: number;
    unitPrice: number;
    taxRate: number;
}

export interface UpsertInvoiceRequest {
    invoiceNumber: string;
    customerName: string;
    customerRegistrationNumber: string | null;
    customerTaxIdentificationNumber: string | null;
    issueDate: string;
    dueDate: string | null;
    currencyCode: string | null;
    lineItems: InvoiceLineItemRequest[];
}

export interface InvoiceSummaryResponse {
    id: string;
    invoiceNumber: string;
    customerName: string;
    issueDate: string;
    dueDate: string | null;
    currencyCode: string;
    status: string;
    subtotal: number;
    taxTotal: number;
    grandTotal: number;
}

export interface InvoiceLineItemResponse {
    id: string;
    description: string;
    quantity: number;
    unitPrice: number;
    taxRate: number;
    lineSubtotal: number;
    taxAmount: number;
    lineTotal: number;
}

export interface InvoiceResponse extends InvoiceSummaryResponse {
    companyId: string;
    customerRegistrationNumber: string | null;
    customerTaxIdentificationNumber: string | null;
    createdAt: string;
    lineItems: InvoiceLineItemResponse[];
}

export interface InvoiceExportPartyResponse {
    name: string;
    registrationNumber: string | null;
    taxIdentificationNumber: string | null;
    salesAndServiceTaxNumber: string | null;
}

export interface InvoiceJsonExportResponse {
    invoiceId: string;
    companyId: string;
    invoiceNumber: string;
    status: string;
    issueDate: string;
    dueDate: string | null;
    currencyCode: string;
    seller: InvoiceExportPartyResponse;
    buyer: InvoiceExportPartyResponse;
    subtotal: number;
    taxTotal: number;
    grandTotal: number;
    lineItems: InvoiceLineItemResponse[];
}

export interface UpsertExpenseRequest {
    expenseDate: string;
    category: string;
    description: string;
    amount: number;
}

export interface ExpenseSummaryResponse {
    id: string;
    expenseDate: string;
    category: string;
    description: string;
    amount: number;
    createdAt: string;
}

export interface ExpenseResponse extends ExpenseSummaryResponse {
    companyId: string;
    createdByUserId: string | null;
}

export interface DashboardSummaryResponse {
    startDate: string;
    endDate: string;
    revenue: number;
    expenses: number;
    invoiceCount: number;
    outstandingBalance: number;
}

export interface DashboardTrendBucketResponse {
    monthStart: string;
    revenue: number;
    expenses: number;
}

export interface DashboardTrendsResponse {
    startMonth: string;
    endMonth: string;
    buckets: DashboardTrendBucketResponse[];
}

export interface FieldErrorBag {
    [key: string]: string[];
}

export class ApiRequestError extends Error {
    public readonly status: number | undefined;
    public readonly detail: string | undefined;
    public readonly fieldErrors: FieldErrorBag;

    public constructor(
        message: string,
        status?: number,
        detail?: string,
        fieldErrors: FieldErrorBag = {})
    {
        super(message);
        this.name = 'ApiRequestError';
        this.status = status;
        this.detail = detail;
        this.fieldErrors = fieldErrors;
    }
}

interface ProblemDetailsLike {
    title?: string;
    detail?: string;
    errors?: FieldErrorBag;
}

function normalizeError(error: unknown): ApiRequestError {
    if (axios.isAxiosError<ProblemDetailsLike>(error)) {
        const status = error.response?.status;
        const data = error.response?.data;
        const title = data?.title ?? error.message ?? 'Request failed.';
        const detail = data?.detail;
        const fieldErrors = data?.errors ?? {};

        return new ApiRequestError(title, status, detail, fieldErrors);
    }

    if (error instanceof Error) {
        return new ApiRequestError(error.message);
    }

    return new ApiRequestError('Unexpected request failure.');
}

async function getJson<T>(url: string, params?: Record<string, string | undefined>) {
    try {
        const response = await apiClient.get<T>(url, { params });
        return response.data;
    }
    catch (error) {
        throw normalizeError(error);
    }
}

async function postJson<T>(url: string, body: unknown) {
    try {
        const response = await apiClient.post<T>(url, body);
        return response.data;
    }
    catch (error) {
        throw normalizeError(error);
    }
}

async function putJson<T>(url: string, body: unknown) {
    try {
        const response = await apiClient.put<T>(url, body);
        return response.data;
    }
    catch (error) {
        throw normalizeError(error);
    }
}

async function deleteRequest(url: string) {
    try {
        await apiClient.delete(url);
    }
    catch (error) {
        throw normalizeError(error);
    }
}

export async function signIn(request: SignInRequest) {
    return await postJson<AuthResponse>('/api/auth/sign-in', request);
}

export async function getCurrentUser() {
    return await getJson<CurrentUserResponse>('/api/auth/me');
}

export async function listInvoices() {
    return await getJson<InvoiceSummaryResponse[]>('/api/invoices');
}

export async function getInvoice(id: string) {
    return await getJson<InvoiceResponse>(`/api/invoices/${id}`);
}

export async function createInvoice(request: UpsertInvoiceRequest) {
    return await postJson<InvoiceResponse>('/api/invoices', request);
}

export async function updateInvoice(id: string, request: UpsertInvoiceRequest) {
    return await putJson<InvoiceResponse>(`/api/invoices/${id}`, request);
}

export async function deleteInvoice(id: string) {
    await deleteRequest(`/api/invoices/${id}`);
}

export async function finalizeInvoice(id: string) {
    return await postJson<InvoiceResponse>(`/api/invoices/${id}/finalize`, null);
}

export async function exportInvoiceJson(id: string) {
    return await getJson<InvoiceJsonExportResponse>(`/api/invoices/${id}/export/json`);
}

export async function exportInvoiceCsv(id: string) {
    try {
        const response = await apiClient.get<string>(`/api/invoices/${id}/export/csv`, {
            responseType: 'text'
        });

        return response.data;
    }
    catch (error) {
        throw normalizeError(error);
    }
}

export async function listExpenses() {
    return await getJson<ExpenseSummaryResponse[]>('/api/expenses');
}

export async function getExpense(id: string) {
    return await getJson<ExpenseResponse>(`/api/expenses/${id}`);
}

export async function createExpense(request: UpsertExpenseRequest) {
    return await postJson<ExpenseResponse>('/api/expenses', request);
}

export async function updateExpense(id: string, request: UpsertExpenseRequest) {
    return await putJson<ExpenseResponse>(`/api/expenses/${id}`, request);
}

export async function deleteExpense(id: string) {
    await deleteRequest(`/api/expenses/${id}`);
}

export async function getDashboardSummary(startDate?: string, endDate?: string) {
    return await getJson<DashboardSummaryResponse>('/api/dashboard/summary', {
        startDate,
        endDate
    });
}

export async function getDashboardTrends(startMonth?: string, endMonth?: string) {
    return await getJson<DashboardTrendsResponse>('/api/dashboard/trends', {
        startMonth,
        endMonth
    });
}
