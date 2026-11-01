import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AirportWindSpeedChart } from './airport-wind-speed-chart';

describe('AirportWindSpeedChart', () => {
    let component: AirportWindSpeedChart;
    let fixture: ComponentFixture<AirportWindSpeedChart>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportWindSpeedChart],
        });
        TestBed.overrideComponent(AirportWindSpeedChart, { set: { template: '' } });
        await TestBed.compileComponents();

        fixture = TestBed.createComponent(AirportWindSpeedChart);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airportHistory', { icao: 'EHAM', isMissingData: false, history: [] });
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('uses full-width native sizing without animations for initial and updated history', () => {
        for (const history of [[], [{ dateTime: '2026-11-01T12:00:00Z', windSpeedKnots: 10 }]]) {
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
