import { useEffect, useState } from 'react';
import {
    ApiRequestError,
    finalizeInvoice,
    exportInvoiceCsv,
    exportInvoiceJson,
    getDashboardSummary,
    getDashboardTrends,
    listInvoices,
    type DashboardSummaryResponse,
    type DashboardTrendsResponse,
    type InvoiceSummaryResponse
} from '../lib/api';
import {
    downloadJsonFile,
    downloadTextFile,
    formatCompactCurrency,
    formatCurrency,
    formatDate,
    formatMonth,
    toDateInputValue
} from '../lib/format';

function createSummaryRange() {
    const today = new Date();
    const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);
    const monthEnd = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    return {
        startDate: toDateInputValue(monthStart),
        endDate: toDateInputValue(monthEnd)
    };
}

function createTrendRange() {
    const today = new Date();
    const currentMonth = new Date(today.getFullYear(), today.getMonth(), 1);
    const startMonth = new Date(today.getFullYear(), today.getMonth() - 5, 1);

    return {
        startMonth: toDateInputValue(startMonth),
        endMonth: toDateInputValue(currentMonth)
    };
}

export function DashboardPage() {
    const [summaryRange, setSummaryRange] = useState(createSummaryRange);
    const [trendRange, setTrendRange] = useState(createTrendRange);
    const [summary, setSummary] = useState<DashboardSummaryResponse | null>(null);
    const [trends, setTrends] = useState<DashboardTrendsResponse | null>(null);
    const [invoices, setInvoices] = useState<InvoiceSummaryResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [pageError, setPageError] = useState<string | null>(null);
    const [actionMessage, setActionMessage] = useState<string | null>(null);
    const [actionError, setActionError] = useState<string | null>(null);
    const [busyInvoiceId, setBusyInvoiceId] = useState<string | null>(null);

    async function loadDashboard() {
        setIsLoading(true);
        setPageError(null);

        try {
            const [summaryResponse, trendsResponse, invoiceRows] = await Promise.all([
                getDashboardSummary(summaryRange.startDate, summaryRange.endDate),
                getDashboardTrends(trendRange.startMonth, trendRange.endMonth),
                listInvoices()
            ]);

            setSummary(summaryResponse);
            setTrends(trendsResponse);
            setInvoices(invoiceRows.slice(0, 6));
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setPageError(error.detail ?? error.message);
            }
            else {
                setPageError('Unable to load the dashboard.');
            }
        }
        finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        void loadDashboard();
    }, []);

    async function handleFinalize(invoiceId: string) {
        setBusyInvoiceId(invoiceId);
        setActionMessage(null);
        setActionError(null);

        try {
            const result = await finalizeInvoice(invoiceId);
            setActionMessage(`Invoice ${result.invoiceNumber} finalized and ready for export.`);
            await loadDashboard();
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setActionError(error.detail ?? error.message);
            }
            else {
                setActionError('Unable to finalize the invoice.');
            }
        }
        finally {
            setBusyInvoiceId(null);
        }
    }

    async function handleExportJson(invoice: InvoiceSummaryResponse) {
        setBusyInvoiceId(invoice.id);
        setActionMessage(null);
        setActionError(null);

        try {
            const payload = await exportInvoiceJson(invoice.id);
            downloadJsonFile(payload, `${invoice.invoiceNumber}.json`);
            setActionMessage(`Downloaded ${invoice.invoiceNumber}.json.`);
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setActionError(error.detail ?? error.message);
            }
            else {
                setActionError('Unable to export the invoice as JSON.');
            }
        }
        finally {
            setBusyInvoiceId(null);
        }
    }

    async function handleExportCsv(invoice: InvoiceSummaryResponse) {
        setBusyInvoiceId(invoice.id);
        setActionMessage(null);
        setActionError(null);

        try {
            const payload = await exportInvoiceCsv(invoice.id);
            downloadTextFile(payload, `${invoice.invoiceNumber}.csv`, 'text/csv;charset=utf-8');
            setActionMessage(`Downloaded ${invoice.invoiceNumber}.csv.`);
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setActionError(error.detail ?? error.message);
            }
            else {
                setActionError('Unable to export the invoice as CSV.');
            }
        }
        finally {
            setBusyInvoiceId(null);
        }
    }

    const trendMax = Math.max(
        1,
        ...(trends?.buckets.flatMap((bucket) => [bucket.revenue, bucket.expenses]) ?? [1])
    );

    const netPosition = summary ? summary.revenue - summary.expenses : 0;

    return (
        <section className="page-grid">
            <div className="page-header">
                <h1 className="page-header__title">Dashboard</h1>
                <p className="page-header__subtitle">Financial overview and export controls</p>
            </div>

            <section className="panel">
                <div className="section-bar">
                    <span className="section-bar__title">Reporting period</span>
                    <button className="button button--secondary" type="button" onClick={() => void loadDashboard()}>
                        Refresh
                    </button>
                </div>

                <div className="filters-grid">
                    <label className="field">
                        <span className="field__label">Summary start</span>
                        <input
                            className="field__input"
                            type="date"
                            value={summaryRange.startDate}
                            onChange={(event) =>
                                setSummaryRange((current) => ({ ...current, startDate: event.target.value }))
                            }
                        />
                    </label>

                    <label className="field">
                        <span className="field__label">Summary end</span>
                        <input
                            className="field__input"
                            type="date"
                            value={summaryRange.endDate}
                            onChange={(event) =>
                                setSummaryRange((current) => ({ ...current, endDate: event.target.value }))
                            }
                        />
                    </label>

                    <label className="field">
                        <span className="field__label">Trend start</span>
                        <input
                            className="field__input"
                            type="date"
                            value={trendRange.startMonth}
                            onChange={(event) =>
                                setTrendRange((current) => ({ ...current, startMonth: event.target.value }))
                            }
                        />
                    </label>

                    <label className="field">
                        <span className="field__label">Trend end</span>
                        <input
                            className="field__input"
                            type="date"
                            value={trendRange.endMonth}
                            onChange={(event) =>
                                setTrendRange((current) => ({ ...current, endMonth: event.target.value }))
                            }
                        />
                    </label>
                </div>
            </section>

            {pageError ? <p className="status status--error">{pageError}</p> : null}
            {actionMessage ? <p className="status status--success">{actionMessage}</p> : null}
            {actionError ? <p className="status status--error">{actionError}</p> : null}

            <section className="stats-row">
                <article className="stat-cell">
                    <span className="stat-cell__label">Revenue</span>
                    <strong className="stat-cell__value">{summary ? formatCompactCurrency(summary.revenue) : '--'}</strong>
                    <span className="stat-cell__note">
                        {summary ? `${formatDate(summary.startDate)} – ${formatDate(summary.endDate)}` : 'Loading...'}
                    </span>
                </article>

                <article className="stat-cell">
                    <span className="stat-cell__label">Expenses</span>
                    <strong className="stat-cell__value">{summary ? formatCompactCurrency(summary.expenses) : '--'}</strong>
                    <span className="stat-cell__note">Company outflow in period</span>
                </article>

                <article className="stat-cell">
                    <span className="stat-cell__label">Net position</span>
                    <strong className="stat-cell__value">{summary ? formatCompactCurrency(netPosition) : '--'}</strong>
                    <span className="stat-cell__note">Revenue minus expenses</span>
                </article>

                <article className="stat-cell">
                    <span className="stat-cell__label">Outstanding</span>
                    <strong className="stat-cell__value">{summary ? formatCompactCurrency(summary.outstandingBalance) : '--'}</strong>
                    <span className="stat-cell__note">Finalized invoice balance</span>
                </article>
            </section>

            <section className="dashboard-grid">
                <section className="panel">
                    <div className="section-bar">
                        <span className="section-bar__title">Revenue vs. expense trend</span>
                        <span className="metric-note">
                            {summary ? `${summary.invoiceCount} invoices` : ''}
                        </span>
                    </div>

                    {isLoading ? (
                        <p className="empty-state">Loading trends...</p>
                    ) : (
                        <div className="trend-chart">
                            {trends?.buckets.map((bucket) => (
                                <article key={bucket.monthStart} className="trend-column">
                                    <div className="trend-column__bars">
                                        <div
                                            className="trend-bar trend-bar--revenue"
                                            style={{ height: `${Math.max((bucket.revenue / trendMax) * 100, 6)}%` }}
                                        />
                                        <div
                                            className="trend-bar trend-bar--expense"
                                            style={{ height: `${Math.max((bucket.expenses / trendMax) * 100, 6)}%` }}
                                        />
                                    </div>
                                    <strong>{formatMonth(bucket.monthStart)}</strong>
                                    <span>R {formatCurrency(bucket.revenue)}</span>
                                    <span>E {formatCurrency(bucket.expenses)}</span>
                                </article>
                            ))}
                        </div>
                    )}
                </section>

                <section className="panel">
                    <div className="section-bar">
                        <span className="section-bar__title">Export desk</span>
                    </div>

                    <div className="ledger-list">
                        {invoices.length === 0 ? (
                            <p className="empty-state">Create invoices first to activate the export desk.</p>
                        ) : (
                            invoices.map((invoice) => (
                                <article key={invoice.id} className="ledger-row">
                                    <div className="ledger-row__primary">
                                        <div>
                                            <span className="eyebrow">{invoice.status}</span>
                                            <h4>{invoice.invoiceNumber}</h4>
                                            <p>{invoice.customerName}</p>
                                        </div>
                                        <div className="ledger-row__meta">
                                            <span>{formatDate(invoice.issueDate)}</span>
                                            <strong>{formatCurrency(invoice.grandTotal, invoice.currencyCode)}</strong>
                                        </div>
                                    </div>

                                    <div className="action-row">
                                        {invoice.status !== 'Finalized' ? (
                                            <button
                                                className="button button--secondary"
                                                disabled={busyInvoiceId === invoice.id}
                                                type="button"
                                                onClick={() => void handleFinalize(invoice.id)}
                                            >
                                                {busyInvoiceId === invoice.id ? 'Working...' : 'Finalize'}
                                            </button>
                                        ) : null}

                                        <button
                                            className="button button--ghost"
                                            disabled={busyInvoiceId === invoice.id}
                                            type="button"
                                            onClick={() => void handleExportJson(invoice)}
                                        >
                                            JSON
                                        </button>

                                        <button
                                            className="button button--ghost"
                                            disabled={busyInvoiceId === invoice.id}
                                            type="button"
                                            onClick={() => void handleExportCsv(invoice)}
                                        >
                                            CSV
                                        </button>
                                    </div>
                                </article>
                            ))
                        )}
                    </div>
                </section>
            </section>
        </section>
    );
}
