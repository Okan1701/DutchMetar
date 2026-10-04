import { AfterViewInit, Component, effect, input, output, ViewChild } from '@angular/core';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle } from '@angular/material/card';
import { MatTable, MatTableDataSource, MatTableModule } from '@angular/material/table';
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
    template: `
        <mat-card appearance="outlined">
            <mat-card-header>
                <mat-card-subtitle>Published TAF History</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
                <app-status-display [status]="status()">
                    <span class="subtext">{{ tafHistory().totalItems | number }} total reports</span>
                    <table mat-table matSort matSortDirection="desc" matSortActive="issuedAt" [dataSource]="dataSource">
                        <ng-container matColumnDef="issuedAt">
                            <th mat-header-cell mat-sort-header class="issued-at-column" *matHeaderCellDef>Issued At</th>
                            <td mat-cell *matCellDef="let element">
                                {{ element.issuedAt ? (element.issuedAt | date:'yyyy-MM-dd HH:mm') + 'z' : 'Unknown' }}
                            </td>
                        </ng-container>
                        <ng-container matColumnDef="taf">
                            <th mat-header-cell *matHeaderCellDef>TAF Text</th>
                            <td mat-cell *matCellDef="let element">{{ element.rawTaf }}</td>
                        </ng-container>
                        <tr mat-header-row *matHeaderRowDef="tableColumns"></tr>
                        <tr mat-row *matRowDef="let row; columns: tableColumns;"></tr>
                    </table>
                </app-status-display>
                <mat-paginator
                    [pageSize]="50"
                    [pageIndex]="tafHistory().currentPage"
                    [showFirstLastButtons]="true"
                    [length]="tafHistory().totalItems"
                    (page)="changePage($event)">
                </mat-paginator>
            </mat-card-content>
        </mat-card>
    `,
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
