import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AirportTemperatureChart } from './airport-temperature-chart';

describe('AirportTemperatureChart', () => {
    let component: AirportTemperatureChart;
    let fixture: ComponentFixture<AirportTemperatureChart>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportTemperatureChart],
        }).compileComponents();

        fixture = TestBed.createComponent(AirportTemperatureChart);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});

