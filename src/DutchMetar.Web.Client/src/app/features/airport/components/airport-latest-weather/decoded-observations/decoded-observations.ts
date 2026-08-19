import { Component, computed, input } from '@angular/core';
import { CeilingtypePipe } from '../../../../../shared/pipes/ceilingtype-pipe';
import { DecimalPipe } from '@angular/common';
import { LatestWeatherValueCard } from './latest-weather-value-card/latest-weather-value-card';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { AirportDetails } from '../../../../../shared/models/airport-details';

@Component({
    selector: 'app-decoded-observations',
    imports: [
        CeilingtypePipe,
        DecimalPipe,
        LatestWeatherValueCard,
        MatCard,
        MatCardContent,
        MatCardHeader,
        MatCardSubtitle,
        MatIcon,
    ],
    templateUrl: './decoded-observations.html',
    styleUrl: './decoded-observations.scss',
})
export class DecodedObservations {
    public airportDetails = input.required<AirportDetails>();

    protected sortedCeilings = computed(() => {
        let ceilings = this.airportDetails().latestWeather?.ceilings ?? [];
        return ceilings.sort((x) => x.type);
    });

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
