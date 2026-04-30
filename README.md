# RAC Booking

A scalable scheduling platform designed for service-based businesses, with support for multi-tenant architectures.

## ✨ Overview

RAC Booking allows customers to book appointments online while giving business owners full control over scheduling, professionals, and services.

The system focuses on availability accuracy, scalability, and real-world scheduling constraints, including support for multi-tenant scenarios.

---

## 👀 Demo

A visual walkthrough is available here: [Demo](./docs/DEMO.md)

## 🧠 Key Features

- Online self-booking for clients
- Professional-based scheduling
- Availability engine with conflict prevention
- Buffer handling between appointments
- Dynamic tenant branding (logo, colors, banner)
- Role-based access (Admin / Professional)
- Designed to support both single-tenant and multi-tenant deployments

---

## 🏗️ Architecture

The project follows **Clean Architecture** principles:

- **Domain** – core business rules
- **Application** – use cases (CQRS with MediatR)
- **Infrastructure** – EF Core, PostgreSQL, external services
- **API** – REST endpoints

### Patterns & Practices

- CQRS with MediatR
- Repository pattern
- Dependency Injection
- Separation of concerns
- Scalable design for real-world usage

---

## ⚙️ Tech Stack

**Backend**
- .NET 8 (ASP.NET Core)
- Entity Framework Core
- PostgreSQL

**Frontend**
- Angular
- Bootstrap

**Infrastructure**
- Docker / Docker Compose
- DigitalOcean (Droplet + Spaces)
- Nginx

---

## 🔥 Highlights

### Availability Engine
- Prevents overlapping appointments
- Considers:
  - Working hours
  - Existing bookings
  - Schedule blocks
  - Service duration + buffer time

### Flexible Architecture
- Designed to scale from single-tenant to multi-tenant environments
- Clean separation between layers
- Easy to extend and maintain

### Booking Flow
1. Select service
2. Select professional
3. Select date
4. Select available time slot
5. Confirm booking

---

## 📸 Screenshots

> Add screenshots here:
- Public booking page
- Admin dashboard
- Calendar view

---

## 🚀 Running Locally
bash
docker-compose up --build

Then access:
- API: http://localhost:5000
- Frontend: http://localhost:4200

📌 Notes
This project was built with real-world constraints in mind, focusing on reliability, maintainability, and scalability.
