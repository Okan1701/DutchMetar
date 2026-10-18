import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AirportService } from '../../shared/services/airport-service';
import { BehaviorSubject, map } from 'rxjs';
import { Stack } from '../../shared/components/stack/stack';
import { MatChipsModule } from '@angular/material/chips';
import { AirportLatestWeather } from './components/airport-latest-weather/airport-latest-weather';
import { AsyncPipe, DatePipe } from '@angular/common';
import { AirportHistoryData } from './components/airport-history-data/airport-history-data';
import { Badge } from '../../shared/components/badge/badge';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MeteoCondition } from '../../shared/types/meteo-condition';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { AirportSummary } from '../../shared/models/airport/airport-summary';

@Component({
    selector: 'app-airport',
    imports: [
        Stack,
        MatChipsModule,
        AirportLatestWeather,
        DatePipe,
        AirportHistoryData,
        Badge,
        MatIconButton,
        MatIcon,
        MatTooltip,
        MatButton,
        AsyncPipe,
    ],
    templateUrl: './airport.html',
    styleUrl: './airport.scss',
})
export class Airport {
    protected airportDetails = signal<AirportSummary>({
        name: '',
        icao: '',
        lastUpdated: new Date(),
        meteoCondition: MeteoCondition.None,
    });
    protected readonly airportIcao = signal<string>('');
    protected readonly MeteoCondition = MeteoCondition;

    // The actual value of this is never used, it is merely to signal a refresh of the data
    // A normal subject would not be suitable because we need it to emit at least once
    private readonly refreshClicked$ = new BehaviorSubject<boolean>(false);

    protected get isRefreshDisabled$() {
        return this.airportService.loadingStatus$.pipe(map((x) => x !== "success"));
    }

    constructor(
        route: ActivatedRoute,
        private readonly router: Router,
        private readonly airportService: AirportService,
    ) {
        route.params.pipe(takeUntilDestroyed()).subscribe((params) => {
            let icao = params['icao'];
            if (icao) {
                this.airportIcao.set(icao);
                this.airportService.setCurrentAirport(icao);
            }
        });

        airportService.currentAirport$
            .pipe(takeUntilDestroyed())
            .subscribe((x) => this.airportDetails.set(x));
    }

    protected refresh(): void {
        this.refreshClicked$.next(true);
        this.airportService.initializeAirports();
    }

    protected history(): void {
        this.router.navigate(['history', this.airportIcao]);
    }
}
