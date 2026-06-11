import { Component, computed, input, OnDestroy, OnInit, signal } from '@angular/core';
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
import { ApexOptions, NgApexchartsModule } from 'ng-apexcharts';
import { AirportDayHistorySnapshot } from '../../../../shared/models/airport-day-history-snapshot';
import { Stack } from '../../../../shared/components/stack/stack';

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
        NgApexchartsModule,
        Stack,
    ],
    templateUrl: './airport-history-data.html',
    styleUrl: './airport-history-data.scss',
})
export class AirportHistoryData implements OnInit, OnDestroy {
    public airportIcao = input.required<string>();

    protected loadingStatus = signal<LoadingStatus>('loading');
    protected airportHistory = signal<AirportDayHistory | undefined>(undefined);
    protected airportHistoryJson = computed<string>(() => JSON.stringify(this.airportHistory()));
    protected isLoading = computed(() => this.loadingStatus() === 'loading');
    protected readonly maxDate = new Date();
    protected chartOptions?: ApexOptions;

    private dateSelected$ = new BehaviorSubject<Date>(new Date());
    private unsubscribe$ = new Subject<void>();

    protected form = new FormGroup({
        selectedDate: new FormControl<Date>(new Date(), Validators.required),
    });

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
            this.createChartOptions(history.history);
        } else {
            this.loadingStatus.set('notfound');
            this.airportHistory.set(undefined);
        }
    }

    private onRetrievalError(error: HttpErrorResponse): void {
        this.loadingStatus.set('error');
        console.error('Failed to retrieve history', error);
        this.airportHistory.set(undefined);
    }

    private createChartOptions(history: AirportDayHistorySnapshot[]): void {
        this.chartOptions = {
            theme: {
                mode: 'dark',
                monochrome: {
                    enabled: true,
                    color: '#005CBBFF',
                    shadeTo: 'dark',
                    shadeIntensity: 0.65,
                },
            },
            series: [
                {
                    name: 'Temperature (°C)',
                    data: history
                        .filter((d) => d.temperatureCelsius !== undefined)
                        .map((d) => d.temperatureCelsius!),
                },
            ],
            chart: {
                type: 'line',
                height: 350,
                background: 'transparent',
                zoom: {
                    enabled: false,
                },
            },
            xaxis: {
                type: 'datetime',
                labels: {
                    format: 'HH:mm',
                    datetimeFormatter: {
                        hour: 'HH:mm',
                    },
                },
                tickAmount: 24,
                categories: history
                    .filter((d) => d.temperatureCelsius !== undefined)
                    .map((d) => d.dateTime),
            },
            yaxis: {
                title: {
                    text: 'Temperature (°C)',
                },
            },
            stroke: {
                curve: 'smooth',
            },
            tooltip: {
                x: {
                    format: 'HH:mm',
                },
            },
            dataLabels: {
                enabled: false,
            },
            markers: {
                size: 3,
            },
        };
    }
}
