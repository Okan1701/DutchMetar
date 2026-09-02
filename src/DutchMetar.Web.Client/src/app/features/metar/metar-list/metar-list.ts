import { AfterViewInit, Component, effect, input, output, ViewChild } from '@angular/core';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle } from '@angular/material/card';
import { MetarHistoryReport } from '../../../shared/models/metar/metar-history-report';
import { MatTable, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe, DecimalPipe } from '@angular/common';
import { MatSort, MatSortHeader } from '@angular/material/sort';
import { MetarHistory } from '../../../shared/models/metar/metar-history';
import { StatusDisplay } from '../../../shared/components/status-display/status-display';
import { LoadingStatus } from '../../../shared/types/status';

const DEFAULT_PAGE_SIZE = 50;

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
        MatSort,
        MatSortHeader,
        DecimalPipe,
        StatusDisplay,
    ],
    templateUrl: './metar-list.html',
    styleUrl: './metar-list.scss',
})
export class MetarList implements AfterViewInit {
    public status = input.required<LoadingStatus>();
    public metarHistory = input.required<MetarHistory>();
    public newPage = output<number>();

    @ViewChild(MatSort) public sort?: MatSort;

    protected dataSource = new MatTableDataSource<MetarHistoryReport>([]);
    protected readonly tableColumns = ['issuedAt', 'metar'];

    constructor() {
        effect(() => {
            this.dataSource.data = this.metarHistory().metarReports;
        });
    }

    public ngAfterViewInit(): void {
        this.dataSource.sort = this.sort;
    }

    protected changePage(pageEvent: PageEvent): void {
        this.newPage.emit(pageEvent.pageIndex);
    }
}
