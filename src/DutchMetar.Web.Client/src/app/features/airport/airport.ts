import { Component, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AirportDetails } from '../../shared/models/airport-details';
import { LoadingStatus } from '../../shared/types/status';
import { AirportService } from '../../shared/services/airport-service';
import { StatusDisplay } from '../../shared/components/status-display/status-display';
import { BehaviorSubject, combineLatestWith, delay, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { Stack } from '../../shared/components/stack/stack';
import { MatChipsModule } from '@angular/material/chips';
import { AirportLatestWeather } from './components/airport-latest-weather/airport-latest-weather';
import { DatePipe } from '@angular/common';
import { AirportHistoryData } from './components/airport-history-data/airport-history-data';
import { Badge } from '../../shared/components/badge/badge';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MeteoCondition } from '../../shared/types/meteo-condition';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';

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
        MatIconButton,
        MatIcon,
        MatTooltip,
    ],
    templateUrl: './airport.html',
    styleUrl: './airport.scss',
})
export class Airport {
    protected airportIcao: string | null = null;
    protected airportDetails = signal<AirportDetails>({
        icao: '',
        meteoCondition: MeteoCondition.None,
        lastUpdated: new Date(),
    });
    protected loadingStatus = signal<LoadingStatus>('loading');
    protected readonly MeteoCondition = MeteoCondition;

    // The actual value of this is never used, it is merely to signal a refresh of the data
    // A normal subject would not be suitable because we need it to emit at least once
    private readonly refreshClicked$ = new BehaviorSubject<boolean>(false);

    constructor(
        route: ActivatedRoute,
        private readonly airportService: AirportService,
    ) {
        route.params
            .pipe(
                combineLatestWith(this.refreshClicked$),
                tap(([params]) => {
                    this.airportIcao = params['icao'];
                    if (this.airportIcao != null) {
                        this.loadingStatus.set('loading');
                    }
                }),
                switchMap(([params]) => {
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

    protected refresh(): void {
        this.refreshClicked$.next(true);
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
