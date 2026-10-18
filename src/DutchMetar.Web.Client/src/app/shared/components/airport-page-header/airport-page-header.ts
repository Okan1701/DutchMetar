import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { Badge } from '../badge/badge';
import { AirportSummary } from '../../models/airport/airport-summary';
import { MeteoCondition } from '../../types/meteo-condition';

@Component({
    selector: 'app-airport-page-header',
    imports: [DatePipe, Badge, MatIconButton, MatIcon, MatTooltip],
    templateUrl: './airport-page-header.html',
    styleUrl: './airport-page-header.scss',
})
export class AirportPageHeader {
    public airport = input.required<AirportSummary>();
    public refreshDisabled = input<boolean>(false);
    public refreshClicked = output<void>();

    protected readonly MeteoCondition = MeteoCondition;
}
