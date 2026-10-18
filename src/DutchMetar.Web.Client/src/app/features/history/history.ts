import { Component, inject, signal } from '@angular/core';
import { Stack } from '../../shared/components/stack/stack';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTab, MatTabGroup, MatTabContent } from '@angular/material/tabs';
import { AsyncPipe } from '@angular/common';
import { combineLatest, distinctUntilChanged, map, Observable, of, startWith, switchMap, timer } from 'rxjs';
import { MetarHistoryTab } from './metar-history-tab/metar-history-tab';
import { TafHistoryTab } from './taf-history-tab/taf-history-tab';
import { AirportPageHeader } from '../../shared/components/airport-page-header/airport-page-header';
import { AirportService } from '../../shared/services/airport-service';
import { AirportSummary } from '../../shared/models/airport/airport-summary';
import { MeteoCondition } from '../../shared/types/meteo-condition';
import { LoadingStatus } from '../../shared/types/status';

@Component({
    selector: 'app-history',
    imports: [
        Stack,
        MatButton,
        MatIcon,
        MatTabGroup,
        MatTab,
        MatTabContent,
        MetarHistoryTab,
        TafHistoryTab,
        AirportPageHeader,
        AsyncPipe,
    ],
    templateUrl: './history.html',
    styleUrl: './history.scss',
})
export class History {
    protected readonly airportIcao = signal('');
    protected readonly airportDetails = signal<AirportSummary>({
        name: '',
        icao: '',
        lastUpdated: new Date(),
        meteoCondition: MeteoCondition.None,
    });
    protected readonly refreshToken = signal(0);
    protected readonly tabLoadingStatus = signal<LoadingStatus>('loading');
    protected readonly isRefreshDisabled$: Observable<boolean>;

    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly airportService = inject(AirportService);

    constructor() {
        this.isRefreshDisabled$ = combineLatest([
            this.airportService.loadingStatus$,
            toObservable(this.tabLoadingStatus),
        ]).pipe(
            map(([airportStatus, tabStatus]) => airportStatus === 'loading' || tabStatus === 'loading'),
            distinctUntilChanged(),
            switchMap((loading) => (loading ? of(true) : timer(1000).pipe(map(() => false)))),
            startWith(true),
        );

        this.route.params.pipe(takeUntilDestroyed()).subscribe((routeParams) => {
            const icao = routeParams['icao'];
            this.airportIcao.set(icao);
            if (icao) {
                this.airportService.setCurrentAirport(icao);
            }
        });

        this.airportService.currentAirport$
            .pipe(takeUntilDestroyed())
            .subscribe((airport) => this.airportDetails.set(airport));
    }

    protected async returnToAirport(): Promise<void> {
        await this.router.navigate(['airport', this.airportIcao()]);
    }

    protected refresh(): void {
        this.airportService.initializeAirports();
        this.refreshToken.update((token) => token + 1);
    }

    protected setTabLoadingStatus(status: LoadingStatus): void {
        this.tabLoadingStatus.set(status);
    }
}
