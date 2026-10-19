import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AirportService } from '../../shared/services/airport-service';
import {
    BehaviorSubject,
    catchError,
    combineLatest,
    distinctUntilChanged,
    EMPTY,
    filter,
    map,
    Observable,
    of,
    startWith,
    switchMap,
    tap,
    timer,
} from 'rxjs';
import { Stack } from '../../shared/components/stack/stack';
import { MatChipsModule } from '@angular/material/chips';
import { AirportLatestWeather } from './components/airport-latest-weather/airport-latest-weather';
import { AsyncPipe } from '@angular/common';
import { AirportHistoryData } from './components/airport-history-data/airport-history-data';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { MeteoCondition } from '../../shared/types/meteo-condition';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { AirportSummary } from '../../shared/models/airport/airport-summary';
import { AirportDayHistory } from '../../shared/models/airport/airport-day-history';
import { LoadingStatus } from '../../shared/types/status';
import { HttpErrorResponse } from '@angular/common/http';
import { StatusDisplay } from '../../shared/components/status-display/status-display';
import { AirportPageHeader } from '../../shared/components/airport-page-header/airport-page-header';

@Component({
    selector: 'app-airport',
    imports: [
        Stack,
        MatChipsModule,
        AirportLatestWeather,
        AirportHistoryData,
        MatIcon,
        MatButton,
        AsyncPipe,
        StatusDisplay,
        AirportPageHeader,
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
    protected readonly airportHistory = signal<AirportDayHistory | undefined>(undefined);
    protected readonly historyLoadingStatus = signal<LoadingStatus>('loading');
    private readonly historyDate$: BehaviorSubject<Date>;

    // The actual value of this is never used, it is merely to signal a refresh of the data
    // A normal subject would not be suitable because we need it to emit at least once
    private readonly refreshClicked$ = new BehaviorSubject<boolean>(false);

    protected readonly isRefreshDisabled$: Observable<boolean>;

    constructor(
        route: ActivatedRoute,
        private readonly router: Router,
        private readonly airportService: AirportService,
    ) {
        this.isRefreshDisabled$ = combineLatest([
            this.airportService.loadingStatus$,
            toObservable(this.historyLoadingStatus),
        ]).pipe(
            map(([airportStatus, historyStatus]) =>
                airportStatus === 'loading' || historyStatus === 'loading',
            ),
            distinctUntilChanged(),
            switchMap((loading) => loading ? of(true) : timer(1000).pipe(map(() => false))),
            startWith(true),
        );
        const today = new Date();
        this.historyDate$ = new BehaviorSubject<Date>(
            new Date(Date.UTC(today.getFullYear(), today.getMonth(), today.getDate())),
        );

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

        combineLatest([
            toObservable(this.airportIcao).pipe(filter((icao) => !!icao)),
            this.historyDate$,
            this.refreshClicked$,
        ])
            .pipe(
                switchMap(([icao, date]) => {
                    this.historyLoadingStatus.set('loading');
                    this.airportHistory.set(undefined);
                    return this.airportService.getAirportHistory(icao, date).pipe(
                        tap((history) => {
                            this.airportHistory.set(history);
                            this.historyLoadingStatus.set(
                                history.history?.length > 0 ? 'success' : 'notfound',
                            );
                        }),
                        catchError((error: HttpErrorResponse) => {
                            this.historyLoadingStatus.set('error');
                            console.error('Failed to retrieve history', error);
                            return EMPTY;
                        }),
                    );
                }),
                takeUntilDestroyed(),
            )
            .subscribe();
    }

    protected onHistoryDateSelected(date: Date): void {
        this.historyDate$.next(date);
    }

    protected refresh(): void {
        this.refreshClicked$.next(true);
        this.airportService.initializeAirports();
    }

    protected history(): void {
        this.router.navigate(['history', this.airportIcao()]);
    }
}
