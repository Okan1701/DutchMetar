import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AirportSummary } from '../models/airport-summary';
import { LoadingStatus } from '../types/status';
import { HttpClient } from '@angular/common/http';
import { AirportDetails } from '../models/airport-details';

@Injectable({
    providedIn: 'root',
})
export class AirportService {
    private readonly airportsSubject = new BehaviorSubject<AirportSummary[]>([]);
    private readonly loadingStatusSubject = new BehaviorSubject<LoadingStatus>('loading')
    private readonly airportsEndpoint = "/api/airport"

    constructor(private readonly httpClient: HttpClient) {
    }

    public get airports$(): Observable<AirportSummary[]> {
        return this.airportsSubject.asObservable();
    }

    public get loadingStatus$(): Observable<LoadingStatus> {
        return this.loadingStatusSubject.asObservable();
    }
    
    public initializeAirports(): void {
        this.httpClient.get<AirportSummary[]>(this.airportsEndpoint).subscribe({
            next: (airports) => {
                this.airportsSubject.next(airports);
                this.loadingStatusSubject.next('success');
            },
            error: (err) => {
                console.error('Failed to retrieve airports', err)
                this.loadingStatusSubject.next('error');
            },
        });
    }
    
    public getAirportDetails(icao: string): Observable<AirportDetails> {
        return this.httpClient.get<AirportDetails>(`${this.airportsEndpoint}/${icao}`);
    }

    public getAirportHistory(icao: string, targetDate: Date): Observable<AirportDetails> {
        return this.httpClient.get<AirportDetails>(`${this.airportsEndpoint}/${icao}/history/?targetDate=${targetDate.toISOString()}`);
    }
}