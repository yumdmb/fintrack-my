import { useEffect, useState } from 'react';
import {
    ApiRequestError,
    createExpense,
    deleteExpense,
    getExpense,
    listExpenses,
    updateExpense,
    type ExpenseResponse,
    type ExpenseSummaryResponse,
    type UpsertExpenseRequest
} from '../lib/api';
import { formatCurrency, formatDate, formatDateTime, toDateInputValue } from '../lib/format';

interface ExpenseDraft {
    expenseDate: string;
    category: string;
    description: string;
    amount: string;
}

function createEmptyExpenseDraft(): ExpenseDraft {
    return {
        expenseDate: toDateInputValue(new Date()),
        category: '',
        description: '',
        amount: '0.00'
    };
}

function expenseToDraft(expense: ExpenseResponse): ExpenseDraft {
    return {
        expenseDate: expense.expenseDate,
        category: expense.category,
        description: expense.description,
        amount: expense.amount.toString()
    };
}

function buildRequest(draft: ExpenseDraft): UpsertExpenseRequest {
    return {
        expenseDate: draft.expenseDate,
        category: draft.category,
        description: draft.description,
        amount: Number.parseFloat(draft.amount || '0')
    };
}

export function ExpensesPage() {
    const [expenses, setExpenses] = useState<ExpenseSummaryResponse[]>([]);
    const [selectedExpenseId, setSelectedExpenseId] = useState<string | null>(null);
    const [selectedExpense, setSelectedExpense] = useState<ExpenseResponse | null>(null);
    const [draft, setDraft] = useState<ExpenseDraft>(createEmptyExpenseDraft);
    const [mode, setMode] = useState<'create' | 'edit'>('create');
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [statusMessage, setStatusMessage] = useState<string | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

    async function refreshExpenses(preferredExpenseId?: string | null, stayInCreateMode = false) {
        setIsLoading(true);
        setErrorMessage(null);

        try {
            const rows = await listExpenses();
            setExpenses(rows);

            if (preferredExpenseId) {
                setMode('edit');
                setSelectedExpenseId(preferredExpenseId);
                return;
            }

            if (stayInCreateMode) {
                return;
            }

            if (rows.length === 0) {
                setMode('create');
                setSelectedExpenseId(null);
                setSelectedExpense(null);
                setDraft(createEmptyExpenseDraft());
                return;
            }

            if (!selectedExpenseId || !rows.some((expense) => expense.id === selectedExpenseId)) {
                setMode('edit');
                setSelectedExpenseId(rows[0].id);
            }
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setErrorMessage(error.detail ?? error.message);
            }
            else {
                setErrorMessage('Unable to load expenses.');
            }
        }
        finally {
            setIsLoading(false);
        }
    }

    async function loadExpense(expenseId: string) {
        setIsLoading(true);
        setErrorMessage(null);

        try {
            const expense = await getExpense(expenseId);
            setSelectedExpense(expense);
            setDraft(expenseToDraft(expense));
        }
        catch (error) {
            if (error instanceof ApiRequestError) {
                setErrorMessage(error.detail ?? error.message);
            }
            else {
                setErrorMessage('Unable to load expense details.');
            }
        }
        finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        void refreshExpenses();
    }, []);

    useEffect(() => {
        if (mode === 'edit' && selectedExpenseId) {
            void loadExpense(selectedExpenseId);
        }
    }, [selectedExpenseId, mode]);

    function clearMessages() {
        setStatusMessage(null);
        setErrorMessage(null);
        setFieldErrors({});
    }

    function startNewExpense() {
        clearMessages();
        setMode('create');
        setSelectedExpenseId(null);
        setSelectedExpense(null);
        setDraft(createEmptyExpenseDraft());
    }

    function getFieldError(field: string) {
        return fieldErrors[field]?.[0];
    }

    function applyApiError(error: unknown, fallback: string) {
        if (error instanceof ApiRequestError) {
            setErrorMessage(error.detail ?? error.message);
            setFieldErrors(error.fieldErrors);
            return;
        }

        setErrorMessage(fallback);
    }

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();
        clearMessages();
        setIsSubmitting(true);

        try {
            const expense = mode === 'create'
                ? await createExpense(buildRequest(draft))
                : await updateExpense(selectedExpenseId!, buildRequest(draft));

            setMode('edit');
            setSelectedExpenseId(expense.id);
            setSelectedExpense(expense);
            setDraft(expenseToDraft(expense));
            setStatusMessage(
                mode === 'create'
                    ? `Expense "${expense.category}" created.`
                    : `Expense "${expense.category}" updated.`
            );
            await refreshExpenses(expense.id);
        }
        catch (error) {
            applyApiError(error, 'Unable to save the expense.');
        }
        finally {
            setIsSubmitting(false);
        }
    }

    async function handleDelete() {
        if (!selectedExpenseId || !window.confirm('Delete this expense?')) {
            return;
        }

        clearMessages();
        setIsSubmitting(true);

        try {
            await deleteExpense(selectedExpenseId);
            setStatusMessage('Expense deleted.');
            startNewExpense();
            await refreshExpenses(null, true);
        }
        catch (error) {
            applyApiError(error, 'Unable to delete the expense.');
        }
        finally {
            setIsSubmitting(false);
        }
    }

    return (
        <section className="workspace-grid">
            <aside className="panel">
                <div className="section-bar">
                    <span className="section-bar__title">Expenses</span>
                    <button className="button button--secondary" type="button" onClick={startNewExpense}>
                        New
                    </button>
                </div>

                <div className="ledger-list">
                    {expenses.length === 0 ? (
                        <p className="empty-state">No expenses yet. Record the first one.</p>
                    ) : (
                        expenses.map((expense) => (
                            <button
                                key={expense.id}
                                className={expense.id === selectedExpenseId ? 'ledger-item ledger-item--active' : 'ledger-item'}
                                type="button"
                                onClick={() => {
                                    clearMessages();
                                    setMode('edit');
                                    setSelectedExpenseId(expense.id);
                                }}
                            >
                                <div>
                                    <span className="ledger-item__label">{expense.category}</span>
                                    <strong>{expense.description}</strong>
                                    <p>{formatDate(expense.expenseDate)}</p>
                                </div>
                                <div className="ledger-item__meta">
                                    <strong>{formatCurrency(expense.amount)}</strong>
                                </div>
                            </button>
                        ))
                    )}
                </div>
            </aside>

            <section className="panel">
                <div className="section-heading">
                    <div>
                        <span className="eyebrow">{mode === 'create' ? 'New expense' : 'Edit expense'}</span>
                        <h3>{mode === 'create' ? 'Record expense' : selectedExpense?.category ?? 'Expense'}</h3>
                    </div>

                    {mode === 'edit' ? (
                        <button className="button button--danger" disabled={isSubmitting} type="button" onClick={() => void handleDelete()}>
                            Delete
                        </button>
                    ) : null}
                </div>

                {statusMessage ? <p className="status status--success">{statusMessage}</p> : null}
                {errorMessage ? <p className="status status--error">{errorMessage}</p> : null}
                {isLoading ? <p className="empty-state">Loading expense data...</p> : null}

                <form className="form-stack" onSubmit={handleSubmit}>
                    <div className="form-grid form-grid--three">
                        <label className="field">
                            <span className="field__label">Date</span>
                            <input
                                className="field__input"
                                type="date"
                                value={draft.expenseDate}
                                onChange={(event) => setDraft((current) => ({ ...current, expenseDate: event.target.value }))}
                            />
                        </label>

                        <label className="field">
                            <span className="field__label">Category</span>
                            <input
                                className="field__input"
                                type="text"
                                value={draft.category}
                                onChange={(event) => setDraft((current) => ({ ...current, category: event.target.value }))}
                            />
                            {getFieldError('Category') ? <small className="field__error">{getFieldError('Category')}</small> : null}
                        </label>

                        <label className="field">
                            <span className="field__label">Amount</span>
                            <input
                                className="field__input"
                                step="0.01"
                                type="number"
                                value={draft.amount}
                                onChange={(event) => setDraft((current) => ({ ...current, amount: event.target.value }))}
                            />
                            {getFieldError('Amount') ? <small className="field__error">{getFieldError('Amount')}</small> : null}
                        </label>
                    </div>

                    <label className="field">
                        <span className="field__label">Description</span>
                        <textarea
                            className="field__input field__input--textarea"
                            value={draft.description}
                            onChange={(event) => setDraft((current) => ({ ...current, description: event.target.value }))}
                        />
                        {getFieldError('Description') ? <small className="field__error">{getFieldError('Description')}</small> : null}
                    </label>

                    <section className="totals-panel">
                        <div className="section-bar">
                            <span className="section-bar__title">Summary</span>
                        </div>

                        <dl className="totals-list">
                            <div>
                                <dt>Amount</dt>
                                <dd>{formatCurrency(Number.parseFloat(draft.amount || '0'))}</dd>
                            </div>
                        </dl>

                        {selectedExpense ? (
                            <div className="detail-note">
                                <span>Created: {formatDateTime(selectedExpense.createdAt)}</span>
                                <span>By: {selectedExpense.createdByUserId ?? 'N/A'}</span>
                            </div>
                        ) : null}
                    </section>

                    <div className="action-row">
                        <button className="button button--primary" disabled={isSubmitting} type="submit">
                            {isSubmitting ? 'Saving...' : mode === 'create' ? 'Create expense' : 'Save changes'}
                        </button>
                    </div>
                </form>
            </section>
        </section>
    );
}
