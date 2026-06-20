import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AirportService } from '../../services/airport-service';
import { BehaviorSubject, combineLatestWith, map, Observable } from 'rxjs';
import { LoadingStatus } from '../../types/status';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AirportSummary } from '../../models/airport-summary';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatFormField, MatInput, MatLabel, MatSuffix } from '@angular/material/input';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
    selector: 'app-airport-nav-list',
    imports: [
        CommonModule,
        MatProgressSpinnerModule,
        MatListModule,
        MatIconModule,
        MatRippleModule,
        MatChipsModule,
        RouterLink,
        RouterLinkActive,
        MatFormField,
        MatInput,
        FormsModule,
        MatLabel,
        ReactiveFormsModule,
        MatIconModule,
        MatSuffix,
    ],
    templateUrl: './airport-nav-list.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './airport-nav-list.scss',
})
export class AirportNavList {
    private searchValue$: BehaviorSubject<string> = new BehaviorSubject<string>('');

    constructor(private readonly airportService: AirportService) {}

    protected get isLoading$(): Observable<boolean> {
        return this.airportService.loadingStatus$.pipe(
            map((status: LoadingStatus) => status === 'loading'),
        );
    }

    protected get airports$(): Observable<AirportSummary[]> {
        return this.airportService.airports$.pipe(
            combineLatestWith(this.searchValue$),
            map(([airports, searchValue]) =>
                searchValue
                    ? airports.filter((x) =>
                          x.icao.toLocaleLowerCase().includes(searchValue.toLowerCase()),
                      )
                    : airports,
            ),
        );
    }

    protected onSearchValueChange(event: Event): void {
        this.searchValue$.next((event.target as HTMLInputElement).value);
    }
}
