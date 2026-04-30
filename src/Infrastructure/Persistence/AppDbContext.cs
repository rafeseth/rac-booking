using RacBooking.Application.Interfaces;
using RacBooking.Domain.Entities;
using RacBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Persistence;

/// <summary>EF Core context for PostgreSQL. Applies global tenant filters when <see cref="CurrentTenantId"/> is set.</summary>
public class AppDbContext : DbContext
{
    private readonly ITenantProvider? _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    /// <summary>When empty, global query filters do not restrict by tenant (design-time / migrations).</summary>
    public Guid CurrentTenantId => _tenantProvider?.TenantId ?? Guid.Empty;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceSegment> ServiceSegments => Set<ServiceSegment>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ProfessionalWorkingHours> ProfessionalWorkingHours => Set<ProfessionalWorkingHours>();
    public DbSet<ProfessionalService> ProfessionalServices => Set<ProfessionalService>();
    public DbSet<ScheduleBlock> ScheduleBlocks => Set<ScheduleBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.Property(e => e.WhatsappNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
            entity.Property(e => e.AccentColor).HasMaxLength(20);
            entity.Property(e => e.BackgroundColor).HasMaxLength(20);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.LogoObjectKey).HasMaxLength(500);
            entity.Property(e => e.TimeZoneId).HasMaxLength(100);
            entity.Property(e => e.ClosingTime).HasMaxLength(5);
            entity.Property(e => e.HeaderImageUrl).HasMaxLength(500);
            entity.Property(e => e.HeaderImageObjectKey).HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.MonthlyFee).HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.JoinedAt).IsRequired();
            entity.Property(e => e.BillingStatus)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(TenantBillingStatus.Active);
            entity.Property(e => e.CourtesyUntil);
            entity.Property(e => e.AttendanceMode)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(AttendanceMode.Salon);
            entity.Property(e => e.SalonAddressLine).HasMaxLength(300);
            entity.Property(e => e.SalonAddressReference).HasMaxLength(200);
            entity.Property(e => e.ServiceAreaDescription).HasMaxLength(300);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.BirthDate).HasColumnType("date");
            entity.Property(e => e.AddressLine).HasMaxLength(300);
            entity.Property(e => e.Neighborhood).HasMaxLength(120);
            entity.Property(e => e.AddressReference).HasMaxLength(200);
            entity.Property(e => e.TenantId).IsRequired();
            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Professional>(entity =>
        {
            entity.ToTable("Professionals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.IsPrimary).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.TenantId)
                .IsUnique()
                .HasFilter("\"IsPrimary\" = true");
            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceSegment>(entity =>
        {
            entity.ToTable("ServiceSegments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.TenantId).IsRequired();
            entity.HasIndex(e => new { e.TenantId, e.Slug }).IsUnique();
            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Services");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.BufferAfterMinutes).IsRequired();
            entity.Property(e => e.Price).HasColumnType("numeric(10,2)");
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ServiceSegmentId).IsRequired();
            entity.HasIndex(e => e.ServiceSegmentId);
            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ServiceSegment)
                .WithMany()
                .HasForeignKey(e => e.ServiceSegmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProfessionalWorkingHours>(entity =>
        {
            entity.ToTable("ProfessionalWorkingHours");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ProfessionalId).IsRequired();
            entity.Property(e => e.DayOfWeek).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.HasOne(e => e.Professional)
                .WithMany()
                .HasForeignKey(e => e.ProfessionalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfessionalService>(entity =>
        {
            entity.ToTable("ProfessionalServices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ProfessionalId).IsRequired();
            entity.Property(e => e.ServiceId).IsRequired();
            entity.HasIndex(e => new { e.ProfessionalId, e.ServiceId }).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Professional)
                .WithMany()
                .HasForeignKey(e => e.ProfessionalId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Service)
                .WithMany()
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.Property(e => e.ProfessionalId).IsRequired();
            entity.Property(e => e.ServiceId).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.ServicePrice).HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.OriginalServicePrice).HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.FinalServicePrice).HasColumnType("numeric(10,2)").HasDefaultValue(0m);
            entity.Property(e => e.AttendanceLocationType)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(AttendanceLocationType.Salon);
            entity.Property(e => e.ClientAddress).HasMaxLength(300);
            entity.Property(e => e.ClientNeighborhood).HasMaxLength(120);
            entity.Property(e => e.ClientAddressReference).HasMaxLength(200);
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Professional)
                .WithMany()
                .HasForeignKey(e => e.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Service)
                .WithMany()
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ScheduleBlock>(entity =>
        {
            entity.ToTable("ScheduleBlocks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ProfessionalId).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.IsAllDay).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.Professional)
                .WithMany()
                .HasForeignKey(e => e.ProfessionalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Client>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Professional>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Service>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
        modelBuilder.Entity<ServiceSegment>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
        modelBuilder.Entity<ProfessionalWorkingHours>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
        modelBuilder.Entity<ProfessionalService>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
        modelBuilder.Entity<ScheduleBlock>()
            .HasQueryFilter(e => CurrentTenantId == Guid.Empty || e.TenantId == CurrentTenantId);
    }
}
