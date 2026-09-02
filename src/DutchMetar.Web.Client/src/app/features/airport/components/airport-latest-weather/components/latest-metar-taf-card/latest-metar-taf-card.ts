import { Component, computed, inject, input } from '@angular/core';
import { MatCard, MatCardContent } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { Clipboard } from '@angular/cdk/clipboard';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AirportCurrentMetar } from '../../../../../../shared/models/airport/airport-current-metar';
import { AirportForecast } from '../../../../../../shared/models/airport/airport-forecast';
import { DecimalPipe } from '@angular/common';

@Component({
    selector: 'app-latest-metar-taf-card',
    imports: [MatCard, MatCardContent, MatIcon, MatIconButton, MatTooltip, DecimalPipe],
    templateUrl: './latest-metar-taf-card.html',
    styleUrl: './latest-metar-taf-card.scss',
})
export class LatestMetarTafCard {
    public latestMetar = input<AirportCurrentMetar | undefined>();
    public latestForecast = input<AirportForecast | undefined>();

    protected metar = computed(() => this.latestMetar()?.rawMetar ?? '');
    protected taf = computed(() => this.latestForecast()?.rawTaf ?? '');
    protected metarAgeMinutes = computed(() => this.getAgeInMinutes(this.latestMetar()?.issuedAt));
    protected tafAgeHours = computed(() => this.getAgeInMinutes(this.latestForecast()?.issuedAt) / 60);

    private readonly clipboard: Clipboard = inject(Clipboard);
    private readonly snackBar: MatSnackBar = inject(MatSnackBar);

    protected copyMetarToClipboard(): void {
        let metar = this.metar();
        if (!metar) return;

        if (this.clipboard.copy(metar)) {
            this.snackBar.open('METAR copied to clipboard!', undefined, {
                duration: 2000,
            });
        }
    }

    protected copyTafToClipboard(): void {
        let taf = this.taf();
        if (!taf) return;

        if (this.clipboard.copy(taf)) {
            this.snackBar.open('TAF copied to clipboard!', undefined, {
                duration: 2000,
            });
        }
    }

    protected formatTaf(taf: string): string {
        // Normalize whitespace first
        const normalized = taf.trim().replace(/\s+/g, ' ');

        // Forecast groups that should start on a new line.
        // The negative lookahead prevents TEMPO from being split away
        // from a preceding PROB30/40.
        return normalized.replace(
            /\s+(?=(?:BECMG|TEMPO|PROB(?:30|40)(?:\s+TEMPO)?)(?:\s|$))/g,
            '\n  ',
        );
    }

    private getAgeInMinutes(issuedAt?: string): number {
        if (!issuedAt) return 0;
        let parsedIssuedAt = new Date(issuedAt);

        if (isNaN(parsedIssuedAt.valueOf())) return 0;

        let now = new Date();
        console.log(Math.abs(parsedIssuedAt.getTime() - now.getTime()) / 60000)
        return Math.abs(parsedIssuedAt.getTime() - now.getTime()) / 60000;
    }
}
