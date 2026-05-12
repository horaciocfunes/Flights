import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FlightService } from '../../core/services/flight.service';
import { SearchStateService } from '../../core/services/search-state.service';
import { Flight } from '../../core/models/flight.model';

type SortKey = 'price' | 'duration' | 'departure';

@Component({
  selector: 'app-flight-results',
  standalone: true,
  imports: [CommonModule, DecimalPipe, RouterLink],
  templateUrl: './flight-results.component.html',
  styleUrl: './flight-results.component.scss'
})
export class FlightResultsComponent implements OnInit {
  flights: Flight[] = [];
  sortedFlights: Flight[] = [];
  loading = true;
  error = false;
  activeSort: SortKey = 'departure';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private flightService: FlightService,
    private state: SearchStateService
  ) {}

  ngOnInit(): void {
    const p = this.route.snapshot.queryParams;
    this.flightService.search({
      originCode:      p['originCode'],
      destinationCode: p['destinationCode'],
      departureDate:   p['departureDate'],
      passengerCount:  +p['passengerCount'],
      cabinClass:      p['cabinClass']
    }).subscribe({
      next: flights => {
        this.flights = flights;
        this.applySort('departure');
        this.loading = false;
      },
      error: () => {
        this.error = true;
        this.loading = false;
      }
    });
  }

  applySort(key: SortKey): void {
    this.activeSort = key;
    this.sortedFlights = [...this.flights].sort((a, b) => {
      if (key === 'price')     return a.totalPrice - b.totalPrice;
      if (key === 'duration')  return a.durationMinutes - b.durationMinutes;
      return new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime();
    });
  }

  selectFlight(flight: Flight): void {
    this.state.selectedFlight = flight;
    this.router.navigate(['/book']);
  }

  formatTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  formatDuration(minutes: number): string {
    return `${Math.floor(minutes / 60)}h ${minutes % 60}m`;
  }
}
