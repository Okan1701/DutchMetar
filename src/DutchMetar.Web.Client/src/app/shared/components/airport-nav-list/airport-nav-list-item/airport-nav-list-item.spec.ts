import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter, Router, RouterLink } from '@angular/router';
import { AirportNavListItem } from './airport-nav-list-item';
import { AirportSummary } from '../../../models/airport/airport-summary';
import { MeteoCondition } from '../../../types/meteo-condition';
import { Badge } from '../../badge/badge';

describe('AirportNavListItem', () => {
    let fixture: ComponentFixture<AirportNavListItem>;
    const airport: AirportSummary = {
        icao: 'EHAM',
        meteoCondition: MeteoCondition.Vmc,
        lastUpdated: new Date(),
        latestWeather: {
            rawMetar: '',
            issuedAt: '2026-10-24T09:15:00Z',
            windDirection: 240,
            windSpeedKnots: 12,
            isAuto: true,
            isCavok: false,
            isCorrected: true,
            noCloudsDetected: false,
            ceilings: [],
        },
    };

    function badges(): { text: string; type: string }[] {
        return fixture.debugElement.queryAll(By.directive(Badge)).map((element) => ({
            text: element.nativeElement.textContent.trim(),
            type: (element.componentInstance as Badge).type(),
        }));
    }

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportNavListItem],
            providers: [provideRouter([{ path: 'airport/:icao', children: [] }])],
        }).compileComponents();
        fixture = TestBed.createComponent(AirportNavListItem);
        fixture.componentRef.setInput('airport', airport);
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('toggles the original active class from its input', () => {
        const row: HTMLElement = fixture.nativeElement.querySelector('.airport-item');
        expect(row.classList.contains('active')).toBe(false);
        fixture.componentRef.setInput('active', true);
        fixture.detectChanges();
        expect(row.classList.contains('active')).toBe(true);
        fixture.componentRef.setInput('active', false);
        fixture.detectChanges();
        expect(row.classList.contains('active')).toBe(false);
    });

    it('retains the airport link and navigates when clicked', async () => {
        const link = fixture.debugElement.query(By.directive(RouterLink));
        const router = TestBed.inject(Router);
        expect(router.serializeUrl(link.injector.get(RouterLink).urlTree!)).toBe('/airport/EHAM');
        link.nativeElement.click();
        await fixture.whenStable();
        expect(router.url).toBe('/airport/EHAM');
    });

    it('renders the ICAO, UTC issue time, wind, and corrected and automatic badges', () => {
        const text = fixture.nativeElement.textContent;
        expect(text).toContain('EHAM');
        expect(text).toContain('Issued at 09:15Z');
        expect(text).toContain('Wind 240° at 12kt');
        expect(badges()).toEqual([
            { text: 'COR', type: 'primary' },
            { text: 'VMC', type: 'success' },
            { text: 'AUTO', type: 'neutral' },
        ]);
    });

    it.each([
        { condition: MeteoCondition.Vmc, css: 'vmc', text: 'VMC', type: 'success' },
        { condition: MeteoCondition.Imc, css: 'imc', text: 'IMC', type: 'danger' },
        { condition: MeteoCondition.Marginal, css: 'marginal', text: 'MARGINAL', type: 'warning' },
    ])('renders the $text indicator and badge', ({ condition, css, text, type }) => {
        fixture.componentRef.setInput('airport', {
            ...airport,
            meteoCondition: condition,
            latestWeather: undefined,
        });
        fixture.detectChanges();
        const icon: HTMLElement = fixture.nativeElement.querySelector('mat-icon');
        expect(icon.classList.contains(css)).toBe(true);
        for (const other of ['vmc', 'imc', 'marginal'].filter((value) => value !== css)) {
            expect(icon.classList.contains(other)).toBe(false);
        }
        expect(badges()).toEqual([{ text, type }]);
    });

    it('gives CAVOK precedence over the meteorological condition badge', () => {
        fixture.componentRef.setInput('airport', {
            ...airport,
            meteoCondition: MeteoCondition.Imc,
            latestWeather: { ...airport.latestWeather, isCavok: true },
        });
        fixture.detectChanges();
        expect(badges()).toEqual([
            { text: 'COR', type: 'primary' },
            { text: 'CAVOK', type: 'success' },
            { text: 'AUTO', type: 'neutral' },
        ]);
        expect(fixture.nativeElement.querySelector('mat-icon').classList.contains('imc')).toBe(
            true,
        );
    });

    it('renders an airport without optional weather data or a meteorological condition', () => {
        fixture.componentRef.setInput('airport', {
            ...airport,
            meteoCondition: MeteoCondition.None,
            latestWeather: undefined,
        });
        fixture.detectChanges();
        expect(fixture.nativeElement.textContent).toContain('EHAM');
        expect(fixture.nativeElement.textContent).not.toContain('Issued at');
        expect(fixture.nativeElement.textContent).not.toContain('Wind');
        expect(badges()).toEqual([]);
        const icon: HTMLElement = fixture.nativeElement.querySelector('mat-icon');
        expect(['vmc', 'imc', 'marginal'].some((css) => icon.classList.contains(css))).toBe(false);
    });
});
