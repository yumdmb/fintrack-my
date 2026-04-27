export function formatCurrency(amount: number, currencyCode = 'MYR') {
    return new Intl.NumberFormat('en-MY', {
        style: 'currency',
        currency: currencyCode,
        maximumFractionDigits: 2
    }).format(amount);
}

export function formatCompactCurrency(amount: number, currencyCode = 'MYR') {
    return new Intl.NumberFormat('en-MY', {
        style: 'currency',
        currency: currencyCode,
        notation: 'compact',
        maximumFractionDigits: 1
    }).format(amount);
}

export function formatDate(value: string) {
    return new Intl.DateTimeFormat('en-MY', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
    }).format(new Date(`${value}T00:00:00`));
}

export function formatDateTime(value: string) {
    return new Intl.DateTimeFormat('en-MY', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(value));
}

export function formatMonth(value: string) {
    return new Intl.DateTimeFormat('en-MY', {
        month: 'short',
        year: 'numeric'
    }).format(new Date(`${value}T00:00:00`));
}

export function toDateInputValue(date: Date) {
    return date.toISOString().slice(0, 10);
}

export function downloadTextFile(content: string, fileName: string, contentType: string) {
    const blob = new Blob([content], { type: contentType });
    const objectUrl = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(objectUrl);
}

export function downloadJsonFile(payload: unknown, fileName: string) {
    downloadTextFile(JSON.stringify(payload, null, 2), fileName, 'application/json');
}
