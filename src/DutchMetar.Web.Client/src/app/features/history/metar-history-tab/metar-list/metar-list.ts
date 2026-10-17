import { Component, effect, input, output } from '@angular/core';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle } from '@angular/material/card';
import { MetarHistoryReport } from '../../../../shared/models/metar/metar-history-report';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe, DecimalPipe } from '@angular/common';
import { MetarHistory } from '../../../../shared/models/metar/metar-history';
import { StatusDisplay } from '../../../../shared/components/status-display/status-display';
import { LoadingStatus } from '../../../../shared/types/status';

@Component({
    selector: 'app-metar-list',
    imports: [
        MatCard,
        MatCardContent,
        MatCardHeader,
        MatCardSubtitle,
        MatTableModule,
        MatPaginator,
        DatePipe,
        DecimalPipe,
        StatusDisplay,
    ],
    templateUrl: './metar-list.html',
    styleUrl: './metar-list.scss',
})
export class MetarList {
    public status = input.required<LoadingStatus>();
    public metarHistory = input.required<MetarHistory>();
    public newPage = output<number>();

    protected dataSource = new MatTableDataSource<MetarHistoryReport>([]);
    protected readonly tableColumns = ['issuedAt', 'metar'];

    constructor() {
        effect(() => {
            this.dataSource.data = this.metarHistory().metarReports;
        });
    }

    protected changePage(pageEvent: PageEvent): void {
        this.newPage.emit(pageEvent.pageIndex);
    }
}
