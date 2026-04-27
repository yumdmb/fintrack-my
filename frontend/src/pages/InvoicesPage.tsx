import { useEffect, useState } from 'react';
import {
    ApiRequestError,
    createInvoice,
    deleteInvoice,
    exportInvoiceCsv,
    exportInvoiceJson,
    finalizeInvoice,
    getInvoice,
    listInvoices,
    updateInvoice,
    type InvoiceResponse,
    type InvoiceSummaryResponse,
    type UpsertInvoiceRequest
} from '../lib/api';
import {
    downloadJsonFile,
    downloadTextFile,
    formatCurrency,
    formatDate,
    formatDateTime,
    toDateInputValue
} from '../lib/format';

interface InvoiceDraftLineItem {
    description: string;
    quantity: string;
    unitPrice: string;
    taxRate: string;
}

interface InvoiceDraft {
    invoiceNumber: string;
    customerName: string;
    customerRegistrationNumber: string;
    customerTaxIdentificationNumber: string;
    issueDate: string;
    dueDate: string;
    currencyCode: string;
    lineItems: InvoiceDraftLineItem[];
}

function createEmptyLineItem(): InvoiceDraftLineItem {
    return {
        description: '',
        quantity: '1',
        unitPrice: '0',
        taxRate: '6'
    };
}

function createEmptyInvoiceDraft(): InvoiceDraft {
    const issueDate = new Date();
    const dueDate = new Date();
    dueDate.setDate(dueDate.getDate() + 30);

    return {
        invoiceNumber: '',
        customerName: '',
        customerRegistrationNumber: '',
        customerTaxIdentificationNumber: '',
        issueDate: toDateInputValue(issueDate),
        dueDate: toDateInputValue(dueDate),
        currencyCode: 'MYR',
        lineItems: [createEmptyLineItem()]
    };
}

function invoiceToDraft(invoice: InvoiceResponse): InvoiceDraft {
    return {
        invoiceNumber: invoice.invoiceNumber,
        customerName: invoice.customerName,
        customerRegistrationNumber: invoice.customerRegistrationNumber ?? '',
        customerTaxIdentificationNumber: invoice.customerTaxIdentificationNumber ?? '',
        issueDate: invoice.issueDate,
        dueDate: invoice.dueDate ?? '',
        currencyCode: invoice.currencyCode,
        lineItems: invoice.lineItems.map((lineItem) => ({
            description: lineItem.description,
            quantity: lineItem.quantity.toString(),
            unitPrice: lineItem.unitPrice.toString(),
            taxRate: lineItem.taxRate.toString()
        }))
    };
}

function toNumber(value: string) {
    const parsed = Number.parseFloat(value);
    return Number.isFinite(parsed) ? parsed : 0;
}

function roundCurrency(value: number) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
}

function buildRequest(draft: InvoiceDraft): UpsertInvoiceRequest {
    return {
        invoiceNumber: draft.invoiceNumber,
        customerName: draft.customerName,
        customerRegistrationNumber: draft.customerRegistrationNumber.trim() || null,
        customerTaxIdentificationNumber: draft.customerTaxIdentificationNumber.trim() || null,
        issueDate: draft.issueDate,
        dueDate: draft.dueDate || null,
        currencyCode: draft.currencyCode.trim() || null,
        lineItems: draft.lineItems.map((lineItem) => ({
            description: lineItem.description,
            quantity: toNumber(lineItem.quantity),
            unitPrice: toNumber(lineItem.unitPrice),
            taxRate: toNumber(lineItem.taxRate)
        }))
    };
}

function calculateEstimate(draft: InvoiceDraft) {
    const lines = draft.lineItems.map((lineItem) => {
        const quantity = toNumber(lineItem.quantity);
        const unitPrice = toNumber(lineItem.unitPrice);
        const taxRate = toNumber(lineItem.taxRate);
        const lineSubtotal = roundCurrency(quantity * unitPrice);
        const taxAmount = roundCurrency(lineSubtotal * taxRate / 100);
        const lineTotal = roundCurrency(lineSubtotal + taxAmount);

        return { lineSubtotal, taxAmount, lineTotal };
    });

    return {
        subtotal: roundCurrency(lines.reduce((total, line) => total + line.lineSubtotal, 0)),
        taxTotal: roundCurrency(lines.reduce((total, line) => total + line.taxAmount, 0)),
        grandTotal: roundCurrency(lines.reduce((total, line) => total + line.lineTotal, 0))
    };
}

