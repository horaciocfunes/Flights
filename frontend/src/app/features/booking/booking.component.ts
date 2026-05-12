import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BookingService } from '../../core/services/booking.service';
import { SearchStateService } from '../../core/services/search-state.service';
import { Flight } from '../../core/models/flight.model';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, DecimalPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.scss'
})
export class BookingComponent implements OnInit {
  flight: Flight | null = null;
  bookingForm: FormGroup;
  submitting = false;
  serverErrors: string[] = [];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private bookingService: BookingService,
    private state: SearchStateService
  ) {
    this.bookingForm = this.fb.group({ passengers: this.fb.array([]) });
  }

  ngOnInit(): void {
    this.flight = this.state.selectedFlight;
    if (!this.flight) {
      this.router.navigate(['/']);
      return;
    }
    for (let i = 0; i < this.flight.passengerCount; i++) {
      this.addPassengerGroup();
    }
  }

  get passengers(): FormArray {
    return this.bookingForm.get('passengers') as FormArray;
  }

  isDuplicateDocument(index: number): boolean {
    const current = this.passengers.at(index).get('documentNumber')?.value?.trim();
    if (!current) return false;
    return this.passengers.controls.some((ctrl, i) =>
      i !== index && ctrl.get('documentNumber')?.value?.trim().toLowerCase() === current.toLowerCase()
    );
  }

  private addPassengerGroup(): void {
    this.passengers.push(this.fb.group({
      fullName:       ['', [Validators.required, Validators.maxLength(100)]],
      email:          ['', [Validators.required, Validators.email]],
      documentNumber: ['', Validators.required]
    }));
  }

  onSubmit(): void {
    if (this.bookingForm.invalid || !this.flight) return;

    this.submitting   = true;
    this.serverErrors = [];

    this.bookingService.book({
      flightId:   this.flight.flightId,
      passengers: this.passengers.value
    }).subscribe({
      next: confirmation => {
        this.state.bookingReference = confirmation.bookingReference;
        this.router.navigate(['/confirmation']);
      },
      error: err => {
        const details: { field: string; message: string }[] = err.error?.details;
        this.serverErrors = details?.length
          ? details.map(d => d.message)
          : [err.error?.message ?? 'Booking failed. Please try again.'];
        this.submitting = false;
      }
    });
  }

  formatTime(dateStr: string): string {
    return new Date(dateStr).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' });
  }

  formatDuration(minutes: number): string {
    return `${Math.floor(minutes / 60)}h ${minutes % 60}m`;
  }
}
