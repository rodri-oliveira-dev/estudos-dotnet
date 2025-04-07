// Entities/Transaction.cs
using System;
using System.Collections.Generic;

namespace YourNamespace.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int TransactionTypeId { get; set; }
        public int TierId { get; set; }
        public int DocumentNumber { get; set; }
        public string EconomicGroupFederalId { get; set; }
        public string PolicyHolderFederalId { get; set; }
        public decimal Amount { get; set; }
        public decimal ReserveAmount { get; set; }
        public int? TransactionReferenceId { get; set; }
        public bool Refund { get; set; }
        public DateTime CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Deleted { get; set; }

        // Navegações
        public TransactionType TransactionType { get; set; }
        public Tier Tier { get; set; }
        public Transaction TransactionReference { get; set; }
        public ICollection<Transaction> ChildTransactions { get; set; }
    }
}




// Entities/TransactionType.cs
using System.Collections.Generic;

namespace YourNamespace.Entities
{
    public class TransactionType
    {
        public int Id { get; set; }
        public string TypeName { get; set; }

        // Navegação
        public ICollection<Transaction> Transactions { get; set; }
    }
}


// Entities/Tier.cs
using System;
using System.Collections.Generic;

namespace YourNamespace.Entities
{
    public class Tier
    {
        public int Id { get; set; }
        public int ReinsuranceContractId { get; set; }
        public string Name { get; set; }
        public decimal RetentionPercentage { get; set; }
        public decimal TransferPercentage { get; set; }
        public int Sequence { get; set; }
        public decimal StartRange { get; set; }
        public decimal EndRange { get; set; }
        public decimal ZeroRetentionRange { get; set; }
        public bool SubLimit { get; set; }
        public bool Additional { get; set; }
        public bool ZeroRetention { get; set; }
        public bool TechnicalLimitSUSEP { get; set; }
        public bool AllowShadow { get; set; }
        public DateTime CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Deleted { get; set; }

        // Navegações
        public ReinsuranceContract ReinsuranceContract { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<CustomerTierAggregate> CustomerTierAggregates { get; set; }
    }
}



// Entities/ReinsuranceContract.cs
using System;
using System.Collections.Generic;

namespace YourNamespace.Entities
{
    public class ReinsuranceContract
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Sequence { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime CompletionDate { get; set; }
        public string RegistrationLogin { get; set; }
        public string CompletionLogin { get; set; }
        public decimal ContractPriorityValue { get; set; }
        public bool IsolatedAccumulation { get; set; }
        public string NCNumber { get; set; }
        public DateTime NCDate { get; set; }
        public DateTime AcceptanceDate { get; set; }
        public DateTime FormalizationDate { get; set; }
        public decimal AdministrativeExpensesPercentage { get; set; }
        public decimal PLRPercentage { get; set; }
        public decimal Rate { get; set; }
        public bool Active { get; set; }
        public string DiscoveryPaymentInterval { get; set; }
        public DateTime CreatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Deleted { get; set; }

        // Navegações
        public ICollection<Tier> Tiers { get; set; }
        public ICollection<CustomerReinsuranceContractAggregate> CustomerReinsuranceContractAggregates { get; set; }
    }
}


// Entities/CustomerTierAggregate.cs
namespace YourNamespace.Entities
{
    public class CustomerTierAggregate
    {
        public int Id { get; set; }
        public int TierId { get; set; }
        public string EconomicGroupFederalId { get; set; }
        public decimal JudicialLimitRange { get; set; }
        public decimal Balance { get; set; }

        // Navegação
        public Tier Tier { get; set; }
    }
}



// Entities/CustomerReinsuranceContractAggregate.cs
namespace YourNamespace.Entities
{
    public class CustomerReinsuranceContractAggregate
    {
        public int Id { get; set; }
        public int ReinsuranceContractsId { get; set; }
        public string EconomicGroupFederalId { get; set; }
        public decimal ReserveBalance { get; set; }

