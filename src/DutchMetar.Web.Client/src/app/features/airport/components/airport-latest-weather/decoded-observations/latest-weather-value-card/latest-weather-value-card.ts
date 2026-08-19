import { Component, ElementRef, inject, input, ViewChild } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { MatRipple } from '@angular/material/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-latest-weather-value-card',
    imports: [MatCardModule, MatIcon, MatRipple],
    templateUrl: './latest-weather-value-card.html',
    styleUrl: './latest-weather-value-card.scss',
})
export class LatestWeatherValueCard {
    public label = input<string>('');
    public icon = input<string>();
    @ViewChild('value') public valueElement?: ElementRef;

    private readonly clipboard: Clipboard = inject(Clipboard);
    private readonly snackBar: MatSnackBar = inject(MatSnackBar);

    protected copyToClipboard() {
        console.warn(this.valueElement)
        if (this.valueElement) {
            this.clipboard.copy(this.valueElement.nativeElement.innerText);
            this.snackBar.open('Value copied to clipboard!', undefined, {
                duration: 2000,
            });
        }
    }
}
