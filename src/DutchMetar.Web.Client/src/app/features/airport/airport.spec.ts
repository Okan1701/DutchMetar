import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { AirportService } from '../../shared/services/airport-service';
import { provideNativeDateAdapter } from '@angular/material/core';
import { Airport } from './airport';

describe('Airport', () => {
    let component: Airport;
    let fixture: ComponentFixture<Airport>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Airport],
            providers: [
                provideNativeDateAdapter(),
                { provide: ActivatedRoute, useValue: { params: of({ icao: 'EHAM' }) } },
                { provide: Router, useValue: { navigate: () => Promise.resolve(true) } },
                {
                    provide: AirportService,
                    useValue: {
                        getAirportDetails: () =>
                            of({
                                icao: 'EHAM',
                                meteoCondition: 'None',
                                lastUpdated: new Date(),
                            }),
                    },
                },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(Airport);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
