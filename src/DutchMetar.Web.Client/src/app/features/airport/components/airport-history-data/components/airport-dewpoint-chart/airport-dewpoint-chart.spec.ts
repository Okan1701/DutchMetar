import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AirportDewpointChart } from './airport-dewpoint-chart';

describe('AirportDewpointChart', () => {
    let component: AirportDewpointChart;
    let fixture: ComponentFixture<AirportDewpointChart>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportDewpointChart],
        });
        TestBed.overrideComponent(AirportDewpointChart, { set: { template: '' } });
        await TestBed.compileComponents();

        fixture = TestBed.createComponent(AirportDewpointChart);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airportHistory', { icao: 'EHAM', isMissingData: false, history: [] });
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
