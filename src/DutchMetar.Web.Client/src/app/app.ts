import { Component, computed, HostListener, signal, ViewChild } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { AirportNavList } from './shared/components/airport-nav-list/airport-nav-list';
import { Bootstrap } from './shared/components/bootstrap/bootstrap';
import { MatIconButton } from '@angular/material/button';
import { environment } from '../environments/environment';
import { NgOptimizedImage } from '@angular/common';
import { UtcClock } from './shared/components/utc-clock/utc-clock';

const MOBILE_WIDTH_BREAKPOINT = 786;

@Component({
    selector: 'app-root',
    imports: [
        RouterOutlet,
        MatToolbarModule,
        MatSidenavModule,
        MatListModule,
        MatIconModule,
        Bootstrap,
        AirportNavList,
        RouterLink,
        MatIconButton,
        NgOptimizedImage,
        UtcClock,
    ],
    templateUrl: './app.html',
    styleUrl: './app.scss',
})
export class App {
    @ViewChild(MatSidenav)
    public matSideNav?: MatSidenav;
    protected showMobileSideNav = computed(() => this.windowWidth() <= MOBILE_WIDTH_BREAKPOINT);
    protected version = computed(() => environment.version);
    private windowWidth = signal<number>(window.innerWidth);

    @HostListener('window:resize')
    public onResize() {
        this.windowWidth.set(window.innerWidth);
    }

    protected toggleSideNav(): void {
        this.matSideNav?.toggle();
    }
}
