using System.Linq.Expressions;
using Crm.Domain.Common;
using Crm.Domain.Entities;
using Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Crm.Infrastructure.Persistence;

public class CrmDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SpecialPrice> SpecialPrices => Set<SpecialPrice>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>().Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Entity<Contact>().Property(x => x.Email).HasMaxLength(200);
        builder.Entity<Lead>().Property(x => x.Email).HasMaxLength(200);
        builder.Entity<Deal>().Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Entity<Product>().Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
        builder.Entity<SpecialPrice>().Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Entity<Quote>().Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Entity<QuoteItem>().Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Entity<QuoteItem>().Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        builder.Entity<Invoice>().Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Entity<Invoice>().Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Entity<InvoiceItem>().Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Entity<InvoiceItem>().Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        builder.Entity<Payment>().Property(x => x.Amount).HasColumnType("decimal(18,2)");

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(CrmDbContext).GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var genericMethod = method!.MakeGenericMethod(entityType.ClrType);
                genericMethod.Invoke(null, new object[] { builder });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDeleteRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDeleteRules();
        return base.SaveChanges();
    }

    private void ApplyAuditAndSoftDeleteRules()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }
    }
}
