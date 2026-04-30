import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PublicSalonResponse {
  tenantId: string;
  name: string;
  slug: string;
  branding: {
    primaryColor?: string | null;
    secondaryColor?: string | null;
    accentColor?: string | null;
    backgroundColor?: string | null;
    logoUrl?: string | null;
    headerImageUrl?: string | null;
    closingTime?: string | null;
  };
  timeZoneId?: string | null;
  attendanceMode: 'Salon' | 'ClientAddress' | 'Both';
  salonAddressLine?: string | null;
  salonAddressReference?: string | null;
  serviceAreaDescription?: string | null;
  segments: ServiceSegmentResponse[];
  services: ServiceResponse[];
  professionals: ProfessionalResponse[];
  professionalServices: ProfessionalServiceLink[];
}

export interface ServiceSegmentResponse {
  id: string;
  name: string;
  slug: string;
  displayOrder: number;
}

export interface ServiceResponse {
  id: string;
  serviceSegmentId: string;
  name: string;
  description: string;
  durationMinutes: number;
  bufferAfterMinutes: number;
  price: number;
}

export interface ProfessionalResponse {
  id: string;
  name: string;
  isPrimary: boolean;
}

export interface ProfessionalServiceLink {
  professionalId: string;
  serviceId: string;
}

export interface AvailableSlotResponse {
  startTimeUtc: string;
  endTimeUtc: string;
  displayTime: string;
  displayDate: string;
  priority: string;
}

export interface CreateAppointmentRequest {
  name: string;
  phone: string;
  email?: string | null;
  professionalId: string;
  serviceId: string;
  startTime: string;
  notes?: string | null;
  attendanceLocationType: 'Salon';
}

@Injectable({ providedIn: 'root' })
export class PublicBookingApiService {
  private readonly baseUrl = 'http://localhost:5280/api/public';
  private readonly demoSlug = 'demo-salon';

  constructor(private readonly http: HttpClient) {}

  getSalon(): Observable<PublicSalonResponse> {
    return this.http.get<PublicSalonResponse>(`${this.baseUrl}/salon/${this.demoSlug}`);
  }

  getAvailability(professionalId: string, serviceId: string, date: string): Observable<AvailableSlotResponse[]> {
    return this.http.get<AvailableSlotResponse[]>(`${this.baseUrl}/availability`, {
      params: {
        slug: this.demoSlug,
        professionalId,
        serviceId,
        date
      }
    });
  }

  createAppointment(payload: CreateAppointmentRequest): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/appointments`, payload, {
      params: { slug: this.demoSlug }
    });
  }
}
