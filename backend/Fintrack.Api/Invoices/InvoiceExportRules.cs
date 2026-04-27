using Fintrack.Api.Data;

namespace Fintrack.Api.Invoices;

internal static class InvoiceExportRules
{
    public static IReadOnlyDictionary<string, string[]> GetFinalizationErrors(Invoice invoice)
    {
        return GetComplianceErrors(invoice);
    }

    public static IReadOnlyDictionary<string, string[]> GetExportErrors(Invoice invoice)
    {
        var errors = CopyErrors(GetComplianceErrors(invoice));

        if (invoice.Status != InvoiceStatus.Finalized)
        {
            AddError(errors, "status", "Invoice must be finalized before export.");
        }

        return ToReadOnly(errors);
    }

    private static IReadOnlyDictionary<string, string[]> GetComplianceErrors(Invoice invoice)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        if (invoice.IssueDate == default)
        {
            AddError(errors, "issueDate", "Invoice issue date is required.");
        }

        if (string.IsNullOrWhiteSpace(invoice.CurrencyCode))
        {
            AddError(errors, "currencyCode", "Invoice currency code is required.");
        }

        if (invoice.LineItems.Count == 0)
        {
            AddError(errors, "lineItems", "Invoice must contain at least one line item.");
        }

        if (string.IsNullOrWhiteSpace(invoice.Company.Name))
        {
            AddError(errors, "seller.name", "Seller company name is required.");
        }

        if (string.IsNullOrWhiteSpace(invoice.Company.RegistrationNumber))
        {
            AddError(errors, "seller.registrationNumber", "Seller registration number is required.");
        }

        if (string.IsNullOrWhiteSpace(invoice.Company.TaxIdentificationNumber))
        {
            AddError(errors, "seller.taxIdentificationNumber", "Seller tax identification number is required.");
        }

        if (string.IsNullOrWhiteSpace(invoice.Company.DefaultCurrencyCode))
        {
            AddError(errors, "seller.defaultCurrencyCode", "Seller default currency code is required.");
        }

        return ToReadOnly(errors);
    }

    private static Dictionary<string, List<string>> CopyErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        var copy = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var (key, values) in errors)
        {
            copy[key] = [.. values];
        }

        return copy;
    }

    private static IReadOnlyDictionary<string, string[]> ToReadOnly(Dictionary<string, List<string>> errors)
    {
        return errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var values))
        {
            values = [];
            errors[key] = values;
        }

        values.Add(message);
    }
}
