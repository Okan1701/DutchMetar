import { Component } from '@angular/core';
import { AirportService } from '../../services/airport-service';
import { BehaviorSubject, combineLatestWith, map, Observable, startWith } from 'rxjs';
import { CommonModule } from '@angular/common';
import { AirportSummary } from '../../models/airport/airport-summary';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatFormField, MatInput, MatLabel, MatSuffix } from '@angular/material/input';
import { AirportNavListItem } from './airport-nav-list-item/airport-nav-list-item';

@Component({
    selector: 'app-airport-nav-list',
    imports: [
        CommonModule,
        MatListModule,
        MatIconModule,
        MatFormField,
        MatInput,
        MatLabel,
        MatSuffix,
        AirportNavListItem,
    ],
    templateUrl: './airport-nav-list.html',
    styleUrl: './airport-nav-list.scss',
})
export class AirportNavList {
    protected readonly airportItems$: Observable<{ airport: AirportSummary; active: boolean }[]>;

    private readonly searchValue$ = new BehaviorSubject<string>('');

    constructor(airportService: AirportService) {
        this.airportItems$ = airportService.airports$.pipe(
            combineLatestWith(this.searchValue$),
            map(([airports, searchValue]) =>
                searchValue
                    ? airports.filter((x) =>
                          x.icao.toLocaleLowerCase().includes(searchValue.toLowerCase()),
                      )
                    : airports,
            ),
            combineLatestWith(airportService.currentAirport$.pipe(startWith(null))),
            map(([airports, currentAirport]) =>
                airports.map((airport) => ({
                    airport,
                    active: currentAirport?.icao
                        ? airport.icao.toUpperCase() === currentAirport.icao.toUpperCase()
                        : false,
                })),
            ),
        );
    }

    protected onSearchValueChange(event: Event): void {
        this.searchValue$.next((event.target as HTMLInputElement).value);
    }
}
