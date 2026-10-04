import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AirportTemperatureChart } from './airport-temperature-chart';

describe('AirportTemperatureChart', () => {
    let component: AirportTemperatureChart;
    let fixture: ComponentFixture<AirportTemperatureChart>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportTemperatureChart],
        });
        TestBed.overrideComponent(AirportTemperatureChart, { set: { template: '' } });
        await TestBed.compileComponents();

        fixture = TestBed.createComponent(AirportTemperatureChart);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airportHistory', { icao: 'EHAM', isMissingData: false, history: [] });
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
