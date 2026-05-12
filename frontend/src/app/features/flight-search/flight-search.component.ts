import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AirportService } from '../../core/services/airport.service';
import { Airport } from '../../core/models/airport.model';

@Component({
  selector: 'app-flight-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './flight-search.component.html',
  styleUrl: './flight-search.component.scss'
})
export class FlightSearchComponent implements OnInit {
  searchForm: FormGroup;
  airports: Airport[] = [];
  readonly cabinClasses = ['Economy', 'Business', 'First'];
  readonly passengerOptions = Array.from({ length: 9 }, (_, i) => i + 1);
  readonly minDate = new Date().toISOString().split('T')[0];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private airportService: AirportService
  ) {
    this.searchForm = this.fb.group({
      originCode:      ['', Validators.required],
      destinationCode: ['', Validators.required],
      departureDate:   ['', Validators.required],
      passengerCount:  [1, [Validators.required, Validators.min(1), Validators.max(9)]],
      cabinClass:      ['Economy', Validators.required]
    });
  }

  get selectedOrigin(): string {
    return this.searchForm.get('originCode')?.value ?? '';
  }

  get availableDestinations(): Airport[] {
    return this.airports.filter(a => a.code !== this.selectedOrigin);
  }

  onOriginChange(): void {
    this.searchForm.get('destinationCode')?.reset('');
  }

  ngOnInit(): void {
    this.airportService.getAll().subscribe(airports => (this.airports = airports));
  }

  onSubmit(): void {
    if (this.searchForm.invalid) return;
    this.router.navigate(['/results'], { queryParams: this.searchForm.value });
  }
}
