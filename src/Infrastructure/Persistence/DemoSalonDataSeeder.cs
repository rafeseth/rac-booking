using RacBooking.Domain.Entities;
using RacBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Persistence;

/// <summary>Development-only fictional data for the public booking demo tenant. All strings are English.</summary>
public static class DemoSalonDataSeeder
{
    public const string DemoSlug = "demo-salon";

    // Stable IDs so docs and local tests can reference them without another round-trip.
    private static readonly Guid TenantId = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3456789abcde");
    private static readonly Guid SegmentId = Guid.Parse("b2c3d4e5-f6a7-4890-b123-456789abcdef");
    private static readonly Guid ServiceHaircutId = Guid.Parse("c3d4e5f6-a7b8-4901-c234-56789abcdef0");
    private static readonly Guid ServiceColorId = Guid.Parse("d4e5f6a7-b8c9-4012-d345-6789abcdef01");
    private static readonly Guid ServiceBeardId = Guid.Parse("e5f6a7b8-c9d0-4123-e456-789abcdef012");
    private static readonly Guid ProfessionalEmmaId = Guid.Parse("f6a7b8c9-d0e1-4234-f567-89abcdef0123");
    private static readonly Guid ProfessionalJohnId = Guid.Parse("a7b8c9d0-e1f2-4345-a678-9abcdef01234");

    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Tenants.AsNoTracking().AnyAsync(t => t.Slug == DemoSlug, cancellationToken))
        {
            await SeedCoreAsync(db, cancellationToken);
            return;
        }

        await EnsureOptionalBlocksAsync(db, cancellationToken);
    }

    private static async Task SeedCoreAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var tenant = new Tenant
        {
            Id = TenantId,
            Name = "RAC Booking Demo Salon",
            Slug = DemoSlug,
            WhatsappNumber = "00000000000",
            CreatedAt = utcNow,
            JoinedAt = utcNow,
            IsActive = true,
            MonthlyFee = 0m,
            BillingStatus = TenantBillingStatus.Active,
            AttendanceMode = AttendanceMode.Salon,
            TimeZoneId = "America/Sao_Paulo",
            PrimaryColor = "#2C5282",
            SecondaryColor = "#DD6B20",
            AccentColor = "#319795",
            BackgroundColor = "#F7FAFC",
            LogoUrl = null,
            HeaderImageUrl = null,
            ClosingTime = "18:00",
            SalonAddressLine = "123 Example Avenue, Suite 200",
            SalonAddressReference = "Next to the central plaza clock tower.",
            ServiceAreaDescription = "Downtown core — illustrative service area for demo only."
        };

        var segment = new ServiceSegment
        {
            Id = SegmentId,
            TenantId = TenantId,
            Name = "Hair & Grooming",
            Slug = "hair-grooming",
            IsActive = true,
            DisplayOrder = 0,
            CreatedAt = utcNow
        };

        var services = new[]
        {
            new Service
            {
                Id = ServiceHaircutId,
                TenantId = TenantId,
                ServiceSegmentId = SegmentId,
                Name = "Signature Haircut",
                Description = "Wash, cut, and style tailored to your hair type.",
                DurationMinutes = 45,
                BufferAfterMinutes = 15,
                Price = 48.00m,
                IsActive = true
            },
            new Service
            {
                Id = ServiceColorId,
                TenantId = TenantId,
                ServiceSegmentId = SegmentId,
                Name = "Full Hair Color",
                Description = "Full head color application with conditioning finish.",
                DurationMinutes = 120,
                BufferAfterMinutes = 30,
                Price = 135.00m,
                IsActive = true
            },
            new Service
            {
                Id = ServiceBeardId,
                TenantId = TenantId,
                ServiceSegmentId = SegmentId,
                Name = "Beard Trim & Line-Up",
                Description = "Trim, shape, and detail finish for beards and edges.",
                DurationMinutes = 30,
                BufferAfterMinutes = 15,
                Price = 28.00m,
                IsActive = true
            }
        };

        var professionals = new[]
        {
            new Professional
            {
                Id = ProfessionalEmmaId,
                TenantId = TenantId,
                Name = "Emma Stylist",
                Phone = "+15550001001",
                Email = "emma.stylist@example.invalid",
                IsPrimary = true,
                IsActive = true
            },
            new Professional
            {
                Id = ProfessionalJohnId,
                TenantId = TenantId,
                Name = "John Barber",
                Phone = "+15550001002",
                Email = "john.barber@example.invalid",
                IsPrimary = false,
                IsActive = true
            }
        };

        var links = new[]
        {
            new ProfessionalService
            {
                Id = Guid.Parse("11223344-5566-4778-899a-001122334455"),
                TenantId = TenantId,
                ProfessionalId = ProfessionalEmmaId,
                ServiceId = ServiceHaircutId
            },
            new ProfessionalService
            {
                Id = Guid.Parse("22334455-6677-4889-9abb-112233445566"),
                TenantId = TenantId,
                ProfessionalId = ProfessionalEmmaId,
                ServiceId = ServiceColorId
            },
            new ProfessionalService
            {
                Id = Guid.Parse("33445566-7788-4990-abcc-223344556677"),
                TenantId = TenantId,
                ProfessionalId = ProfessionalJohnId,
                ServiceId = ServiceBeardId
            },
            new ProfessionalService
            {
                Id = Guid.Parse("44556677-8899-4001-bcdd-334455667788"),
                TenantId = TenantId,
                ProfessionalId = ProfessionalJohnId,
                ServiceId = ServiceHaircutId
            }
        };

        var open = new TimeSpan(9, 0, 0);
        var close = new TimeSpan(18, 0, 0);
        var days = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
        };
        // Stable hexadecimal GUIDs (Monday..Friday × Emma, John).
        var emmaRowIds = new[]
        {
            Guid.Parse("e1a00001-0000-4000-8000-000000000001"),
            Guid.Parse("e1a00002-0000-4000-8000-000000000002"),
            Guid.Parse("e1a00003-0000-4000-8000-000000000003"),
            Guid.Parse("e1a00004-0000-4000-8000-000000000004"),
            Guid.Parse("e1a00005-0000-4000-8000-000000000005")
        };
        var johnRowIds = new[]
        {
            Guid.Parse("e2b00001-0000-4000-8000-000000000001"),
            Guid.Parse("e2b00002-0000-4000-8000-000000000002"),
            Guid.Parse("e2b00003-0000-4000-8000-000000000003"),
            Guid.Parse("e2b00004-0000-4000-8000-000000000004"),
            Guid.Parse("e2b00005-0000-4000-8000-000000000005")
        };

        var working = new List<ProfessionalWorkingHours>();
        for (var i = 0; i < days.Length; i++)
        {
            working.Add(new ProfessionalWorkingHours
            {
                Id = emmaRowIds[i],
                TenantId = TenantId,
                ProfessionalId = ProfessionalEmmaId,
                DayOfWeek = days[i],
                StartTime = open,
                EndTime = close
            });
            working.Add(new ProfessionalWorkingHours
            {
                Id = johnRowIds[i],
                TenantId = TenantId,
                ProfessionalId = ProfessionalJohnId,
                DayOfWeek = days[i],
                StartTime = open,
                EndTime = close
            });
        }

        db.Tenants.Add(tenant);
        db.ServiceSegments.Add(segment);
        db.Services.AddRange(services);
        db.Professionals.AddRange(professionals);
        db.ProfessionalServices.AddRange(links);
        db.ProfessionalWorkingHours.AddRange(working);

        AddDemoScheduleBlocks(db, utcNow);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>If the tenant existed but blocks were missing (e.g. partial seed), add blocks once.</summary>
    private static async Task EnsureOptionalBlocksAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (await db.ScheduleBlocks.AsNoTracking().AnyAsync(
                b => b.TenantId == TenantId && b.Type == "Break",
                cancellationToken))
            return;

        AddDemoScheduleBlocks(db, DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddDemoScheduleBlocks(AppDbContext db, DateTime referenceUtc)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        // Fixed illustrative weekdays for recurring-style demo blocks (stored in UTC).
        var lunchLocal = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Unspecified);
        var lunchEndLocal = new DateTime(2026, 7, 15, 13, 0, 0, DateTimeKind.Unspecified);
        var meetingLocal = new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Unspecified);
        var meetingEndLocal = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Unspecified);

        var lunchStartUtc = TimeZoneInfo.ConvertTimeToUtc(lunchLocal, tz);
        var lunchEndUtc = TimeZoneInfo.ConvertTimeToUtc(lunchEndLocal, tz);
        var meetingStartUtc = TimeZoneInfo.ConvertTimeToUtc(meetingLocal, tz);
        var meetingEndUtc = TimeZoneInfo.ConvertTimeToUtc(meetingEndLocal, tz);

        db.ScheduleBlocks.AddRange(
            new ScheduleBlock
            {
                Id = Guid.Parse("55112233-4455-4667-8899-aabbccddeeff"),
                TenantId = TenantId,
                ProfessionalId = ProfessionalEmmaId,
                StartTime = lunchStartUtc,
                EndTime = lunchEndUtc,
                IsAllDay = false,
                Type = "Break",
                Reason = "Lunch break (demo data)",
                CreatedAt = referenceUtc
            },
            new ScheduleBlock
            {
                Id = Guid.Parse("66223344-5566-4778-9900-bbccddeeff00"),
                TenantId = TenantId,
                ProfessionalId = ProfessionalJohnId,
                StartTime = meetingStartUtc,
                EndTime = meetingEndUtc,
                IsAllDay = false,
                Type = "Internal",
                Reason = "Team sync hold (demo data)",
                CreatedAt = referenceUtc
            });
    }
}
