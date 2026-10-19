import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { History } from './history';
import { AirportService } from '../../shared/services/airport-service';
import { AirportPageHeader } from '../../shared/components/airport-page-header/airport-page-header';

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
        expect(fixture.nativeElement.querySelector('app-airport-page-header')).not.toBeNull();
    });

    it('reloads airports and the active tab history when refresh is clicked', () => {
        const http = TestBed.inject(HttpTestingController);
        const airportService = TestBed.inject(AirportService);
        const initializeAirports = vi.spyOn(airportService, 'initializeAirports');
        http.match((req) => req.url.startsWith('/api/metar/EHAM')).forEach((req) => req.flush({}));

        const header = fixture.debugElement.query(By.directive(AirportPageHeader)).componentInstance as AirportPageHeader;
        header.refreshClicked.emit();
        fixture.detectChanges();

        expect(initializeAirports).toHaveBeenCalledOnce();
        expect(http.match((req) => req.url.startsWith('/api/metar/EHAM')).length).toBe(1);
    });
});
