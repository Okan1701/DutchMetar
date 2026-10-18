import { Component, input, output } from '@angular/core';
import { AirportDayHistory } from '../../../../shared/models/airport/airport-day-history';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Stack } from '../../../../shared/components/stack/stack';
import { AirportTemperatureChart } from './components/airport-temperature-chart/airport-temperature-chart';
import { AirportDewpointChart } from './components/airport-dewpoint-chart/airport-dewpoint-chart';
import { AirportVisibilityChart } from './components/airport-visibility-chart/airport-visibility-chart';
import { AirportWindSpeedChart } from './components/airport-wind-speed-chart/airport-wind-speed-chart';

@Component({
    selector: 'app-airport-history-data',
    imports: [
        MatCardModule,
        MatFormFieldModule,
        MatDatepickerModule,
        MatInputModule,
        MatIconModule,
        MatButtonModule,
        MatTooltipModule,
        ReactiveFormsModule,
        AirportTemperatureChart,
        AirportDewpointChart,
        AirportVisibilityChart,
        AirportWindSpeedChart,
        Stack,
    ],
    templateUrl: './airport-history-data.html',
    styleUrl: './airport-history-data.scss',
})
export class AirportHistoryData {
    public readonly airportHistory = input.required<AirportDayHistory | undefined>();
    public readonly dateSelected = output<Date>();
    protected readonly maxDate = new Date();
    protected form = new FormGroup({
        selectedDate: new FormControl<Date>(new Date(), Validators.required),
    });

    constructor() {
        this.form.valueChanges.pipe(debounceTime(200), takeUntilDestroyed()).subscribe(() => {
            if (this.form.valid && this.form.controls.selectedDate.value != null) {
                // Convert to UTC format without shifting the day due to timezone change
                let utcDateEpoch = Date.UTC(
                    this.form.controls.selectedDate.value.getFullYear(),
                    this.form.controls.selectedDate.value.getMonth(),
                    this.form.controls.selectedDate.value.getDate(),
                );
                this.dateSelected.emit(new Date(utcDateEpoch));
            }
        });
    }
}
