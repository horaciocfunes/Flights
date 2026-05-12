import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Flight, FlightSearchParams } from '../models/flight.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FlightService {
  private readonly searchUrl = `${environment.apiUrl}/flights/search`;

  constructor(private http: HttpClient) {}

  search(params: FlightSearchParams): Observable<Flight[]> {
    const httpParams = new HttpParams()
      .set('originCode',      params.originCode)
      .set('destinationCode', params.destinationCode)
      .set('departureDate',   params.departureDate)
      .set('passengerCount',  params.passengerCount.toString())
      .set('cabinClass',      params.cabinClass);

    return this.http.get<Flight[]>(this.searchUrl, { params: httpParams });
  }
}
