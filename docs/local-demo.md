# Local demo (PostgreSQL)

Fictional English seed data runs **only when `ASPNETCORE_ENVIRONMENT=Development`**. It never runs in Production.

## Prerequisites

- PostgreSQL reachable from your machine  
- Connection string in `appsettings.Development.json` (or environment `ConnectionStrings__DefaultConnection`) pointing at an empty or existing dev database  

## Apply migrations

From the repository root (`PORTFOLIO/rac-booking`):

```bash
dotnet ef database update --project src/Infrastructure/RacBooking.Infrastructure.csproj --startup-project src/Api/RacBooking.Api.csproj
```

## Run the API

```bash
dotnet run --project src/Api/RacBooking.Api.csproj
```

On startup in Development, the **demo tenant** is ensured (slug below).

## Demo tenant slug

| Field | Value |
|--------|--------|
| **Slug** | `demo-salon` |
| **Name** | RAC Booking Demo Salon |
| **Time zone** | `America/Sao_Paulo` |

## Quick checks

Replace `{base}` with your API base URL (for example `https://localhost:7xxx`).

**Salon bootstrap (IDs for services and professionals):**

```http
GET {base}/api/public/salon/demo-salon
```

**Availability** — copy `professionalId` and `serviceId` from the salon response, pick a **future weekday** (`yyyy-MM-dd`):

```http
GET {base}/api/public/availability?slug=demo-salon&professionalId={professionalId}&serviceId={serviceId}&date=2026-07-20
```

**Book** — use an international-style phone (8–15 digits, e.g. `+15551234567`) in the JSON body:

```http
POST {base}/api/public/appointments?slug=demo-salon
Content-Type: application/json
```

Example body (adjust IDs and `startTime` to a slot from availability):

```json
{
  "name": "Test Customer",
  "phone": "+15551234567",
  "email": "customer@example.invalid",
  "professionalId": "f6a7b8c9-d0e1-4234-f567-89abcdef0123",
  "serviceId": "c3d4e5f6-a7b8-4901-c234-56789abcdef0",
  "startTime": "2026-07-21T14:00:00Z",
  "notes": "Demo booking"
}
```

**Admin calendar** — requires header `X-Tenant-Id` with the demo tenant id and uses tenant-local calendar dates:

```http
GET {base}/api/appointments/calendar?from=2026-07-01&to=2026-07-31
X-Tenant-Id: a1b2c3d4-e5f6-4789-a012-3456789abcde
```

Optional: `&professionalId={guid}` to filter one professional.

## Stable demo GUIDs (optional)

If you need fixed identifiers without calling the salon endpoint first:

| Entity | Id |
|--------|-----|
| Tenant | `a1b2c3d4-e5f6-4789-a012-3456789abcde` |
| Emma Stylist | `f6a7b8c9-d0e1-4234-f567-89abcdef0123` |
| John Barber | `a7b8c9d0-e1f2-4345-a678-9abcdef01234` |
| Signature Haircut | `c3d4e5f6-a7b8-4901-c234-56789abcdef0` |
| Full Hair Color | `d4e5f6a7-b8c9-4012-d345-6789abcdef01` |
| Beard Trim & Line-Up | `e5f6a7b8-c9d0-4123-e456-789abcdef012` |

Schedule blocks (demo) are stored on **15–16 July 2026** (America/Sao_Paulo) and only affect availability on those dates.
