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
    public DbSet<Company> Companies { get; set; }

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
    public DbSet<BrokerApprovalHistory> BrokerApprovalHistory { get; set; }

    // Other Tables
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<DebitOrder> DebitOrders { get; set; }
    public DbSet<Beneficiary> Beneficiaries { get; set; }
    public DbSet<CsvImport> CsvImports { get; set; }
    public DbSet<CsvImportError> CsvImportErrors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.UserType).HasColumnName("user_type").HasMaxLength(20);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Broker Configuration
        modelBuilder.Entity<Broker>(entity =>
        {
            entity.ToTable("brokers");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AgentNumber).HasColumnName("agent_number");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.CompanyName).HasColumnName("company_name");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number");
            entity.Property(e => e.CommissionRate).HasColumnName("commission_rate");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
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
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PolicyNumber).HasColumnName("policy_number");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.MiddleName).HasColumnName("middle_name");
            entity.Property(e => e.IdNumber).HasColumnName("id_number");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Birthplace).HasColumnName("birthplace");
            entity.Property(e => e.Sex).HasColumnName("sex").HasMaxLength(20);
            entity.Property(e => e.CivilStatus).HasColumnName("civil_status").HasMaxLength(50);
            entity.Property(e => e.Occupation).HasColumnName("occupation");
            entity.Property(e => e.MonthlyIncome).HasColumnName("monthly_income");
            entity.Property(e => e.MonthlyExpenses).HasColumnName("monthly_expenses");
            entity.Property(e => e.EmploymentType).HasColumnName("employment_type").HasMaxLength(50);
            entity.Property(e => e.IncomeTaxNumber).HasColumnName("income_tax_number");
            entity.Property(e => e.EmploymentStartDate).HasColumnName("employment_start_date");
            entity.Property(e => e.EmploymentEndDate).HasColumnName("employment_end_date");
            entity.Property(e => e.AgencyName).HasColumnName("agency_name");
            entity.Property(e => e.AgencyContactNo).HasColumnName("agency_contact_no");
            entity.Property(e => e.AgencyAddress).HasColumnName("agency_address");
            entity.Property(e => e.AgencyEmail).HasColumnName("agency_email");
            entity.Property(e => e.AgencySignatory).HasColumnName("agency_signatory");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.PolicyNumber).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Beneficiary Configuration
        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("beneficiaries");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PolicyHolderId).HasColumnName("policy_holder_id");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Relationship).HasColumnName("relationship").HasMaxLength(100);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.PolicyHolder)
                  .WithMany()
                  .HasForeignKey(e => e.PolicyHolderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Policy)
                  .WithMany()
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Manager Configuration
        modelBuilder.Entity<Manager>(entity =>
        {
            entity.ToTable("managers");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.EmployeeNumber).HasColumnName("employee_number");
            entity.Property(e => e.Department).HasColumnName("department");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CanApprovePolicies).HasColumnName("can_approve_policies");
            entity.Property(e => e.CanManageBrokers).HasColumnName("can_manage_brokers");
            entity.Property(e => e.CanViewReports).HasColumnName("can_view_reports");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Policy Configuration
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.ToTable("policies");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PolicyNumber).HasColumnName("policy_number");
            entity.Property(e => e.PolicyHolderId).HasColumnName("policy_holder_id");
            entity.Property(e => e.BrokerId).HasColumnName("broker_id");
            entity.Property(e => e.ServiceTypeId).HasColumnName("service_type_id");
            entity.Property(e => e.ServiceCode).HasColumnName("service_code");
            entity.Property(e => e.CoverageAmount).HasColumnName("coverage_amount");
            entity.Property(e => e.PremiumAmount).HasColumnName("premium_amount");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
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
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.PolicyHolderId).HasColumnName("policy_holder_id");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.BrokerId).HasColumnName("broker_id");
            entity.Property(e => e.FileName).HasColumnName("file_name");
            entity.Property(e => e.OriginalFileName).HasColumnName("original_file_name");
            entity.Property(e => e.FilePath).HasColumnName("file_path");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.FileType).HasColumnName("file_type");
            entity.Property(e => e.FileExtension).HasColumnName("file_extension");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");
            entity.Property(e => e.UploadDate).HasColumnName("upload_date");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.IsExpired).HasColumnName("is_expired");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");
            entity.Property(e => e.VerifiedDate).HasColumnName("verified_date");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.IsEncrypted).HasColumnName("is_encrypted");
            entity.Property(e => e.Checksum).HasColumnName("checksum");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.Tags).HasColumnName("tags");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
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

        // ServiceType Configuration
        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.ToTable("service_types");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ServiceCode).HasColumnName("service_code");
            entity.Property(e => e.ServiceName).HasColumnName("service_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // Company Configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // DocumentType Configuration  
        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("document_types");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DocumentCode).HasColumnName("document_code");
            entity.Property(e => e.DocumentName).HasColumnName("document_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ServiceTypeId).HasColumnName("service_type_id");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.IsOptional).HasColumnName("is_optional");
            entity.Property(e => e.MaxFileSizeMb).HasColumnName("max_file_size_mb");
            entity.Property(e => e.AllowedFileTypes).HasColumnName("allowed_file_types");
            entity.Property(e => e.ValidityPeriodDays).HasColumnName("validity_period_days");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // Broker Approval History Configuration
        modelBuilder.Entity<BrokerApprovalHistory>(entity =>
        {
            entity.ToTable("broker_approval_history");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BrokerId).HasColumnName("broker_id");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(50);
            entity.Property(e => e.PerformedByManagerId).HasColumnName("performed_by_manager_id");
            entity.Property(e => e.PerformedByEmail).HasColumnName("performed_by_email").HasMaxLength(255);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PreviousStatus).HasColumnName("previous_status").HasMaxLength(50);
            entity.Property(e => e.NewStatus).HasColumnName("new_status").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.HasOne(e => e.Broker)
                  .WithMany()
                  .HasForeignKey(e => e.BrokerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PerformedByManager)
                  .WithMany()
                  .HasForeignKey(e => e.PerformedByManagerId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Additional table mappings (minimal - add column mappings if errors occur)
        modelBuilder.Entity<ServiceDocumentRequirement>().ToTable("service_document_requirements");
        modelBuilder.Entity<PolicyApprovalHistory>().ToTable("policy_approval_history");
        modelBuilder.Entity<Claim>().ToTable("claims");
        modelBuilder.Entity<Payment>().ToTable("payments");
        modelBuilder.Entity<DebitOrder>().ToTable("debit_orders");
    }
}

