namespace Crm.Web.Models;

public static class ModuleRegistry
{
    public static readonly IReadOnlyDictionary<string, ModuleInfo> Modules = new Dictionary<string, ModuleInfo>(StringComparer.OrdinalIgnoreCase)
    {
        ["companies"] = new("Companies", "/api/companies"),
        ["contacts"] = new("Contacts", "/api/contacts"),
        ["leads"] = new("Leads", "/api/leads"),
        ["deals"] = new("Deals", "/api/deals"),
        ["categories"] = new("Categories", "/api/categories"),
        ["products"] = new("Products", "/api/products"),
        ["special-prices"] = new("Special Prices", "/api/special-prices"),
        ["quotes"] = new("Quotes", "/api/quotes"),
        ["quote-items"] = new("Quote Items", "/api/quote-items"),
        ["invoices"] = new("Invoices", "/api/invoices"),
        ["invoice-items"] = new("Invoice Items", "/api/invoice-items"),
        ["payments"] = new("Payments", "/api/payments"),
        ["activities"] = new("Activities", "/api/activities")
    };
}

public record ModuleInfo(string DisplayName, string ApiPath);
