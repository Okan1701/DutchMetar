import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LatestWeatherValueCard } from './latest-weather-value-card';

describe('LatestWeatherValueCard', () => {
    let component: LatestWeatherValueCard;
    let fixture: ComponentFixture<LatestWeatherValueCard>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [LatestWeatherValueCard],
        }).compileComponents();

        fixture = TestBed.createComponent(LatestWeatherValueCard);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
