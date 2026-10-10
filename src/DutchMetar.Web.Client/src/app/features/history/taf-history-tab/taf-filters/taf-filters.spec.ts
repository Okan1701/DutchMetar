import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { TafFilters } from './taf-filters';

describe('TafFilters', () => {
    let fixture: ComponentFixture<TafFilters>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [TafFilters],
            providers: [provideNativeDateAdapter()],
        }).compileComponents();

        fixture = TestBed.createComponent(TafFilters);
        await fixture.whenStable();
    });

    it('creates the TAF date filters', () => {
        expect(fixture.componentInstance).toBeTruthy();
    });
});
