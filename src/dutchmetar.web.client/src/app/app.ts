import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListItem, MatListModule, MatNavList } from '@angular/material/list';
import { MatIcon, MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-root',
    imports: [
        RouterOutlet,
        MatToolbarModule,
        MatSidenavModule,
        MatNavList,
        MatListModule,
        MatIconModule,
    ],
    templateUrl: './app.html',
    styleUrl: './app.scss',
})
export class App {
    protected readonly title = signal('DutchMetar');
}
