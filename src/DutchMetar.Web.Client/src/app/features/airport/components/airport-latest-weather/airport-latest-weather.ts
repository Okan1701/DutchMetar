import { Component, computed, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { Stack } from '../../../../shared/components/stack/stack';
import { AirportDetails } from '../../../../shared/models/airport-details';
import { LatestMetarTafCard } from './components/latest-metar-taf-card/latest-metar-taf-card';
import { DecodedObservations } from './components/decoded-observations/decoded-observations';

@Component({
    selector: 'app-airport-latest-weather',
    imports: [MatCardModule, Stack, LatestMetarTafCard, DecodedObservations],
    templateUrl: './airport-latest-weather.html',
    styleUrl: './airport-latest-weather.scss',
})
export class AirportLatestWeather {
    public airportDetails = input.required<AirportDetails>();
}
