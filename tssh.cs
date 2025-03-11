public class TransactionType
{
    public int TransactionTypeId { get; set; }
    public string TypeName { get; set; }

    // Relacionamentos
    public ICollection<Transaction> Transactions { get; set; }
}

public class Transaction
{
    public int TransactionId { get; set; }
    public int TransactionTypeId { get; set; }
    public int? TermId { get; set; }
    public string DocumentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    // Relacionamentos
    public TransactionType TransactionType { get; set; }
    public Term Term { get; set; }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.ToTable("TransactionType");

        builder.HasKey(t => t.TransactionTypeId);

        builder.Property(t => t.TransactionTypeId)
               .HasColumnName("TransactionTypeID");

        builder.Property(t => t.TypeName)
               .HasMaxLength(50)
               .IsRequired();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.TransactionId)
               .HasColumnName("TransactionID");

        builder.Property(t => t.TransactionTypeId)
               .HasColumnName("TransactionTypeID");

        builder.Property(t => t.TermId)
               .HasColumnName("TermID");

        builder.Property(t => t.DocumentNumber)
               .HasMaxLength(50)
               .IsRequired();

        // Em PostgreSQL, "numeric(18,2)" é normalmente aceito
        builder.Property(t => t.Amount)
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        builder.Property(t => t.CreatedDate)
               .IsRequired();

        builder.Property(t => t.UpdatedDate)
               .IsRequired(false);

        builder.Property(t => t.DeletedDate)
               .IsRequired(false);

        // Relacionamentos
        builder.HasOne(t => t.TransactionType)
               .WithMany(tt => tt.Transactions)
               .HasForeignKey(t => t.TransactionTypeId);

        builder.HasOne(t => t.Term)
               .WithMany(term => term.Transactions)
               .HasForeignKey(t => t.TermId);
    }
}


using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TransactionType> TransactionTypes { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<CustomerTermFingerprint> CustomerTermFingerprints { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<RevenueGovernance> RevenueGovernances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TransactionTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerTermFingerprintConfiguration());
        modelBuilder.ApplyConfiguration(new TermConfiguration());
        modelBuilder.ApplyConfiguration(new RevenueGovernanceConfiguration());
    }
}


