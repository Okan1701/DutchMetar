import { Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';

@Component({
    selector: 'app-latest-weather-value-card',
    imports: [MatCardModule, MatIcon],
    templateUrl: './latest-weather-value-card.html',
    styleUrl: './latest-weather-value-card.scss',
})
export class LatestWeatherValueCard {
    public label = input<string>('');
    public icon = input<string>();
}
