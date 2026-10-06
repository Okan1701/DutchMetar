import { AfterViewInit, Component, effect, input, output, ViewChild } from '@angular/core';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle } from '@angular/material/card';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe, DecimalPipe } from '@angular/common';
import { MatSort, MatSortHeader } from '@angular/material/sort';
import { TafHistory } from '../../../../shared/models/taf/taf-history';
import { TafHistoryReport } from '../../../../shared/models/taf/taf-history-report';
import { StatusDisplay } from '../../../../shared/components/status-display/status-display';
import { LoadingStatus } from '../../../../shared/types/status';

@Component({
    selector: 'app-taf-list',
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
    templateUrl: './taf-list.html',
    styleUrl: './taf-list.scss',
})
export class TafList implements AfterViewInit {
    public status = input.required<LoadingStatus>();
    public tafHistory = input.required<TafHistory>();
    public newPage = output<number>();

    @ViewChild(MatSort) public sort?: MatSort;

    protected dataSource = new MatTableDataSource<TafHistoryReport>([]);
    protected readonly tableColumns = ['issuedAt', 'taf'];

    constructor() {
        effect(() => {
            this.dataSource.data = this.tafHistory().tafReports;
        });
    }

    public ngAfterViewInit(): void {
        this.dataSource.sort = this.sort;
    }

    protected changePage(pageEvent: PageEvent): void {
        this.newPage.emit(pageEvent.pageIndex);
    }
}
