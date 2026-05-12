import { Injectable } from '@angular/core';
import { Flight } from '../models/flight.model';

@Injectable({ providedIn: 'root' })
export class SearchStateService {
  selectedFlight: Flight | null = null;
  bookingReference: string | null = null;
}
