import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { SearchStateService } from '../../core/services/search-state.service';

@Component({
  selector: 'app-booking-confirmation',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './booking-confirmation.component.html',
  styleUrl: './booking-confirmation.component.scss'
})
export class BookingConfirmationComponent implements OnInit {
  bookingReference = '';

  constructor(private state: SearchStateService, private router: Router) {}

  ngOnInit(): void {
    if (!this.state.bookingReference) {
      this.router.navigate(['/']);
      return;
    }
    this.bookingReference = this.state.bookingReference;
    this.state.bookingReference = null;
    this.state.selectedFlight   = null;
  }
}
