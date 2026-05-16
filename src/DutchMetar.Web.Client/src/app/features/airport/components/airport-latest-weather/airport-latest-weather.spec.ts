import { ComponentFixture, TestBed } from '@angular/core/testing';

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
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
