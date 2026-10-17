import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatestWith, map, Observable, startWith, Subject } from 'rxjs';
import { AirportSummary } from '../models/airport/airport-summary';
import { LoadingStatus } from '../types/status';
import { HttpClient } from '@angular/common/http';
import { AirportDetails } from '../models/airport/airport-details';
import { AirportDayHistory } from '../models/airport/airport-day-history';
import { MeteoCondition } from '../types/meteo-condition';

@Injectable({
    providedIn: 'root',
})
export class AirportService {
    private readonly airportsSubject = new BehaviorSubject<AirportSummary[]>([]);
    private readonly currentAirportIcaoSubject = new BehaviorSubject<string>("");
    private readonly loadingStatusSubject = new BehaviorSubject<LoadingStatus>('loading');
    private readonly airportsEndpoint = '/api/airport';

    constructor(private readonly httpClient: HttpClient) {}

    public get airports$(): Observable<AirportSummary[]> {
        return this.airportsSubject.asObservable();
    }
    
    public get currentAirport$(): Observable<AirportSummary> {
        return this.airportsSubject.pipe(
            combineLatestWith(this.currentAirportIcaoSubject),
            map(([airports, icao]) => {
                let airport = airports.find(x => x.icao.toUpperCase() === icao.toUpperCase());
                return airport ?? {
                    icao: "",
                    name: "",
                    meteoCondition: MeteoCondition.None,
                    lastUpdated: new Date()
                } as AirportSummary
            }),
        );
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
                console.error('Failed to retrieve airports', err);
                this.loadingStatusSubject.next('error');
            },
        });
    }
    
    public setCurrentAirport(icao: string) {
        this.currentAirportIcaoSubject.next(icao);
    }

    public getAirportDetails(icao: string): Observable<AirportDetails> {
        return this.httpClient.get<AirportDetails>(`${this.airportsEndpoint}/${icao}`);
    }

    public getAirportHistory(icao: string, targetDate: Date): Observable<AirportDayHistory> {
        return this.httpClient.get<AirportDayHistory>(
            `${this.airportsEndpoint}/${icao}/history/?targetDate=${targetDate.toISOString().split('T')[0]}`,
        );
    }
}