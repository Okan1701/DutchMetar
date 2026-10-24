import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { BehaviorSubject, Subject } from 'rxjs';
import { AirportNavList } from './airport-nav-list';
import { AirportService } from '../../services/airport-service';
import { AirportSummary } from '../../models/airport/airport-summary';
import { MeteoCondition } from '../../types/meteo-condition';

describe('AirportNavList', () => {
    let fixture: ComponentFixture<AirportNavList>;
    let airports: BehaviorSubject<AirportSummary[]>;
    let currentAirport: Subject<AirportSummary>;
    const schiphol: AirportSummary = {
        icao: 'EHAM',
        meteoCondition: MeteoCondition.Vmc,
        lastUpdated: new Date(),
    };
    const rotterdam: AirportSummary = { ...schiphol, icao: 'EHRD' };

    function rows(): HTMLElement[] {
        return Array.from(fixture.nativeElement.querySelectorAll('.airport-item'));
    }

    function activeIcaos(): string[] {
        return rows()
            .filter((row) => row.classList.contains('active'))
            .map((row) => row.querySelector('.title')!.textContent!.trim());
    }

    function search(value: string): void {
        const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
        input.value = value;
        input.dispatchEvent(new Event('input'));
        fixture.detectChanges();
    }

    beforeEach(async () => {
        airports = new BehaviorSubject([schiphol, rotterdam]);
        currentAirport = new Subject<AirportSummary>();
        const service: Pick<AirportService, 'airports$' | 'currentAirport$'> = {
            airports$: airports.asObservable(),
            currentAirport$: currentAirport.asObservable(),
        };
        await TestBed.configureTestingModule({
            imports: [AirportNavList],
            providers: [provideRouter([]), { provide: AirportService, useValue: service }],
        }).compileComponents();
        fixture = TestBed.createComponent(AirportNavList);
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('renders the airport collection with no active item before selection emits', () => {
        expect(rows()).toHaveLength(2);
        expect(activeIcaos()).toEqual([]);
    });

    it('updates the active item from the service without changing the route', () => {
        currentAirport.next(schiphol);
        fixture.detectChanges();
        expect(activeIcaos()).toEqual(['EHAM']);

        currentAirport.next(rotterdam);
        fixture.detectChanges();
        expect(activeIcaos()).toEqual(['EHRD']);
        expect(TestBed.inject(Router).url).toBe('/');
    });

    it('uses service selection even when the route points to another airport', async () => {
        TestBed.inject(Router).resetConfig([
            { path: 'airport/:icao', children: [] },
            { path: 'history/:icao', children: [] },
        ]);
        await TestBed.inject(Router).navigateByUrl('/airport/EHAM');
        currentAirport.next(rotterdam);
        fixture.detectChanges();
        expect(activeIcaos()).toEqual(['EHRD']);

        await TestBed.inject(Router).navigateByUrl('/history/EHAM');
        fixture.detectChanges();
        expect(activeIcaos()).toEqual(['EHRD']);
    });

    it('clears active styling for the empty service placeholder or an unknown airport', () => {
        currentAirport.next(schiphol);
        fixture.detectChanges();
        currentAirport.next({ ...schiphol, icao: '' });
        fixture.detectChanges();
        expect(activeIcaos()).toEqual([]);

        currentAirport.next({ ...schiphol, icao: 'EHXX' });
        fixture.detectChanges();
        expect(activeIcaos()).toEqual([]);
    });

    it('matches ICAO case-insensitively and preserves selection across refreshed objects', () => {
        currentAirport.next({ ...schiphol, icao: 'eham' });
        fixture.detectChanges();
        expect(activeIcaos()).toEqual(['EHAM']);

        airports.next([{ ...schiphol }, { ...rotterdam }]);
        fixture.detectChanges();
        expect(activeIcaos()).toEqual(['EHAM']);
    });

    it('filters case-insensitively and restores active styling when the filter is cleared', () => {
        currentAirport.next(schiphol);
        fixture.detectChanges();
        search('ehrd');
        expect(rows()).toHaveLength(1);
        expect(rows()[0].textContent).toContain('EHRD');
        expect(activeIcaos()).toEqual([]);

        search('EHXX');
        expect(rows()).toHaveLength(0);

        search('');
        expect(rows()).toHaveLength(2);
        expect(activeIcaos()).toEqual(['EHAM']);
    });

    it('preserves divider placement', () => {
        airports.next([schiphol, rotterdam, { ...schiphol, icao: 'EHEH' }]);
        fixture.detectChanges();
        expect(fixture.nativeElement.querySelectorAll('mat-divider')).toHaveLength(1);
    });

    it('keeps the latest selection while all rows are filtered out', () => {
        search('EHXX');
        currentAirport.next(rotterdam);
        fixture.detectChanges();
        expect(rows()).toHaveLength(0);

        search('');
        expect(activeIcaos()).toEqual(['EHRD']);
    });
});
