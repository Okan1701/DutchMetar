import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LatestMetarTafCard } from './latest-metar-taf-card';

describe('LatestMetarTafCard', () => {
    let component: LatestMetarTafCard;
    let fixture: ComponentFixture<LatestMetarTafCard>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [LatestMetarTafCard],
        }).compileComponents();

        fixture = TestBed.createComponent(LatestMetarTafCard);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
