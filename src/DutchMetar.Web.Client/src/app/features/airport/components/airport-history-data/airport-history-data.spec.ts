import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { AirportHistoryData } from './airport-history-data';
import { By } from '@angular/platform-browser';
import { FormGroupDirective } from '@angular/forms';
import { AirportDayHistory } from '../../../../shared/models/airport/airport-day-history';
import { AirportTemperatureChart } from './components/airport-temperature-chart/airport-temperature-chart';
import { AirportDewpointChart } from './components/airport-dewpoint-chart/airport-dewpoint-chart';
import { AirportVisibilityChart } from './components/airport-visibility-chart/airport-visibility-chart';
import { AirportWindSpeedChart } from './components/airport-wind-speed-chart/airport-wind-speed-chart';

describe('AirportHistoryData', () => {
    let component: AirportHistoryData;
    let fixture: ComponentFixture<AirportHistoryData>;
    const charts = [
        AirportTemperatureChart,
        AirportDewpointChart,
        AirportVisibilityChart,
        AirportWindSpeedChart,
    ];
    const history: AirportDayHistory = {
        icao: 'EHAM',
        isMissingData: false,
        history: [{ dateTime: '2026-10-17T12:00:00Z', temperatureCelsius: 15 }],
    };

    function selectDate(date: Date | null): void {
        const form: FormGroupDirective = fixture.debugElement.query(
            By.directive(FormGroupDirective),
        ).componentInstance;
        form.form.controls['selectedDate'].setValue(date);
    }

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportHistoryData],
            providers: [provideNativeDateAdapter()],
        });
        for (const chart of charts) {
            TestBed.overrideComponent(chart, { set: { template: '' } });
        }
        await TestBed.compileComponents();

        fixture = TestBed.createComponent(AirportHistoryData);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airportHistory', undefined);
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('passes supplied history and updated history to all charts', () => {
        fixture.componentRef.setInput('airportHistory', history);
        fixture.detectChanges();

        for (const chart of charts) {
            expect(
                fixture.debugElement.query(By.directive(chart)).componentInstance.airportHistory(),
            ).toBe(history);
        }

        const updatedHistory = { ...history, history: [...history.history, ...history.history] };
        fixture.componentRef.setInput('airportHistory', updatedHistory);
        fixture.detectChanges();

        for (const chart of charts) {
            expect(
                fixture.debugElement.query(By.directive(chart)).componentInstance.airportHistory(),
            ).toBe(updatedHistory);
        }
    });

    it('debounces date edits and emits the local calendar date at UTC midnight', () => {
        vi.useFakeTimers();
        try {
            const dateSelected = vi.fn();
            component.dateSelected.subscribe(dateSelected);
            selectDate(new Date(2026, 0, 14));
            vi.advanceTimersByTime(100);
            selectDate(new Date(2026, 0, 15));
            vi.advanceTimersByTime(199);
            expect(dateSelected).not.toHaveBeenCalled();

            vi.advanceTimersByTime(1);

            expect(dateSelected).toHaveBeenCalledExactlyOnceWith(new Date('2026-01-15T00:00:00Z'));
        } finally {
            vi.useRealTimers();
        }
    });

    it('does not emit missing or future dates', () => {
        vi.useFakeTimers();
        try {
            const dateSelected = vi.fn();
            component.dateSelected.subscribe(dateSelected);
            selectDate(null);
            vi.advanceTimersByTime(200);
            selectDate(new Date(new Date().getFullYear() + 1, 0, 1));
            vi.advanceTimersByTime(200);

            expect(dateSelected).not.toHaveBeenCalled();
        } finally {
            vi.useRealTimers();
        }
    });

    it('does not emit a pending date edit after destruction', () => {
        vi.useFakeTimers();
        try {
            const dateSelected = vi.fn();
            component.dateSelected.subscribe(dateSelected);
            selectDate(new Date(2026, 0, 15));
            fixture.destroy();
            vi.advanceTimersByTime(200);

            expect(dateSelected).not.toHaveBeenCalled();
        } finally {
            vi.useRealTimers();
        }
    });
});
