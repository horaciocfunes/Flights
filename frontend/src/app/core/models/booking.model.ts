export interface PassengerInfo {
  fullName: string;
  email: string;
  documentNumber: string;
}

export interface BookingRequest {
  flightId: string;
  passengers: PassengerInfo[];
}

export interface BookingConfirmation {
  bookingReference: string;
  totalPrice: number;
  passengerCount: number;
}
