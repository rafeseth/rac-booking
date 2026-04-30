import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AvailableSlotResponse,
  ProfessionalResponse,
  PublicBookingApiService,
  PublicSalonResponse,
  ServiceResponse
} from '../../services/public-booking-api.service';

type BookingStep = 1 | 2 | 3 | 4 | 5 | 6;

@Component({
  selector: 'app-public-booking',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './public-booking.component.html',
  styleUrl: './public-booking.component.scss'
})
export class PublicBookingComponent implements OnInit {
  salon: PublicSalonResponse | null = null;
  selectedService: ServiceResponse | null = null;
  selectedProfessional: ProfessionalResponse | null = null;
  selectedDate = '';
  selectedSlot: AvailableSlotResponse | null = null;
  slots: AvailableSlotResponse[] = [];

  loadingSalon = true;
  loadingSlots = false;
  submitting = false;
  success = false;
  errorMessage = '';

  readonly customerForm;

  constructor(
    private readonly api: PublicBookingApiService,
    private readonly fb: FormBuilder
  ) {
    this.customerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      phone: ['', [Validators.required, Validators.minLength(8)]],
      email: ['', [Validators.email]],
      notes: ['Demo booking']
    });
  }

  ngOnInit(): void {
    this.loadSalon();
  }

  get currentStep(): BookingStep {
    if (this.success) return 6;
    if (this.selectedSlot) return 5;
    if (this.selectedDate && this.selectedProfessional) return 4;
    if (this.selectedProfessional) return 3;
    if (this.selectedService) return 2;
    return 1;
  }

  get groupedServices(): Array<{ segmentName: string; services: ServiceResponse[] }> {
    if (!this.salon) return [];
    return this.salon.segments
      .map((segment) => ({
        segmentName: segment.name,
        services: this.salon!.services.filter((service) => service.serviceSegmentId === segment.id)
      }))
      .filter((group) => group.services.length > 0);
  }

  get filteredProfessionals(): ProfessionalResponse[] {
    if (!this.salon || !this.selectedService) return [];
    const allowed = new Set(
      this.salon.professionalServices
        .filter((link) => link.serviceId === this.selectedService!.id)
        .map((link) => link.professionalId)
    );
    return this.salon.professionals.filter((professional) => allowed.has(professional.id));
  }

  selectService(service: ServiceResponse): void {
    this.selectedService = service;
    this.selectedProfessional = null;
    this.selectedDate = '';
    this.selectedSlot = null;
    this.slots = [];
    this.errorMessage = '';
  }

  selectProfessional(professional: ProfessionalResponse): void {
    this.selectedProfessional = professional;
    this.selectedSlot = null;
    this.slots = [];
    this.errorMessage = '';
  }

  async onDateChanged(date: string): Promise<void> {
    this.selectedDate = date;
    this.selectedSlot = null;
    this.slots = [];
    this.errorMessage = '';

    if (!this.selectedService || !this.selectedProfessional || !date) {
      return;
    }

    this.loadingSlots = true;
    this.api.getAvailability(this.selectedProfessional.id, this.selectedService.id, date).subscribe({
      next: (slots) => {
        this.slots = slots;
        this.loadingSlots = false;
      },
      error: () => {
        this.loadingSlots = false;
        this.errorMessage = 'Could not load availability. Make sure the API is running.';
      }
    });
  }

  selectSlot(slot: AvailableSlotResponse): void {
    this.selectedSlot = slot;
    this.errorMessage = '';
  }

  submitBooking(): void {
    if (!this.selectedService || !this.selectedProfessional || !this.selectedSlot) {
      return;
    }
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = '';

    this.api
      .createAppointment({
        name: this.customerForm.value.name ?? '',
        phone: this.customerForm.value.phone ?? '',
        email: this.customerForm.value.email || null,
        professionalId: this.selectedProfessional.id,
        serviceId: this.selectedService.id,
        // CRITICAL: use startTimeUtc directly from API, no manual conversion.
        startTime: this.selectedSlot.startTimeUtc,
        notes: this.customerForm.value.notes || null,
        attendanceLocationType: 'Salon'
      })
      .subscribe({
        next: () => {
          this.submitting = false;
          this.success = true;
        },
        error: (err) => {
          this.submitting = false;
          this.errorMessage = err?.error?.message ?? 'Booking failed. Please try another slot.';
        }
      });
  }

  private loadSalon(): void {
    this.loadingSalon = true;
    this.api.getSalon().subscribe({
      next: (salon) => {
        this.salon = salon;
        this.loadingSalon = false;
      },
      error: () => {
        this.loadingSalon = false;
        this.errorMessage = 'Could not load salon. Confirm backend is running at http://localhost:5280.';
      }
    });
  }
}
