import { Component, computed, inject, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { Stack } from '../../../../shared/components/stack/stack';
import { LatestWeatherValueCard } from './latest-weather-value-card/latest-weather-value-card';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import { Clipboard } from '@angular/cdk/clipboard';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AirportDetails } from '../../../../shared/models/airport-details';
import { LatestMetarTafCard } from './latest-metar-taf-card/latest-metar-taf-card';

@Component({
    selector: 'app-airport-latest-weather',
    imports: [
        MatCardModule,
        Stack,
        LatestWeatherValueCard,
        MatTooltip,
        MatIcon,
        MatIconButton,
        LatestMetarTafCard,
    ],
    templateUrl: './airport-latest-weather.html',
    styleUrl: './airport-latest-weather.scss',
})
export class AirportLatestWeather {
    public airportDetails = input.required<AirportDetails>();

    private readonly clipboard: Clipboard = inject(Clipboard);
    private readonly snackBar: MatSnackBar = inject(MatSnackBar);

    protected issuedAt = computed(() => {
        let issuedAtString = this.airportDetails().latestWeather?.issuedAt;
        if (!issuedAtString) return undefined;

        let issuedAtDate = new Date(issuedAtString);

        if (issuedAtDate && isNaN(issuedAtDate.valueOf())) {
            return undefined;
        }

        return issuedAtDate.toISOString().slice(11, 16);
    });
}
