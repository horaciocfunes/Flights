import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BookingRequest, BookingConfirmation } from '../models/booking.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly bookingsUrl = `${environment.apiUrl}/bookings`;

  constructor(private http: HttpClient) {}

  book(request: BookingRequest): Observable<BookingConfirmation> {
    return this.http.post<BookingConfirmation>(this.bookingsUrl, request);
  }
}
