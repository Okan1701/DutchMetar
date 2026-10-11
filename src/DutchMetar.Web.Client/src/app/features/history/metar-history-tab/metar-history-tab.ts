import { Component, effect, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MetarService } from '../../../shared/services/metar-service';
import { MetarHistory } from '../../../shared/models/metar/metar-history';
import { LoadingStatus } from '../../../shared/types/status';
import { MetarList } from './metar-list/metar-list';
import { MetarFilters } from './metar-filters/metar-filters';
import { HistoryFilterModel } from '../history-filter-model';
import { Stack } from '../../../shared/components/stack/stack';

@Component({
    selector: 'app-metar-history-tab',
    imports: [Stack, MetarList, MetarFilters],
    templateUrl: './metar-history-tab.html',
})
export class MetarHistoryTab {
    public icao = input.required<string>();
    public airportNameChanged = output<string | undefined>();

    protected readonly status = signal<LoadingStatus>('loading');
    protected readonly metarHistory = signal<MetarHistory>({
        icao: '',
        currentPage: 0,
        maxPages: 0,
        metarReports: [],
        totalItems: 0,
        airportName: '',
    });

    private readonly page = signal(0);
    private readonly filters = signal<HistoryFilterModel | undefined>(undefined);
    private readonly activeIcao = signal<string | undefined>(undefined);

    constructor(private readonly metarService: MetarService) {
        effect(() => this.activeIcao.set(this.icao()));
        effect(() => {
            this.metarService.getMetarHistory({
                icao: this.icao(),
                page: this.page(),
                startDate: this.toDate(this.filters()?.startDate),
                endDate: this.toDate(this.filters()?.endDate),
            });
        });

        this.metarService.metarHistory$.pipe(takeUntilDestroyed()).subscribe((history) => {
            if (history.icao !== this.activeIcao()) {
                return;
            }
            this.metarHistory.set(history);
            this.airportNameChanged.emit(history.airportName);
        });
        this.metarService.status$.pipe(takeUntilDestroyed()).subscribe((status) => this.status.set(status));
    }

    protected filtersChanged(filters: HistoryFilterModel): void {
        if (
            this.filters()?.startDate?.getTime() === filters.startDate?.getTime() &&
            this.filters()?.endDate?.getTime() === filters.endDate?.getTime()
        ) {
            return;
        }
        this.filters.set(filters);
        this.page.set(0);
    }

    protected pageChanged(page: number): void {
        this.page.set(page);
    }

    private toDate(date: Date | null | undefined): string | undefined {
        if (!date) {
            return undefined;
        }

        return [
            date.getFullYear(),
            String(date.getMonth() + 1).padStart(2, '0'),
            String(date.getDate()).padStart(2, '0'),
        ].join('-');
    }
}
