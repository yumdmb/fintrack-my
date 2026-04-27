using Fintrack.Api.Data;
using Fintrack.Api.Finance;
using Microsoft.EntityFrameworkCore;

namespace Fintrack.Api.Invoices;

public sealed class InvoiceService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyCollection<InvoiceSummaryResponse>> ListAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.CompanyId == companyId)
            .OrderByDescending(invoice => invoice.IssueDate)
            .ThenBy(invoice => invoice.InvoiceNumber)
            .Select(invoice => new InvoiceSummaryResponse(
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.CustomerName,
                invoice.IssueDate,
                invoice.DueDate,
                invoice.CurrencyCode,
                invoice.Status.ToString(),
                invoice.Subtotal,
                invoice.TaxTotal,
                invoice.GrandTotal))
            .ToListAsync(cancellationToken);
    }

    public async Task<InvoiceResponse?> GetAsync(
        Guid companyId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await FindInvoiceAsync(companyId, invoiceId, tracking: false, cancellationToken);

        return invoice is null ? null : Map(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(
        Guid companyId,
        UpsertInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoiceNumber = NormalizeRequired(request.InvoiceNumber);
        await ThrowIfInvoiceNumberExistsAsync(companyId, invoiceNumber, excludeInvoiceId: null, cancellationToken);

        var invoice = new Invoice
        {
            CompanyId = companyId,
            InvoiceNumber = invoiceNumber
        };

        Apply(invoice, request, replaceTrackedLineItems: false);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(invoice);
    }

    public async Task<InvoiceResponse?> UpdateAsync(
        Guid companyId,
        Guid invoiceId,
        UpsertInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoice = await FindInvoiceAsync(companyId, invoiceId, tracking: true, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        var invoiceNumber = NormalizeRequired(request.InvoiceNumber);
        await ThrowIfInvoiceNumberExistsAsync(companyId, invoiceNumber, invoiceId, cancellationToken);

        invoice.InvoiceNumber = invoiceNumber;
        Apply(invoice, request, replaceTrackedLineItems: true);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(invoice);
    }

    public async Task<bool> DeleteAsync(
        Guid companyId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await FindInvoiceAsync(companyId, invoiceId, tracking: true, cancellationToken);
        if (invoice is null)
        {
            return false;
        }

        dbContext.Invoices.Remove(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<Invoice?> FindInvoiceAsync(
        Guid companyId,
        Guid invoiceId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Invoices
            .Include(invoice => invoice.LineItems)
            .Where(invoice => invoice.CompanyId == companyId && invoice.Id == invoiceId);

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    private async Task ThrowIfInvoiceNumberExistsAsync(
        Guid companyId,
        string invoiceNumber,
        Guid? excludeInvoiceId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Invoices.AnyAsync(
            invoice => invoice.CompanyId == companyId
                && invoice.InvoiceNumber == invoiceNumber
                && (excludeInvoiceId == null || invoice.Id != excludeInvoiceId.Value),
            cancellationToken);

        if (exists)
        {
            throw new DuplicateInvoiceNumberException(invoiceNumber);
        }
    }

    private void Apply(Invoice invoice, UpsertInvoiceRequest request, bool replaceTrackedLineItems)
    {
        invoice.CustomerName = NormalizeRequired(request.CustomerName);
        invoice.CustomerRegistrationNumber = NormalizeOptional(request.CustomerRegistrationNumber);
        invoice.CustomerTaxIdentificationNumber = NormalizeOptional(request.CustomerTaxIdentificationNumber);
        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode);

        if (replaceTrackedLineItems)
        {
            dbContext.InvoiceLineItems.RemoveRange(invoice.LineItems);
        }

        invoice.LineItems.Clear();
        foreach (var requestLineItem in request.LineItems)
        {
            var lineItem = CreateLineItem(requestLineItem);
            invoice.LineItems.Add(lineItem);

            if (replaceTrackedLineItems)
            {
                dbContext.InvoiceLineItems.Add(lineItem);
            }
        }

        var totals = CalculateTotals(invoice.LineItems);
        invoice.Subtotal = totals.Subtotal;
        invoice.TaxTotal = totals.TaxTotal;
        invoice.GrandTotal = totals.GrandTotal;
    }

    private static InvoiceLineItem CreateLineItem(UpsertInvoiceLineItemRequest request)
    {
        var lineSubtotal = FinanceValueRules.RoundCurrency(request.Quantity * request.UnitPrice);
        var taxAmount = FinanceValueRules.RoundCurrency(lineSubtotal * request.TaxRate / 100m);

        return new InvoiceLineItem
        {
            Description = NormalizeRequired(request.Description),
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TaxRate = request.TaxRate,
            LineSubtotal = lineSubtotal,
            TaxAmount = taxAmount,
            LineTotal = FinanceValueRules.RoundCurrency(lineSubtotal + taxAmount)
        };
    }

    private static (decimal Subtotal, decimal TaxTotal, decimal GrandTotal) CalculateTotals(
        IEnumerable<InvoiceLineItem> lineItems)
    {
        return (
            FinanceValueRules.RoundCurrency(lineItems.Sum(lineItem => lineItem.LineSubtotal)),
            FinanceValueRules.RoundCurrency(lineItems.Sum(lineItem => lineItem.TaxAmount)),
            FinanceValueRules.RoundCurrency(lineItems.Sum(lineItem => lineItem.LineTotal)));
    }

    private static InvoiceResponse Map(Invoice invoice)
    {
        return new InvoiceResponse(
            invoice.Id,
            invoice.CompanyId,
            invoice.InvoiceNumber,
            invoice.CustomerName,
            invoice.CustomerRegistrationNumber,
            invoice.CustomerTaxIdentificationNumber,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.CurrencyCode,
            invoice.Status.ToString(),
            invoice.Subtotal,
            invoice.TaxTotal,
            invoice.GrandTotal,
            invoice.CreatedAt,
            [.. invoice.LineItems
                .OrderBy(lineItem => lineItem.Description)
                .Select(lineItem => new InvoiceLineItemResponse(
                    lineItem.Id,
                    lineItem.Description,
                    lineItem.Quantity,
                    lineItem.UnitPrice,
                    lineItem.TaxRate,
                    lineItem.LineSubtotal,
                    lineItem.TaxAmount,
                    lineItem.LineTotal))]);
    }

    private static string NormalizeRequired(string value) => value.Trim();

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeCurrencyCode(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "MYR" : value.Trim().ToUpperInvariant();
    }

}
