import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AirportVisibilityChart } from './airport-visibility-chart';

describe('AirportVisibilityChart', () => {
    let component: AirportVisibilityChart;
    let fixture: ComponentFixture<AirportVisibilityChart>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportVisibilityChart],
        }).compileComponents();

        fixture = TestBed.createComponent(AirportVisibilityChart);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});

