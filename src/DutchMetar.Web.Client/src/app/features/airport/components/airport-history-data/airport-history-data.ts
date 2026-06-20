import {
    Component,
    input,
    OnDestroy,
    OnInit,
    signal,
    ChangeDetectionStrategy,
} from '@angular/core';
import { AirportDayHistory } from '../../../../shared/models/airport-day-history';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AirportService } from '../../../../shared/services/airport-service';
import { BehaviorSubject, debounceTime, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { LoadingStatus } from '../../../../shared/types/status';
import { StatusDisplay } from '../../../../shared/components/status-display/status-display';
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
        StatusDisplay,
        AirportTemperatureChart,
        AirportDewpointChart,
        AirportVisibilityChart,
        AirportWindSpeedChart,
        Stack,
    ],
    templateUrl: './airport-history-data.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './airport-history-data.scss',
})
export class AirportHistoryData implements OnInit, OnDestroy {
    public airportIcao = input.required<string>();

    protected loadingStatus = signal<LoadingStatus>('loading');
    protected airportHistory = signal<AirportDayHistory>({
        history: [],
        icao: '',
        isMissingData: false,
    });
    protected readonly maxDate = new Date();
    protected form = new FormGroup({
        selectedDate: new FormControl<Date>(new Date(), Validators.required),
    });

    private dateSelected$ = new BehaviorSubject<Date>(new Date());
    private unsubscribe$ = new Subject<void>();

    constructor(private readonly airportService: AirportService) {
        this.form.valueChanges.pipe(takeUntilDestroyed(), debounceTime(200)).subscribe(() => {
            if (this.form.valid && this.form.controls.selectedDate.value != null) {
                // Convert to UTC format without shifting the day due to timezone change
                let utcDateEpoch = Date.UTC(
                    this.form.controls.selectedDate.value.getFullYear(),
                    this.form.controls.selectedDate.value.getMonth(),
                    this.form.controls.selectedDate.value.getDate(),
                );
                this.dateSelected$.next(new Date(utcDateEpoch));
            }
        });
    }

    public ngOnInit() {
        this.dateSelected$
            .pipe(
                takeUntil(this.unsubscribe$),
                tap(() => this.loadingStatus.set('loading')),
                switchMap((date: Date) =>
                    this.airportService.getAirportHistory(this.airportIcao(), date),
                ),
            )
            .subscribe({
                next: (data) => this.onHistoryRetrieved(data),
                error: (err) => this.onRetrievalError(err),
            });
    }

    public ngOnDestroy() {
        this.unsubscribe$.next();
    }

    private onHistoryRetrieved(history: AirportDayHistory): void {
        if (history.history && history.history.length > 0) {
            this.loadingStatus.set('success');
            this.airportHistory.set(history);
        } else {
            this.loadingStatus.set('notfound');
        }
    }

    private onRetrievalError(error: HttpErrorResponse): void {
        this.loadingStatus.set('error');
        console.error('Failed to retrieve history', error);
    }
}
