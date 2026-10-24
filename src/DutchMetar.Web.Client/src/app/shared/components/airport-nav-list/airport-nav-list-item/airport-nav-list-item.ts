import { Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { RouterLink } from '@angular/router';
import { AirportSummary } from '../../../models/airport/airport-summary';
import { MeteoCondition } from '../../../types/meteo-condition';
import { Badge } from '../../badge/badge';

@Component({
    selector: 'app-airport-nav-list-item',
    imports: [DatePipe, MatIconModule, MatRippleModule, RouterLink, Badge],
    templateUrl: './airport-nav-list-item.html',
    styleUrl: './airport-nav-list-item.scss',
})
export class AirportNavListItem {
    public airport = input.required<AirportSummary>();
    public active = input<boolean>(false);

    protected readonly MeteoCondition = MeteoCondition;
}
