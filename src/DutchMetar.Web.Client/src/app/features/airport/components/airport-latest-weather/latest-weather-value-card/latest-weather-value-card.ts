import { Component, input, ChangeDetectionStrategy } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
    selector: 'app-latest-weather-value-card',
    imports: [MatCardModule],
    templateUrl: './latest-weather-value-card.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './latest-weather-value-card.scss',
})
export class LatestWeatherValueCard {
    public label = input<string>('');
}
