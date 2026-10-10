import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { of } from 'rxjs';
import { History } from './history';

describe('History', () => {
    let component: History;
    let fixture: ComponentFixture<History>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [History],
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                provideNativeDateAdapter(),
                { provide: ActivatedRoute, useValue: { params: of({ icao: 'EHAM' }) } },
                { provide: Router, useValue: { navigate: () => Promise.resolve(true) } },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(History);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
        expect(fixture.nativeElement.textContent).toContain('METAR');
        expect(fixture.nativeElement.textContent).toContain('TAF');
    });
});
