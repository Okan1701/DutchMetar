import { Component, computed, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { NgApexchartsModule, ApexOptions } from 'ng-apexcharts';
import { AirportDayHistory } from '../../../../../../shared/models/airport-day-history';
import { AirportDayHistorySnapshot } from '../../../../../../shared/models/airport-day-history-snapshot';

@Component({
    selector: 'app-airport-dewpoint-chart',
    imports: [MatCardModule, NgApexchartsModule],
    templateUrl: './airport-dewpoint-chart.html',
    styleUrl: './airport-dewpoint-chart.scss',
})
export class AirportDewpointChart {
    public airportHistory = input.required<AirportDayHistory>();
    protected chartOptions = computed(() => {
        const history = this.airportHistory();
        return history && history.history ? this.createChartOptions(history.history) : undefined;
    });

    private createChartOptions(history: AirportDayHistorySnapshot[]): ApexOptions {
        return {
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
                    name: 'Dewpoint (°C)',
                    data: history.map((d) => d.dewpointCelsius ?? null),
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
                categories: history.map((d) => d.dateTime),
            },
            yaxis: {
                title: {
                    text: 'Dewpoint (°C)',
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
