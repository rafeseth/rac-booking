# RAC Booking Demo

This document presents the main flows of RAC Booking through screenshots and technical notes.

## 1. Public Booking Flow

Customers can book appointments online by selecting:

1. Service
2. Professional
3. Date
4. Available time slot
5. Customer information

![Public Booking](./screenshots/public-booking.png)

---

## 2. Availability Engine

The availability engine calculates valid time slots based on:

- Professional working hours
- Existing appointments
- Schedule blocks
- Service duration
- Buffer time between appointments

![Availability](./screenshots/availability.png)

---

## 3. Admin Calendar

Business owners can manage appointments from an internal calendar view.

![Admin Calendar](./screenshots/admin-calendar.png)

---

## 4. Services and Professionals

Administrators can manage services, professionals, and which services each professional can perform.

![Services](./screenshots/services.png)

---

## 5. Architecture Highlights

- .NET 8 backend
- Angular frontend
- PostgreSQL database
- Clean Architecture
- CQRS with MediatR
- Dockerized environment
- Designed to support multi-tenant scenarios

## Booking Flow

### 1. Landing

![Banner](./screenshots/public-booking-banner.png)

### 2. Select Service

![Service](./screenshots/select-service.png)

### 3. Select Professional

![Professional](./screenshots/select-professional.png)

### 4. Select Date

![Date](./screenshots/select-date.png)

### 5. Select Time

![Availability](./screenshots/availability.png)

### 6. Confirmation

![Confirmation](./screenshots/Client-booking-confirmation.png)

---

## Admin Dashboard

![Admin](./screenshots/admin-calendar.png)