        // Navegação
        public ReinsuranceContract ReinsuranceContract { get; set; }
    }
}



// Configurations/TransactionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNamespace.Entities;

namespace YourNamespace.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(t => t.TransactionTypeId).IsRequired();
            builder.Property(t => t.TierId).IsRequired();
            builder.Property(t => t.DocumentNumber).IsRequired();
            builder.Property(t => t.EconomicGroupFederalId)
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(t => t.PolicyHolderFederalId)
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(t => t.Amount).IsRequired();
            builder.Property(t => t.ReserveAmount).IsRequired();
            builder.Property(t => t.TransactionReferenceId).IsRequired(false);
            builder.Property(t => t.Refund).IsRequired();
            builder.Property(t => t.CreatedUser).IsRequired();
            builder.Property(t => t.CreatedDate).IsRequired();
            builder.Property(t => t.UpdatedUser).IsRequired(false);
            builder.Property(t => t.UpdatedDate).IsRequired(false);
            builder.Property(t => t.Deleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasOne(t => t.TransactionType)
                .WithMany(tt => tt.Transactions)
                .HasForeignKey(t => t.TransactionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Tier)
                .WithMany(ti => ti.Transactions)
                .HasForeignKey(t => t.TierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.TransactionReference)
                .WithMany(tr => tr.ChildTransactions)
                .HasForeignKey(t => t.TransactionReferenceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


// Configurations/TransactionTypeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNamespace.Entities;

namespace YourNamespace.Configurations
{
    public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
    {
        public void Configure(EntityTypeBuilder<TransactionType> builder)
        {
            builder.ToTable("TransactionTypes");

            builder.HasKey(tt => tt.Id);
            builder.Property(tt => tt.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(tt => tt.TypeName)
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}



// Configurations/TierConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNamespace.Entities;

namespace YourNamespace.Configurations
{
    public class TierConfiguration : IEntityTypeConfiguration<Tier>
    {
        public void Configure(EntityTypeBuilder<Tier> builder)
        {
            builder.ToTable("Tiers");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(t => t.ReinsuranceContractId).IsRequired();
            builder.Property(t => t.Name)
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(t => t.RetentionPercentage).IsRequired();
            builder.Property(t => t.TransferPercentage).IsRequired();
            builder.Property(t => t.Sequence).IsRequired();
            builder.Property(t => t.StartRange).IsRequired();
            builder.Property(t => t.EndRange).IsRequired();
            builder.Property(t => t.ZeroRetentionRange).IsRequired();
            builder.Property(t => t.SubLimit).IsRequired();
            builder.Property(t => t.Additional).IsRequired();
            builder.Property(t => t.ZeroRetention).IsRequired();
            builder.Property(t => t.TechnicalLimitSUSEP).IsRequired();
            builder.Property(t => t.AllowShadow).IsRequired();
            builder.Property(t => t.CreatedUser).IsRequired();
            builder.Property(t => t.CreatedDate).IsRequired();
            builder.Property(t => t.UpdatedUser).IsRequired(false);
            builder.Property(t => t.UpdatedDate).IsRequired(false);
            builder.Property(t => t.Deleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasOne(t => t.ReinsuranceContract)
                .WithMany(rc => rc.Tiers)
                .HasForeignKey(t => t.ReinsuranceContractId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}




// Configurations/TierConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNamespace.Entities;

namespace YourNamespace.Configurations
{
    public class TierConfiguration : IEntityTypeConfiguration<Tier>
    {
        public void Configure(EntityTypeBuilder<Tier> builder)
        {
            builder.ToTable("Tiers");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(t => t.ReinsuranceContractId).IsRequired();
            builder.Property(t => t.Name)
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(t => t.RetentionPercentage).IsRequired();
            builder.Property(t => t.TransferPercentage).IsRequired();
            builder.Property(t => t.Sequence).IsRequired();
            builder.Property(t => t.StartRange).IsRequired();
            builder.Property(t => t.EndRange).IsRequired();
            builder.Property(t => t.ZeroRetentionRange).IsRequired();
            builder.Property(t => t.SubLimit).IsRequired();
            builder.Property(t => t.Additional).IsRequired();
            builder.Property(t => t.ZeroRetention).IsRequired();
            builder.Property(t => t.TechnicalLimitSUSEP).IsRequired();
            builder.Property(t => t.AllowShadow).IsRequired();
            builder.Property(t => t.CreatedUser).IsRequired();
            builder.Property(t => t.CreatedDate).IsRequired();
            builder.Property(t => t.UpdatedUser).IsRequired(false);
            builder.Property(t => t.UpdatedDate).IsRequired(false);
            builder.Property(t => t.Deleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasOne(t => t.ReinsuranceContract)
                .WithMany(rc => rc.Tiers)
                .HasForeignKey(t => t.ReinsuranceContractId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}



// Configurations/ReinsuranceContractConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNamespace.Entities;

namespace YourNamespace.Configurations
{
    public class ReinsuranceContractConfiguration : IEntityTypeConfiguration<ReinsuranceContract>
    {
        public void Configure(EntityTypeBuilder<ReinsuranceContract> builder)
        {
            builder.ToTable("ReinsuranceContracts");

            builder.HasKey(rc => rc.Id);
            builder.Property(rc => rc.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(rc => rc.Description)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(rc => rc.StartDate).IsRequired();
            builder.Property(rc => rc.EndDate).IsRequired();
            builder.Property(rc => rc.Sequence).IsRequired();
            builder.Property(rc => rc.RegistrationDate).IsRequired();
            builder.Property(rc => rc.CompletionDate).IsRequired();
            builder.Property(rc => rc.RegistrationLogin)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(rc => rc.CompletionLogin)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(rc => rc.ContractPriorityValue).IsRequired();
            builder.Property(rc => rc.IsolatedAccumulation).IsRequired();
            builder.Property(rc => rc.NCNumber)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(rc => rc.NCDate).IsRequired();
            builder.Property(rc => rc.AcceptanceDate).IsRequired();
            builder.Property(rc => rc.FormalizationDate).IsRequired();
            builder.Property(rc => rc.AdministrativeExpensesPercentage)
                .HasDefaultValue(0)
                .IsRequired();
            builder.Property(rc => rc.PLRPercentage)
                .HasDefaultValue(0)
                .IsRequired();
            builder.Property(rc => rc.Rate)
                .HasColumnType("decimal(12,2)")
                .IsRequired();
            builder.Property(rc => rc.Active).IsRequired();
            builder.Property(rc => rc.DiscoveryPaymentInterval)
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(rc => rc.CreatedUser).IsRequired();
            builder.Property(rc => rc.CreatedDate).IsRequired();
            builder.Property(rc => rc.UpdatedUser).IsRequired(false);
            builder.Property(rc => rc.UpdatedDate).IsRequired(false);
            builder.Property(rc => rc.Deleted)
                .HasDefaultValue(false)
                .IsRequired();
        }
    }
}




// Configurations/CustomerTierAggregateConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNamespace.Entities;

namespace YourNamespace.Configurations
{
    public class CustomerTierAggregateConfiguration : IEntityTypeConfiguration<CustomerTierAggregate>
    {
        public void Configure(EntityTypeBuilder<CustomerTierAggregate> builder)
        {
            builder.ToTable("CustomerTierAggregates");

            builder.HasKey(cta => cta.Id);
            builder.Property(cta => cta.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(cta => cta.TierId).IsRequired();
            builder.Property(cta => cta.EconomicGroupFederalId)
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(cta => cta.JudicialLimitRange).IsRequired();
            builder.Property(cta => cta.Balance).IsRequired();

            builder.HasOne(cta => cta.Tier)
                .WithMany(t => t.CustomerTierAggregates)
                .HasForeignKey(cta => cta.TierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
// Configurations/CustomerReinsuranceContractAggregateConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNamespace.Entities;

namespace YourNamespace.Configurations
{
    public class CustomerReinsuranceContractAggregateConfiguration : IEntityTypeConfiguration<CustomerReinsuranceContractAggregate>
    {
        public void Configure(EntityTypeBuilder<CustomerReinsuranceContractAggregate> builder)
        {
            builder.ToTable("CustomerReinsuranceContractAggregates");

            builder.HasKey(cra => cra.Id);
            builder.Property(cra => cra.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(cra => cra.ReinsuranceContractsId).IsRequired();
            builder.Property(cra => cra.EconomicGroupFederalId)
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(cra => cra.ReserveBalance).IsRequired();

            builder.HasOne(cra => cra.ReinsuranceContract)
                .WithMany(rc => rc.CustomerReinsuranceContractAggregates)
                .HasForeignKey(cra => cra.ReinsuranceContractsId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

