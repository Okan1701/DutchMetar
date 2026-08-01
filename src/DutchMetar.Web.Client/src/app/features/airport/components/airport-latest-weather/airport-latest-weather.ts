import { Component, computed, inject, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { Stack } from '../../../../shared/components/stack/stack';
import { LatestWeatherValueCard } from './latest-weather-value-card/latest-weather-value-card';
import { AirportCurrentMetar } from '../../../../shared/models/airport-current-metar';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import { Clipboard } from '@angular/cdk/clipboard';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-airport-latest-weather',
    imports: [MatCardModule, Stack, LatestWeatherValueCard, MatTooltip, MatIcon, MatIconButton],
    templateUrl: './airport-latest-weather.html',
    styleUrl: './airport-latest-weather.scss',
})
export class AirportLatestWeather {
    public airportWeather = input.required<AirportCurrentMetar>();
    private readonly clipboard: Clipboard = inject(Clipboard);
    private readonly snackBar: MatSnackBar = inject(MatSnackBar);

    protected issuedAt = computed(() => {
        let issuedAtDate = new Date(this.airportWeather().issuedAt);

        if (issuedAtDate && isNaN(issuedAtDate.valueOf())) {
            return undefined;
        }

        return issuedAtDate.toISOString().slice(11, 16);
    });

    protected copyMetarToClipboard(): void {
        if (this.clipboard.copy(this.airportWeather().rawMetar)) {
            this.snackBar.open('METAR copied to clipboard!', undefined, {
                duration: 2000,
            });
        }
    }
}
