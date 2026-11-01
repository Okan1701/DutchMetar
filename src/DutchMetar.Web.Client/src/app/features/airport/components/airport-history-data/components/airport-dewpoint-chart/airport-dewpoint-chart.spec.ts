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

    it('uses full-width native sizing without animations for initial and updated history', () => {
        for (const history of [[], [{ dateTime: '2026-11-01T12:00:00Z', dewpointCelsius: 10 }]]) {
            fixture.componentRef.setInput('airportHistory', { icao: 'EHAM', isMissingData: false, history });
            fixture.detectChanges();
            expect(component['chartOptions']()?.chart).toMatchObject({
                width: '100%',
                animations: { enabled: false },
            });
            expect(component['chartOptions']()?.chart?.redrawOnParentResize).not.toBe(false);
            expect(component['chartOptions']()?.chart?.redrawOnWindowResize).not.toBe(false);
        }
    });
});
