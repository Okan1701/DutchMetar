import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AirportHistoryData } from './airport-history-data';

describe('AirportHistoryData', () => {
    let component: AirportHistoryData;
    let fixture: ComponentFixture<AirportHistoryData>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportHistoryData],
        }).compileComponents();

        fixture = TestBed.createComponent(AirportHistoryData);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
