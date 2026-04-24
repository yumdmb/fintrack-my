using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fintrack.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();

    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>(entity =>
        {
            entity.Property(company => company.Name).HasMaxLength(200).IsRequired();
            entity.Property(company => company.RegistrationNumber).HasMaxLength(64);
            entity.Property(company => company.TaxIdentificationNumber).HasMaxLength(64);
            entity.Property(company => company.SalesAndServiceTaxNumber).HasMaxLength(64);
            entity.Property(company => company.DefaultCurrencyCode).HasMaxLength(3).IsRequired();
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(user => user.Company)
                .WithMany(company => company.Users)
                .HasForeignKey(user => user.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(invoice => new { invoice.CompanyId, invoice.InvoiceNumber }).IsUnique();
            entity.Property(invoice => invoice.InvoiceNumber).HasMaxLength(64).IsRequired();
            entity.Property(invoice => invoice.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(invoice => invoice.CustomerRegistrationNumber).HasMaxLength(64);
            entity.Property(invoice => invoice.CustomerTaxIdentificationNumber).HasMaxLength(64);
            entity.Property(invoice => invoice.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(invoice => invoice.Subtotal).HasPrecision(18, 2);
            entity.Property(invoice => invoice.TaxTotal).HasPrecision(18, 2);
            entity.Property(invoice => invoice.GrandTotal).HasPrecision(18, 2);
            entity.Property(invoice => invoice.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasOne(invoice => invoice.Company)
                .WithMany(company => company.Invoices)
                .HasForeignKey(invoice => invoice.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InvoiceLineItem>(entity =>
        {
            entity.Property(lineItem => lineItem.Description).HasMaxLength(300).IsRequired();
            entity.Property(lineItem => lineItem.Quantity).HasPrecision(18, 4);
            entity.Property(lineItem => lineItem.UnitPrice).HasPrecision(18, 2);
            entity.Property(lineItem => lineItem.TaxRate).HasPrecision(7, 4);
            entity.Property(lineItem => lineItem.LineSubtotal).HasPrecision(18, 2);
            entity.Property(lineItem => lineItem.TaxAmount).HasPrecision(18, 2);
            entity.Property(lineItem => lineItem.LineTotal).HasPrecision(18, 2);
            entity.HasOne(lineItem => lineItem.Invoice)
                .WithMany(invoice => invoice.LineItems)
                .HasForeignKey(lineItem => lineItem.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Expense>(entity =>
        {
            entity.Property(expense => expense.Category).HasMaxLength(100).IsRequired();
            entity.Property(expense => expense.Description).HasMaxLength(500).IsRequired();
            entity.Property(expense => expense.Amount).HasPrecision(18, 2);
            entity.HasOne(expense => expense.Company)
                .WithMany(company => company.Expenses)
                .HasForeignKey(expense => expense.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(expense => expense.CreatedByUser)
                .WithMany()
                .HasForeignKey(expense => expense.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
