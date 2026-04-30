# RAC Booking

RAC Booking is a multi-tenant SaaS scheduling platform designed for service-based businesses such as salons, beauty professionals, and appointment-driven teams.

This project showcases a production-oriented architecture built with modern .NET technologies, focusing on scalability, tenant isolation, and real-world scheduling complexity.

> ⚠️ This repository is a **sanitized portfolio version** of a real-world system. Sensitive data, credentials, and production-specific configurations have been removed.

---

## 🚀 Product Overview

RAC Booking allows customers to book appointments online while giving business owners full control over scheduling, professionals, and services.

The system was designed with real-world constraints in mind:

- Preventing double bookings  
- Handling dynamic availability  
- Supporting multiple tenants in a single system  
- Maintaining a clean and scalable architecture  

---

## 🖼️ Demo

A visual walkthrough is available here:  
👉 [View Demo](./docs/DEMO.md)

---

## ✨ Key Features

- Online self-booking for clients  
- Professional-based scheduling  
- Availability engine with conflict prevention  
- Buffer handling between appointments  
- Dynamic tenant branding (logo, colors, banner)  
- Role-based access (Admin / Professional)  
- Multi-tenant ready architecture  

---

## 📸 Screenshots

### Public Booking Flow

> Add screenshots in this order for best impact

- Home / Booking entry  
- Service selection  
- Professional selection  
- Date selection  
- Time selection  
- Confirmation  

### Admin Dashboard

- Calendar view  
- Appointment management  

---

## 🧠 Architecture

The project follows **Clean Architecture** principles:

- **Domain** – core business rules  
- **Application** – use cases (CQRS with MediatR)  
- **Infrastructure** – EF Core, PostgreSQL, external services  
- **API** – REST endpoints  

### Patterns & Practices

- CQRS with MediatR  
- Repository pattern  
- Dependency Injection  
- Strong separation of concerns  
- Production-oriented design  

📄 More details: [docs/architecture.md](docs/architecture.md)

---

## 🏢 Multi-Tenancy

The system supports multi-tenant environments using:

- TenantId across all relevant entities  
- Request-based tenant resolution (`X-Tenant-Id`)  
- Data isolation via EF Core global query filters  
- Public tenant resolution via slug  

Designed to scale from single-tenant to SaaS environments.

📄 More details: [docs/multi-tenancy.md](docs/multi-tenancy.md)

---

## ⏱️ Availability Engine

A core part of the system is availability calculation.

It considers:

- Professional working hours  
- Existing appointments  
- Schedule blocks  
- Service duration + buffer time  

Ensures:

- No overlapping bookings  
- Accurate time slot generation  
- Real-time availability  

📄 More details: [docs/availability-engine.md](docs/availability-engine.md)

---

## ⚙️ Tech Stack

### Backend
- .NET 8 (ASP.NET Core)  
- Entity Framework Core  
- PostgreSQL  
- MediatR (CQRS)  

### Frontend
- Angular  
- Bootstrap  
- FullCalendar  

### Infrastructure
- Docker / Docker Compose  
- DigitalOcean (Droplet + Spaces)  
- Nginx  

### Integrations
- Resend (email delivery)  

---

## 📁 Repository Structure

docs/                 → Documentation and screenshots  
src/                  → Backend source code (Clean Architecture)  
frontend/             → Angular application  
docker-compose.yml    → Local environment setup  
.env.example          → Environment variables template  

---

## 🚀 Running Locally

docker-compose up --build

Then access:

- API: http://localhost:5000  
- Frontend: http://localhost:4200  

---

## 🧪 What This Project Demonstrates

This project was designed to showcase:

- Real-world SaaS architecture  
- Multi-tenant system design  
- Backend scalability patterns  
- Scheduling conflict resolution  
- Clean and maintainable code structure  

---

## 📌 Notes

This is a portfolio-focused version of the system.

- No production secrets included  
- Some integrations are simplified  
- Focus is on architecture and backend design  

---

## 📄 License

MIT
