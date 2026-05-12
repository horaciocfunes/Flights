import { Airport } from './airport.model';

export interface Flight {
  flightId: string;
  flightNumber: string;
  providerName: string;
  origin: Airport;
  destination: Airport;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  cabinClass: string;
  pricePerPassenger: number;
  totalPrice: number;
  passengerCount: number;
  isInternational: boolean;
  documentLabel: string;
}

export interface FlightSearchParams {
  originCode: string;
  destinationCode: string;
  departureDate: string;
  passengerCount: number;
  cabinClass: string;
}
