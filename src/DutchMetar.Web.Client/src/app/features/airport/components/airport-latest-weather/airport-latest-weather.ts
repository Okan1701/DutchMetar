import { Component, computed, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { Stack } from '../../../../shared/components/stack/stack';
import { LatestWeatherValueCard } from './latest-weather-value-card/latest-weather-value-card';
import { AirportCurrentMetar } from '../../../../shared/models/airport-current-metar';

@Component({
    selector: 'app-airport-latest-weather',
    imports: [MatCardModule, Stack, LatestWeatherValueCard],
    templateUrl: './airport-latest-weather.html',
    styleUrl: './airport-latest-weather.scss',
})
export class AirportLatestWeather {
    public airportWeather = input.required<AirportCurrentMetar>();

    protected issuedAt = computed(() => {
        let issuedAtDate = new Date(this.airportWeather().issuedAt);

        if (issuedAtDate && isNaN(issuedAtDate.valueOf())) {
            return undefined;
        }

        return issuedAtDate.toISOString().slice(11, 16);
    });
}
