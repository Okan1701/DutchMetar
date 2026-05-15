import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule, MatNavList } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { AirportService } from './shared/services/airport-service';
import { AirportNavList } from './shared/components/airport-nav-list/airport-nav-list';

@Component({
    selector: 'app-root',
    imports: [
        RouterOutlet,
        MatToolbarModule,
        MatSidenavModule,
        MatNavList,
        MatListModule,
        MatIconModule,
        AirportNavList,
    ],
    templateUrl: './app.html',
    styleUrl: './app.scss',
})
export class App implements OnInit {
    constructor(private readonly airportService: AirportService) {}

    public ngOnInit(): void {
        this.airportService.initializeAirports();
    }
}
