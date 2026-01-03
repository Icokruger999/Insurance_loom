using Microsoft.EntityFrameworkCore;
using InsuranceLoom.Api.Models.Entities;

namespace InsuranceLoom.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Core Tables
    public DbSet<User> Users { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }

    // User Type Tables
    public DbSet<Broker> Brokers { get; set; }
    public DbSet<PolicyHolder> PolicyHolders { get; set; }
    public DbSet<Manager> Managers { get; set; }

    // Policy Tables
    public DbSet<Policy> Policies { get; set; }

    // Document Tables
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<ServiceDocumentRequirement> ServiceDocumentRequirements { get; set; }

    // Approval Tables
    public DbSet<PolicyApproval> PolicyApprovals { get; set; }
    public DbSet<PolicyApprovalHistory> PolicyApprovalHistory { get; set; }

    // Other Tables
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<DebitOrder> DebitOrders { get; set; }
    public DbSet<CsvImport> CsvImports { get; set; }
    public DbSet<CsvImportError> CsvImportErrors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.UserType).HasMaxLength(20);
        });

        // Broker Configuration
        modelBuilder.Entity<Broker>(entity =>
        {
            entity.ToTable("brokers");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.HasIndex(e => e.AgentNumber).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Policy Holder Configuration
        modelBuilder.Entity<PolicyHolder>(entity =>
        {
            entity.ToTable("policy_holders");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.HasIndex(e => e.PolicyNumber).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Manager Configuration
        modelBuilder.Entity<Manager>(entity =>
        {
            entity.ToTable("managers");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Policy Configuration
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.ToTable("policies");
            entity.Property(e => e.PolicyHolderId).HasColumnName("policy_holder_id");
            entity.Property(e => e.BrokerId).HasColumnName("broker_id");
            entity.Property(e => e.ServiceTypeId).HasColumnName("service_type_id");
            entity.HasIndex(e => e.PolicyNumber).IsUnique();
            entity.HasOne(e => e.PolicyHolder)
                  .WithMany()
                  .HasForeignKey(e => e.PolicyHolderId);
            entity.HasOne(e => e.Broker)
                  .WithMany()
                  .HasForeignKey(e => e.BrokerId);
            entity.HasOne(e => e.ServiceType)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceTypeId);
        });

        // Document Configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasOne(e => e.DocumentType)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentTypeId);
            entity.HasOne(e => e.PolicyHolder)
                  .WithMany()
                  .HasForeignKey(e => e.PolicyHolderId);
            entity.HasOne(e => e.Policy)
                  .WithMany()
                  .HasForeignKey(e => e.PolicyId);
        });

        // Policy Approval Configuration
        modelBuilder.Entity<PolicyApproval>(entity =>
        {
            entity.ToTable("policy_approvals");
            entity.HasOne(e => e.Policy)
                  .WithMany()
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedManager)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedManagerId);
        });

        // CsvImport Configuration
        modelBuilder.Entity<CsvImport>(entity =>
        {
            entity.ToTable("csv_imports");
            entity.HasOne(e => e.Broker)
                  .WithMany()
                  .HasForeignKey(e => e.BrokerId);
        });

        // CsvImportError Configuration
        modelBuilder.Entity<CsvImportError>(entity =>
        {
            entity.ToTable("csv_import_errors");
            entity.HasOne(e => e.Import)
                  .WithMany()
                  .HasForeignKey(e => e.ImportId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Additional table mappings
        modelBuilder.Entity<ServiceType>().ToTable("service_types");
        modelBuilder.Entity<DocumentType>().ToTable("document_types");
        modelBuilder.Entity<ServiceDocumentRequirement>().ToTable("service_document_requirements");
        modelBuilder.Entity<PolicyApprovalHistory>().ToTable("policy_approval_history");
        modelBuilder.Entity<Claim>().ToTable("claims");
        modelBuilder.Entity<Payment>().ToTable("payments");
        modelBuilder.Entity<DebitOrder>().ToTable("debit_orders");
    }
}

