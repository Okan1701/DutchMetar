import { Component, inject, signal } from '@angular/core';
import { Stack } from '../../shared/components/stack/stack';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTab, MatTabGroup, MatTabContent } from '@angular/material/tabs';
import { MetarHistoryTab } from './metar-history-tab/metar-history-tab';
import { TafHistoryTab } from './taf-history-tab/taf-history-tab';

@Component({
    selector: 'app-history',
    imports: [Stack, MatButton, MatIcon, MatTabGroup, MatTab, MatTabContent, MetarHistoryTab, TafHistoryTab],
    templateUrl: './history.html',
    styleUrl: './history.scss',
})
export class History {
    protected readonly airportIcao = signal('');
    protected readonly airportName = signal<string | undefined>(undefined);

    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);

    constructor() {
        this.route.params.pipe(takeUntilDestroyed()).subscribe((routeParams) => {
            this.airportIcao.set(routeParams['icao']);
            this.airportName.set(undefined);
        });
    }

    protected async returnToAirport(): Promise<void> {
        await this.router.navigate(['airport', this.airportIcao()]);
    }

    protected setAirportName(name: string | undefined): void {
        if (name) {
            this.airportName.set(name);
        }
    }
}
