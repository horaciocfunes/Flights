import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { Airport } from '../models/airport.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AirportService {
  private readonly airports$ = this.http
    .get<Airport[]>(`${environment.apiUrl}/airports`)
    .pipe(shareReplay(1));

  constructor(private http: HttpClient) {}

  getAll(): Observable<Airport[]> {
    return this.airports$;
  }
}