export function InvoicesPage() {
    const [invoiceSummaries, setInvoiceSummaries] = useState<InvoiceSummaryResponse[]>([]);
    const [selectedInvoiceId, setSelectedInvoiceId] = useState<string | null>(null);
    const [selectedInvoice, setSelectedInvoice] = useState<InvoiceResponse | null>(null);
    const [draft, setDraft] = useState<InvoiceDraft>(createEmptyInvoiceDraft);
    const [mode, setMode] = useState<'create' | 'edit'>('create');
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [statusMessage, setStatusMessage] = useState<string | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

    async function refreshInvoices(preferredInvoiceId?: string | null, stayInCreateMode = false) {
        setIsLoading(true);
        setErrorMessage(null);

        try {
            const rows = await listInvoices();
            setInvoiceSummaries(rows);

            if (preferredInvoiceId) {
                setMode('edit');
                setSelectedInvoiceId(preferredInvoiceId);
                return;
            }

            if (stayInCreateMode) {
                return;
            }

            if (rows.length === 0) {
                setMode('create');
                setSelectedInvoiceId(null);
                setSelectedInvoice(null);
                setDraft(createEmptyInvoiceDraft());
                return;
            }

            if (!selectedInvoiceId || !rows.some((invoice) => invoice.id === selectedInvoiceId)) {
                setMode('edit');
                setSelectedInvoiceId(rows[0].id);
            }
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setErrorMessage(error.detail ?? error.message);
            }
            else {
                setErrorMessage('Unable to load invoices.');
            }
        }
        finally {
            setIsLoading(false);
        }
    }

    async function loadInvoice(invoiceId: string) {
        setIsLoading(true);
        setErrorMessage(null);

        try {
            const invoice = await getInvoice(invoiceId);
            setSelectedInvoice(invoice);
            setDraft(invoiceToDraft(invoice));
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setErrorMessage(error.detail ?? error.message);
            }
            else {
                setErrorMessage('Unable to load invoice details.');
            }
        }
        finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        void refreshInvoices();
    }, []);

    useEffect(() => {
        if (mode === 'edit' && selectedInvoiceId) {
            void loadInvoice(selectedInvoiceId);
        }
    }, [selectedInvoiceId, mode]);

    function clearMessages() {
        setStatusMessage(null);
        setErrorMessage(null);
        setFieldErrors({});
    }

    function startNewInvoice() {
        clearMessages();
        setMode('create');
        setSelectedInvoiceId(null);
        setSelectedInvoice(null);
        setDraft(createEmptyInvoiceDraft());
    }

    function applyApiError(error: unknown, fallback: string) {
        if (error instanceof ApiRequestError) {
            setErrorMessage(error.detail ?? error.message);
            setFieldErrors(error.fieldErrors);
            return;
        }

        setErrorMessage(fallback);
    }

    function getFieldError(field: string) {
        return fieldErrors[field]?.[0];
    }

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();
        clearMessages();
        setIsSubmitting(true);

        try {
            const request = buildRequest(draft);
            const invoice = mode === 'create'
                ? await createInvoice(request)
                : await updateInvoice(selectedInvoiceId!, request);

            setMode('edit');
            setSelectedInvoiceId(invoice.id);
            setSelectedInvoice(invoice);
            setDraft(invoiceToDraft(invoice));
            setStatusMessage(
                mode === 'create'
                    ? `Invoice ${invoice.invoiceNumber} created.`
                    : `Invoice ${invoice.invoiceNumber} updated.`
            );
            await refreshInvoices(invoice.id);
        }
        catch (error) {
            applyApiError(error, 'Unable to save the invoice.');
        }
        finally {
            setIsSubmitting(false);
        }
    }

    async function handleDelete() {
        if (!selectedInvoiceId || !window.confirm('Delete this invoice?')) {
            return;
        }

        clearMessages();
        setIsSubmitting(true);

        try {
            await deleteInvoice(selectedInvoiceId);
            setStatusMessage('Invoice deleted.');
            startNewInvoice();
            await refreshInvoices(null, true);
        }
        catch (error) {
            applyApiError(error, 'Unable to delete the invoice.');
        }
        finally {
            setIsSubmitting(false);
        }
    }

    async function handleFinalize() {
        if (!selectedInvoiceId) {
            return;
        }

        clearMessages();
        setIsSubmitting(true);

        try {
            const invoice = await finalizeInvoice(selectedInvoiceId);
            setSelectedInvoice(invoice);
            setDraft(invoiceToDraft(invoice));
            setStatusMessage(`Invoice ${invoice.invoiceNumber} finalized.`);
            await refreshInvoices(invoice.id);
        }
        catch (error) {
            applyApiError(error, 'Unable to finalize the invoice.');
        }
        finally {
            setIsSubmitting(false);
        }
    }

    async function handleExportJson() {
        if (!selectedInvoiceId || !selectedInvoice) {
            return;
        }

        clearMessages();
        setIsSubmitting(true);

        try {
            const payload = await exportInvoiceJson(selectedInvoiceId);
            downloadJsonFile(payload, `${selectedInvoice.invoiceNumber}.json`);
            setStatusMessage(`Downloaded ${selectedInvoice.invoiceNumber}.json.`);
        }
        catch (error) {
            applyApiError(error, 'Unable to export JSON.');
        }
        finally {
            setIsSubmitting(false);
        }
    }

    async function handleExportCsv() {
        if (!selectedInvoiceId || !selectedInvoice) {
            return;
        }

        clearMessages();
        setIsSubmitting(true);

        try {
            const payload = await exportInvoiceCsv(selectedInvoiceId);
            downloadTextFile(payload, `${selectedInvoice.invoiceNumber}.csv`, 'text/csv;charset=utf-8');
            setStatusMessage(`Downloaded ${selectedInvoice.invoiceNumber}.csv.`);
        }
        catch (error) {
            applyApiError(error, 'Unable to export CSV.');
        }
        finally {
            setIsSubmitting(false);
        }
    }

    const estimate = calculateEstimate(draft);

    return (
        <section className="workspace-grid">
            <aside className="panel">
                <div className="section-bar">
                    <span className="section-bar__title">Invoices</span>
                    <button className="button button--secondary" type="button" onClick={startNewInvoice}>
                        New
                    </button>
                </div>

                <div className="ledger-list">
                    {invoiceSummaries.length === 0 ? (
                        <p className="empty-state">No invoices yet. Start with the first one.</p>
                    ) : (
                        invoiceSummaries.map((invoice) => (
                            <button
                                key={invoice.id}
                                className={invoice.id === selectedInvoiceId ? 'ledger-item ledger-item--active' : 'ledger-item'}
                                type="button"
                                onClick={() => {
                                    clearMessages();
                                    setMode('edit');
                                    setSelectedInvoiceId(invoice.id);
                                }}
                            >
                                <div>
                                    <span className="ledger-item__label">{invoice.status}</span>
                                    <strong>{invoice.invoiceNumber}</strong>
                                    <p>{invoice.customerName}</p>
                                </div>
                                <div className="ledger-item__meta">
                                    <span>{formatDate(invoice.issueDate)}</span>
                                    <strong>{formatCurrency(invoice.grandTotal, invoice.currencyCode)}</strong>
                                </div>
                            </button>
                        ))
                    )}
                </div>
            </aside>

            <section className="panel">
                <div className="section-heading">
                    <div>
                        <span className="eyebrow">{mode === 'create' ? 'New invoice' : selectedInvoice?.status ?? 'Invoice'}</span>
                        <h3>{mode === 'create' ? 'Create invoice' : selectedInvoice?.invoiceNumber ?? 'Invoice'}</h3>
                    </div>

                    <div className="action-row">
                        {mode === 'edit' && selectedInvoice?.status !== 'Finalized' ? (
                            <button className="button button--secondary" disabled={isSubmitting} type="button" onClick={() => void handleFinalize()}>
                                Finalize
                            </button>
                        ) : null}

                        {mode === 'edit' ? (
                            <>
                                <button className="button button--ghost" disabled={isSubmitting} type="button" onClick={() => void handleExportJson()}>
                                    JSON
                                </button>
                                <button className="button button--ghost" disabled={isSubmitting} type="button" onClick={() => void handleExportCsv()}>
                                    CSV
                                </button>
                                <button className="button button--danger" disabled={isSubmitting} type="button" onClick={() => void handleDelete()}>
                                    Delete
                                </button>
                            </>
                        ) : null}
                    </div>
                </div>

                {statusMessage ? <p className="status status--success">{statusMessage}</p> : null}
                {errorMessage ? <p className="status status--error">{errorMessage}</p> : null}
                {isLoading ? <p className="empty-state">Loading invoice data...</p> : null}

                <form className="form-stack" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        <label className="field">
                            <span className="field__label">Invoice number</span>
                            <input
                                className="field__input"
                                type="text"
                                value={draft.invoiceNumber}
                                onChange={(event) => setDraft((current) => ({ ...current, invoiceNumber: event.target.value }))}
                            />
                            {getFieldError('InvoiceNumber') ? <small className="field__error">{getFieldError('InvoiceNumber')}</small> : null}
                        </label>

                        <label className="field">
                            <span className="field__label">Customer name</span>
                            <input
                                className="field__input"
                                type="text"
                                value={draft.customerName}
                                onChange={(event) => setDraft((current) => ({ ...current, customerName: event.target.value }))}
                            />
                            {getFieldError('CustomerName') ? <small className="field__error">{getFieldError('CustomerName')}</small> : null}
                        </label>

                        <label className="field">
                            <span className="field__label">Registration no.</span>
                            <input
                                className="field__input"
                                type="text"
                                value={draft.customerRegistrationNumber}
                                onChange={(event) =>
                                    setDraft((current) => ({ ...current, customerRegistrationNumber: event.target.value }))
                                }
                            />
                        </label>

                        <label className="field">
                            <span className="field__label">TIN</span>
                            <input
                                className="field__input"
                                type="text"
                                value={draft.customerTaxIdentificationNumber}
                                onChange={(event) =>
                                    setDraft((current) => ({ ...current, customerTaxIdentificationNumber: event.target.value }))
                                }
                            />
                        </label>

                        <label className="field">
                            <span className="field__label">Issue date</span>
                            <input
                                className="field__input"
                                type="date"
                                value={draft.issueDate}
                                onChange={(event) => setDraft((current) => ({ ...current, issueDate: event.target.value }))}
                            />
                        </label>

                        <label className="field">
                            <span className="field__label">Due date</span>
                            <input
                                className="field__input"
                                type="date"
                                value={draft.dueDate}
                                onChange={(event) => setDraft((current) => ({ ...current, dueDate: event.target.value }))}
                            />
                            {getFieldError('DueDate') ? <small className="field__error">{getFieldError('DueDate')}</small> : null}
                        </label>

                        <label className="field">
                            <span className="field__label">Currency</span>
                            <input
                                className="field__input"
                                maxLength={3}
                                type="text"
                                value={draft.currencyCode}
                                onChange={(event) => setDraft((current) => ({ ...current, currencyCode: event.target.value.toUpperCase() }))}
                            />
                        </label>
                    </div>

                    <section className="line-items-panel">
                        <div className="section-bar">
                            <span className="section-bar__title">Line items</span>
                            <button
                                className="button button--secondary"
                                type="button"
                                onClick={() =>
                                    setDraft((current) => ({
                                        ...current,
                                        lineItems: [...current.lineItems, createEmptyLineItem()]
                                    }))
                                }
                            >
                                Add line
                            </button>
                        </div>

                        <div className="line-item-table">
                            {draft.lineItems.map((lineItem, index) => (
                                <div key={`${index}-${lineItem.description}`} className="line-item-row">
                                    <label className="field line-item-row__description">
                                        <span className="field__label">Description</span>
                                        <input
                                            className="field__input"
                                            type="text"
                                            value={lineItem.description}
                                            onChange={(event) =>
                                                setDraft((current) => ({
                                                    ...current,
                                                    lineItems: current.lineItems.map((item, itemIndex) =>
                                                        itemIndex === index
                                                            ? { ...item, description: event.target.value }
                                                            : item)
                                                }))
                                            }
                                        />
                                        {getFieldError(`LineItems[${index}].Description`) ? (
                                            <small className="field__error">{getFieldError(`LineItems[${index}].Description`)}</small>
                                        ) : null}
                                    </label>

                                    <label className="field">
                                        <span className="field__label">Qty</span>
                                        <input
                                            className="field__input"
                                            step="0.0001"
                                            type="number"
                                            value={lineItem.quantity}
                                            onChange={(event) =>
                                                setDraft((current) => ({
                                                    ...current,
                                                    lineItems: current.lineItems.map((item, itemIndex) =>
                                                        itemIndex === index
                                                            ? { ...item, quantity: event.target.value }
                                                            : item)
                                                }))
                                            }
                                        />
                                        {getFieldError(`LineItems[${index}].Quantity`) ? (
                                            <small className="field__error">{getFieldError(`LineItems[${index}].Quantity`)}</small>
                                        ) : null}
                                    </label>

                                    <label className="field">
                                        <span className="field__label">Unit price</span>
                                        <input
                                            className="field__input"
                                            step="0.01"
                                            type="number"
                                            value={lineItem.unitPrice}
                                            onChange={(event) =>
                                                setDraft((current) => ({
                                                    ...current,
                                                    lineItems: current.lineItems.map((item, itemIndex) =>
                                                        itemIndex === index
                                                            ? { ...item, unitPrice: event.target.value }
                                                            : item)
                                                }))
                                            }
                                        />
                                        {getFieldError(`LineItems[${index}].UnitPrice`) ? (
                                            <small className="field__error">{getFieldError(`LineItems[${index}].UnitPrice`)}</small>
                                        ) : null}
                                    </label>

                                    <label className="field">
                                        <span className="field__label">Tax %</span>
                                        <input
                                            className="field__input"
                                            step="0.0001"
                                            type="number"
                                            value={lineItem.taxRate}
                                            onChange={(event) =>
                                                setDraft((current) => ({
                                                    ...current,
                                                    lineItems: current.lineItems.map((item, itemIndex) =>
                                                        itemIndex === index
                                                            ? { ...item, taxRate: event.target.value }
                                                            : item)
                                                }))
                                            }
                                        />
                                        {getFieldError(`LineItems[${index}].TaxRate`) ? (
                                            <small className="field__error">{getFieldError(`LineItems[${index}].TaxRate`)}</small>
                                        ) : null}
                                    </label>

                                    <button
                                        className="button button--ghost line-item-row__remove"
                                        disabled={draft.lineItems.length === 1}
                                        type="button"
                                        onClick={() =>
                                            setDraft((current) => ({
                                                ...current,
                                                lineItems: current.lineItems.filter((_, itemIndex) => itemIndex !== index)
                                            }))
                                        }
                                    >
                                        Remove
                                    </button>
                                </div>
                            ))}
                        </div>
                    </section>

                    <section className="totals-panel">
                        <div className="section-bar">
                            <span className="section-bar__title">Estimated totals</span>
                        </div>

                        <dl className="totals-list">
                            <div>
                                <dt>Subtotal</dt>
                                <dd>{formatCurrency(estimate.subtotal, draft.currencyCode || 'MYR')}</dd>
                            </div>
                            <div>
                                <dt>Tax</dt>
                                <dd>{formatCurrency(estimate.taxTotal, draft.currencyCode || 'MYR')}</dd>
                            </div>
                            <div>
                                <dt>Grand total</dt>
                                <dd>{formatCurrency(estimate.grandTotal, draft.currencyCode || 'MYR')}</dd>
                            </div>
                        </dl>

                        {selectedInvoice ? (
                            <div className="detail-note">
                                <span>Status: {selectedInvoice.status}</span>
                                <span>Created: {formatDateTime(selectedInvoice.createdAt)}</span>
                            </div>
                        ) : null}
                    </section>

                    <div className="action-row">
                        <button className="button button--primary" disabled={isSubmitting} type="submit">
                            {isSubmitting ? 'Saving...' : mode === 'create' ? 'Create invoice' : 'Save changes'}
                        </button>
                    </div>
                </form>
            </section>
        </section>
    );
}
