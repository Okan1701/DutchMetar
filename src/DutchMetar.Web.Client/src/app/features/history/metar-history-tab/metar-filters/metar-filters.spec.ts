import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNativeDateAdapter } from '@angular/material/core';

import { MetarFilters } from './metar-filters';

describe('MetarFilters', () => {
    let component: MetarFilters;
    let fixture: ComponentFixture<MetarFilters>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MetarFilters],
            providers: [provideNativeDateAdapter()],
        }).compileComponents();

        fixture = TestBed.createComponent(MetarFilters);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
