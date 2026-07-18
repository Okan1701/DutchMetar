import { Component, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AirportDetails } from '../../shared/models/airport-details';
import { LoadingStatus } from '../../shared/types/status';
import { AirportService } from '../../shared/services/airport-service';
import { StatusDisplay } from '../../shared/components/status-display/status-display';
import { delay, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { Stack } from '../../shared/components/stack/stack';
import { MatChipsModule } from '@angular/material/chips';
import { AirportLatestWeather } from './components/airport-latest-weather/airport-latest-weather';
import { DatePipe } from '@angular/common';
import { AirportHistoryData } from './components/airport-history-data/airport-history-data';
import { Badge } from '../../shared/components/badge/badge';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-airport',
    imports: [
        StatusDisplay,
        Stack,
        MatChipsModule,
        AirportLatestWeather,
        DatePipe,
        AirportHistoryData,
        Badge,
    ],
    templateUrl: './airport.html',
    styleUrl: './airport.scss',
})
export class Airport {
    protected airportIcao: string | null = null;
    protected airportDetails = signal<AirportDetails>({
        icao: '',
        lastUpdated: new Date(),
    });
    protected loadingStatus = signal<LoadingStatus>('loading');

    constructor(
        route: ActivatedRoute,
        private readonly airportService: AirportService,
    ) {
        route.params
            .pipe(
                tap((params) => {
                    this.airportIcao = params['icao'];
                    if (this.airportIcao != null) {
                        this.loadingStatus.set('loading');
                    }
                }),
                switchMap((params) => {
                    const icao = params['icao'];
                    if (icao != null) {
                        return this.airportService.getAirportDetails(icao).pipe(delay(200));
                    }
                    return [];
                }),
                takeUntilDestroyed(),
            )
            .subscribe({
                next: (data) => this.onAirportDetailsRetrieved(data),
                error: (error) => this.onRetrievalError(error),
            });
    }

    private onAirportDetailsRetrieved(airportDetails: AirportDetails): void {
        this.loadingStatus.set('success');
        this.airportDetails.set(airportDetails);
    }

    private onRetrievalError(error: HttpErrorResponse): void {
        this.loadingStatus.set('error');
        console.error('Failed to retrieve details for ' + this.airportIcao, error);
    }
}
