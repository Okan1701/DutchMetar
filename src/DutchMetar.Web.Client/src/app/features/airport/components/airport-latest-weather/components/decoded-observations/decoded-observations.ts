import { Component, computed, ElementRef, inject, input, ViewChild } from '@angular/core';
import { CeilingtypePipe } from '../../../../../../shared/pipes/ceilingtype-pipe';
import { DecimalPipe } from '@angular/common';
import { LatestWeatherValueCard } from './latest-weather-value-card/latest-weather-value-card';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { AirportDetails } from '../../../../../../shared/models/airport/airport-details';
import { MatRipple } from '@angular/material/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { MatSnackBar } from '@angular/material/snack-bar';

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
        MatRipple,
    ],
    templateUrl: './decoded-observations.html',
    styleUrl: './decoded-observations.scss',
})
export class DecodedObservations {
    public airportDetails = input.required<AirportDetails>();
    @ViewChild('ceilingsElement')
    public ceilingsElement?: ElementRef;

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

    protected isMaxVisibility = computed(() => {
        // KNMI uses a max value of 10000 km visibility while their raw METAR only goes up to 9999
        let visibility = this.airportDetails().latestWeather?.visibilityMeters;
        if (!visibility) return false;
        return visibility > 9999;
    });

    private readonly clipboard: Clipboard = inject(Clipboard);
    private readonly snackBar: MatSnackBar = inject(MatSnackBar);

    protected copyCeilingsToClipboard(): void {
        if (this.ceilingsElement) {
            this.clipboard.copy(this.ceilingsElement.nativeElement.innerText);
            this.snackBar.open('Ceilings copied to clipboard!', undefined, {
                duration: 2000,
            });
        }
    }
}
