import { Component } from '@angular/core';
import { AirportService } from '../../services/airport-service';
import { map, Observable } from 'rxjs';
import { LoadingStatus } from '../../types/status';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AirportSummary } from '../../models/airport-summary';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { RouterLink, RouterLinkActive } from '@angular/router';

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
    ],
    templateUrl: './airport-nav-list.html',
    styleUrl: './airport-nav-list.scss',
})
export class AirportNavList {
    constructor(private readonly airportService: AirportService) {}

    protected get isLoading$(): Observable<boolean> {
        return this.airportService.loadingStatus$.pipe(
            map((status: LoadingStatus) => status === 'loading'),
        );
    }

    protected get airports$(): Observable<AirportSummary[]> {
        return this.airportService.airports$;
    }
}
