import { Component, computed, HostListener, OnInit, signal, ViewChild } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { AirportService } from './shared/services/airport-service';
import { AirportNavList } from './shared/components/airport-nav-list/airport-nav-list';
import { MatIconButton } from '@angular/material/button';

const MOBILE_WIDTH_BREAKPOINT = 786;

@Component({
    selector: 'app-root',
    imports: [
        RouterOutlet,
        MatToolbarModule,
        MatSidenavModule,
        MatListModule,
        MatIconModule,
        AirportNavList,
        RouterLink,
        MatIconButton,
    ],
    templateUrl: './app.html',
    styleUrl: './app.scss',
})
export class App implements OnInit {
    @ViewChild(MatSidenav)
    public matSideNav?: MatSidenav;
    protected showMobileSideNav = computed(() => this.windowWidth() <= MOBILE_WIDTH_BREAKPOINT);
    private windowWidth = signal<number>(0);

    constructor(private readonly airportService: AirportService) {}

    public ngOnInit(): void {
        this.airportService.initializeAirports();
        this.windowWidth.set(window.innerWidth);
    }

    @HostListener('window:resize')
    public onResize() {
        this.windowWidth.set(window.innerWidth);
    }

    protected toggleSideNav(): void {
        this.matSideNav?.toggle();
    }
}
