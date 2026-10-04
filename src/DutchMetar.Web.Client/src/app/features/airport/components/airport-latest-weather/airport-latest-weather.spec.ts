import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MeteoCondition } from '../../../../shared/types/meteo-condition';
import { AirportLatestWeather } from './airport-latest-weather';

describe('AirportLatestWeather', () => {
    let component: AirportLatestWeather;
    let fixture: ComponentFixture<AirportLatestWeather>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportLatestWeather],
        }).compileComponents();

        fixture = TestBed.createComponent(AirportLatestWeather);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airportDetails', {
            icao: 'EHAM',
            meteoCondition: MeteoCondition.None,
            lastUpdated: new Date(),
        });
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
