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
});
