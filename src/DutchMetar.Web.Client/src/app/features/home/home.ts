import { Component, inject } from '@angular/core';
import { AirportService } from '../../shared/services/airport-service';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-home',
    imports: [],
    templateUrl: './home.html',
    styleUrl: './home.scss',
})
export class Home {
    private readonly airportService: AirportService = inject(AirportService);
    private readonly router: Router = inject(Router);
    private isInitialized: boolean = false;

    constructor() {
        this.airportService.airports$
            .pipe(takeUntilDestroyed())
            .subscribe(airports => {
                if (airports.length > 0 && !this.isInitialized) {
                    this.router.navigate([`/airport/${airports[0].icao}`]);
                } 
        })
    }
}
