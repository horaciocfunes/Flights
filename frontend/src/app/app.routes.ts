import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/flight-search/flight-search.component')
        .then(m => m.FlightSearchComponent)
  },
  {
    path: 'results',
    loadComponent: () =>
      import('./features/flight-results/flight-results.component')
        .then(m => m.FlightResultsComponent)
  },
  {
    path: 'book',
    loadComponent: () =>
      import('./features/booking/booking.component')
        .then(m => m.BookingComponent)
  },
  {
    path: 'confirmation',
    loadComponent: () =>
      import('./features/booking-confirmation/booking-confirmation.component')
        .then(m => m.BookingConfirmationComponent)
  },
  { path: '**', redirectTo: '' }
];
