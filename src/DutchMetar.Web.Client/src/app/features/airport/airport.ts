import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AirportDetails } from '../../shared/models/airport-details';
import { LoadingStatus } from '../../shared/types/status';
import { AirportService } from '../../shared/services/airport-service';
import { StatusDisplay } from '../../shared/components/status-display/status-display';
import { Subject, takeUntil } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { Stack } from '../../shared/components/stack/stack';
import { MatChipsModule } from '@angular/material/chips';
import { AirportLatestWeather } from './components/airport-latest-weather/airport-latest-weather';
import { DatePipe } from '@angular/common';
import { AirportHistoryData } from './components/airport-history-data/airport-history-data';

@Component({
    selector: 'app-airport',
    imports: [
        StatusDisplay,
        Stack,
        MatChipsModule,
        AirportLatestWeather,
        DatePipe,
        AirportHistoryData,
    ],
    templateUrl: './airport.html',
    styleUrl: './airport.scss',
})
export class Airport implements OnInit, OnDestroy {
    protected airportIcao: string | null = null;
    protected airportDetails = signal<AirportDetails>({
        icao: '',
        lastUpdated: new Date(),
    });
    protected loadingStatus = signal<LoadingStatus>('loading');

    private unsubscribe$: Subject<void> = new Subject<void>();

    constructor(
        private readonly route: ActivatedRoute,
        private readonly airportService: AirportService,
    ) {}

    public ngOnInit(): void {
        this.airportIcao = this.route.snapshot.paramMap.get('icao');

        if (this.airportIcao != null) {
            this.loadingStatus.set('loading');
            this.airportService
                .getAirportDetails(this.airportIcao)
                .pipe(takeUntil(this.unsubscribe$))
                .subscribe({
                    next: (data) => this.onAirportDetailsRetrieved(data),
                    error: (error) => this.onRetrievalError(error),
                });
        }
    }

    public ngOnDestroy(): void {
        this.unsubscribe$.next();
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
