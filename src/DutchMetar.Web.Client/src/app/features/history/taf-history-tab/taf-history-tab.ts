import { Component, effect, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TafService } from '../../../shared/services/taf-service';
import { TafHistory } from '../../../shared/models/taf/taf-history';
import { LoadingStatus } from '../../../shared/types/status';
import { TafFilters } from './taf-filters/taf-filters';
import { HistoryFilterModel } from '../history-filter-model';
import { TafList } from './taf-list/taf-list';
import { Stack } from '../../../shared/components/stack/stack';

@Component({
    selector: 'app-taf-history-tab',
    imports: [Stack, TafFilters, TafList],
    template: `
        <app-stack>
            <app-taf-filters (filtersChanged)="filtersChanged($event)"></app-taf-filters>
            <app-taf-list
                (newPage)="pageChanged($event)"
                [status]="status()"
                [tafHistory]="tafHistory()">
            </app-taf-list>
        </app-stack>
    `,
})
export class TafHistoryTab {
    public icao = input.required<string>();
    public airportNameChanged = output<string | undefined>();

    protected readonly status = signal<LoadingStatus>('loading');
    protected readonly tafHistory = signal<TafHistory>({
        icao: '',
        currentPage: 0,
        maxPages: 0,
        tafReports: [],
        totalItems: 0,
        airportName: '',
    });

    private readonly page = signal(0);
    private readonly filters = signal<HistoryFilterModel | undefined>(undefined);
    private readonly activeIcao = signal<string | undefined>(undefined);

    constructor(private readonly tafService: TafService) {
        effect(() => this.activeIcao.set(this.icao()));
        effect(() => {
            this.tafService.getTafHistory({
                icao: this.icao(),
                page: this.page(),
                startDate: this.toDate(this.filters()?.startDate),
                endDate: this.toDate(this.filters()?.endDate),
            });
        });

        this.tafService.tafHistory$.pipe(takeUntilDestroyed()).subscribe((history) => {
            if (history.icao !== this.activeIcao()) {
                return;
            }
            this.tafHistory.set(history);
            this.airportNameChanged.emit(history.airportName);
        });
        this.tafService.status$.pipe(takeUntilDestroyed()).subscribe((status) => this.status.set(status));
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
