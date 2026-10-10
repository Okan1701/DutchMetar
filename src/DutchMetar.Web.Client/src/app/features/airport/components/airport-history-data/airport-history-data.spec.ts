import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { of } from 'rxjs';
import { AirportService } from '../../../../shared/services/airport-service';
import { AirportHistoryData } from './airport-history-data';

describe('AirportHistoryData', () => {
    let component: AirportHistoryData;
    let fixture: ComponentFixture<AirportHistoryData>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportHistoryData],
            providers: [
                provideNativeDateAdapter(),
                {
                    provide: AirportService,
                    useValue: { getAirportHistory: () => of({ history: [], icao: 'EHAM', isMissingData: false }) },
                },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(AirportHistoryData);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airportIcao', 'EHAM');
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
