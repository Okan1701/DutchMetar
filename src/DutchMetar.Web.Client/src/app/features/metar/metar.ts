import { Component, computed, inject, signal } from '@angular/core';
import { Stack } from '../../shared/components/stack/stack';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { MetarService } from '../../shared/services/metar-service';
import { MetarHistory } from '../../shared/models/metar/metar-history';
import { LoadingStatus } from '../../shared/types/status';
import { MetarList } from './metar-list/metar-list';
import { MetarFilters } from './metar-filters/metar-filters';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MetarFilterModel } from './metar-filters/metar-filters-model';
import { combineLatestWith, skip } from 'rxjs';

@Component({
    selector: 'app-metar',
    imports: [Stack, MetarList, MetarFilters, MatButton, MatIcon],
    templateUrl: './metar.html',
    styleUrl: './metar.scss',
})
export class Metar {
    protected readonly status = signal<LoadingStatus>('loading');
    protected readonly airportIcao = signal<string>('');
    protected readonly metarHistory = signal<MetarHistory>({
        icao: '',
        currentPage: 0,
        maxPages: 0,
        metarReports: [],
        totalItems: 0,
        airportName: '',
    });
    protected readonly airportName = computed(() => this.metarHistory().airportName);

    private readonly route = inject(ActivatedRoute);
    private readonly metarService = inject(MetarService);
    private readonly router = inject(Router);

    private page = signal<number>(0);
    private filters = signal<MetarFilterModel | undefined>(undefined);
    private previousFilters?: MetarFilterModel;

    constructor() {
        let filtersObservable = toObservable(this.filters).pipe(skip(1));
        let pageObservable = toObservable(this.page);

        this.route.params
            .pipe(combineLatestWith(filtersObservable, pageObservable), takeUntilDestroyed())
            .subscribe(([routeParams, filters, page]) => {
                if (
                    filters?.startDate?.getTime() !== this.previousFilters?.startDate?.getTime() ||
                    filters?.endDate?.getTime() !== this.previousFilters?.endDate?.getTime()
                ) {
                    page = 0;
                }

                let icao = routeParams['icao'];
                this.airportIcao.set(icao);
                this.metarService.getMetarHistory({
                    icao: icao,
                    page: page,
                    startDate: filters?.startDate?.toISOString(),
                    endDate: filters?.endDate?.toISOString(),
                });

                this.previousFilters = filters;
            });

        this.metarService.metarHistory$
            .pipe(takeUntilDestroyed())
            .subscribe((metarHistory) => this.metarHistory.set(metarHistory));

        this.metarService.status$
            .pipe(takeUntilDestroyed())
            .subscribe((status) => this.status.set(status));
    }

    protected async returnToAirport(): Promise<void> {
        await this.router.navigate(['airport', this.airportIcao()]);
    }

    protected filtersChanged(filters: MetarFilterModel): void {
        this.filters.set(filters);
    }

    protected pageChanged(page: number): void {
        this.page.set(page);
    }
}
