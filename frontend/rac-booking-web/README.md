# RAC Booking Web (Portfolio Demo)

Minimal Angular public booking flow for the portfolio version of RAC Booking.

## Prerequisites

* Node.js and npm
* Backend API running at `http://localhost:5280`

## Run locally

npm install
npm start

Open `http://localhost:4200`.

## Backend configuration

This frontend calls the public API at:

* http://localhost:5280/api/public/salon/demo-salon
* http://localhost:5280/api/public/availability
* http://localhost:5280/api/public/appointments

## Booking flow

1. Load demo salon (`demo-salon`)
2. Select service (grouped by segment)
3. Select professional compatible with selected service
4. Select date and load slots
5. Select slot and submit customer form
6. Show success screen after appointment creation
