using System.Globalization;
using System.Text;
using Fintrack.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Fintrack.Api.Invoices;

public sealed class InvoiceExportService(ApplicationDbContext dbContext)
{
    public async Task<bool> FinalizeAsync(
        Guid companyId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await FindInvoiceAsync(companyId, invoiceId, tracking: true, cancellationToken);
        if (invoice is null)
        {
            return false;
        }

        ThrowIfInvalid(InvoiceExportRules.GetFinalizationErrors(invoice));

        if (invoice.Status != InvoiceStatus.Finalized)
        {
            invoice.Status = InvoiceStatus.Finalized;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<InvoiceJsonExportResponse?> GetJsonAsync(
        Guid companyId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await FindInvoiceAsync(companyId, invoiceId, tracking: false, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        ThrowIfInvalid(InvoiceExportRules.GetExportErrors(invoice));
        return MapJson(invoice);
    }

    public async Task<string?> GetCsvAsync(
        Guid companyId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await FindInvoiceAsync(companyId, invoiceId, tracking: false, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        ThrowIfInvalid(InvoiceExportRules.GetExportErrors(invoice));
        return BuildCsv(invoice);
    }

    private async Task<Invoice?> FindInvoiceAsync(
        Guid companyId,
        Guid invoiceId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Invoices
            .Include(invoice => invoice.Company)
            .Include(invoice => invoice.LineItems)
            .Where(invoice => invoice.CompanyId == companyId && invoice.Id == invoiceId);

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    private static void ThrowIfInvalid(IReadOnlyDictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
        {
            throw new InvoiceExportValidationException(errors);
        }
    }

    private static InvoiceJsonExportResponse MapJson(Invoice invoice)
    {
        return new InvoiceJsonExportResponse(
            invoice.Id,
            invoice.CompanyId,
            invoice.InvoiceNumber,
            invoice.Status.ToString(),
            invoice.IssueDate,
            invoice.DueDate,
            invoice.CurrencyCode,
            new InvoiceExportPartyResponse(
                invoice.Company.Name,
                invoice.Company.RegistrationNumber,
                invoice.Company.TaxIdentificationNumber,
                invoice.Company.SalesAndServiceTaxNumber),
            new InvoiceExportPartyResponse(
                invoice.CustomerName,
                invoice.CustomerRegistrationNumber,
                invoice.CustomerTaxIdentificationNumber,
                null),
            invoice.Subtotal,
            invoice.TaxTotal,
            invoice.GrandTotal,
            [.. invoice.LineItems
                .OrderBy(lineItem => lineItem.Description)
                .Select(lineItem => new InvoiceExportLineItemResponse(
                    lineItem.Id,
                    lineItem.Description,
                    lineItem.Quantity,
                    lineItem.UnitPrice,
                    lineItem.TaxRate,
                    lineItem.LineSubtotal,
                    lineItem.TaxAmount,
                    lineItem.LineTotal))]);
    }

    private static string BuildCsv(Invoice invoice)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',',
            "InvoiceNumber",
            "Status",
            "IssueDate",
            "DueDate",
            "CurrencyCode",
            "SellerName",
            "SellerRegistrationNumber",
            "SellerTaxIdentificationNumber",
            "SellerSalesAndServiceTaxNumber",
            "CustomerName",
            "CustomerRegistrationNumber",
            "CustomerTaxIdentificationNumber",
            "LineDescription",
            "Quantity",
            "UnitPrice",
            "TaxRate",
            "LineSubtotal",
            "TaxAmount",
            "LineTotal",
            "InvoiceSubtotal",
            "InvoiceTaxTotal",
            "InvoiceGrandTotal"));

        foreach (var lineItem in invoice.LineItems.OrderBy(item => item.Description))
        {
            builder.AppendLine(string.Join(',',
                Escape(invoice.InvoiceNumber),
                Escape(invoice.Status.ToString()),
                Escape(invoice.IssueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Escape(invoice.DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty),
                Escape(invoice.CurrencyCode),
                Escape(invoice.Company.Name),
                Escape(invoice.Company.RegistrationNumber),
                Escape(invoice.Company.TaxIdentificationNumber),
                Escape(invoice.Company.SalesAndServiceTaxNumber),
                Escape(invoice.CustomerName),
                Escape(invoice.CustomerRegistrationNumber),
                Escape(invoice.CustomerTaxIdentificationNumber),
                Escape(lineItem.Description),
                Escape(FormatDecimal(lineItem.Quantity)),
                Escape(FormatDecimal(lineItem.UnitPrice)),
                Escape(FormatDecimal(lineItem.TaxRate)),
                Escape(FormatDecimal(lineItem.LineSubtotal)),
                Escape(FormatDecimal(lineItem.TaxAmount)),
                Escape(FormatDecimal(lineItem.LineTotal)),
                Escape(FormatDecimal(invoice.Subtotal)),
                Escape(FormatDecimal(invoice.TaxTotal)),
                Escape(FormatDecimal(invoice.GrandTotal))));
        }

        return builder.ToString();
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
