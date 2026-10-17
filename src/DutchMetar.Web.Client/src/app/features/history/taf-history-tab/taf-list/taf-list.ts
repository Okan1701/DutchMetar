import { Component, effect, input, output } from '@angular/core';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle } from '@angular/material/card';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe, DecimalPipe } from '@angular/common';
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
        DecimalPipe,
        StatusDisplay,
    ],
    templateUrl: './taf-list.html',
    styleUrl: './taf-list.scss',
})
export class TafList {
    public status = input.required<LoadingStatus>();
    public tafHistory = input.required<TafHistory>();
    public newPage = output<number>();

    protected dataSource = new MatTableDataSource<TafHistoryReport>([]);
    protected readonly tableColumns = ['issuedAt', 'taf'];

    constructor() {
        effect(() => {
            this.dataSource.data = this.tafHistory().tafReports;
        });
    }

    protected changePage(pageEvent: PageEvent): void {
        this.newPage.emit(pageEvent.pageIndex);
    }
}
