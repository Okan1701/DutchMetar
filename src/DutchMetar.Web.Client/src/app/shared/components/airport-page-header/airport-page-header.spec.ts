import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AirportPageHeader } from './airport-page-header';
import { MeteoCondition } from '../../types/meteo-condition';
import { AirportSummary } from '../../models/airport/airport-summary';
import { AirportCurrentMetar } from '../../models/airport/airport-current-metar';

describe('AirportPageHeader', () => {
    let component: AirportPageHeader;
    let fixture: ComponentFixture<AirportPageHeader>;
    const airport: AirportSummary = {
        icao: 'EHAM',
        name: 'Schiphol',
        lastUpdated: new Date('2026-10-17T12:25:00Z'),
        meteoCondition: MeteoCondition.Imc,
        latestWeather: { isCorrected: true, isAuto: true, isCavok: false } as AirportCurrentMetar,
    };

    function button(): HTMLButtonElement {
        return fixture.nativeElement.querySelector('button[mat-icon-button]');
    }

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportPageHeader],
        }).compileComponents();

        fixture = TestBed.createComponent(AirportPageHeader);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airport', airport);
        await fixture.whenStable();
    });

    it('renders source, title, subtitle and badges', () => {
        const text: string = fixture.nativeElement.textContent;
        expect(text).toContain('Source: KNMI (Last update at 12:25z)');
        expect(text).toContain('EHAM');
        expect(text).toContain('Schiphol');
        expect(text).toContain('COR');
        expect(text).toContain('IMC');
        expect(text).toContain('AUTO');
    });

    it('emits refreshClicked when the refresh button is clicked', () => {
        const emitted = vi.fn();
        component.refreshClicked.subscribe(emitted);

        button().click();

        expect(emitted).toHaveBeenCalledOnce();
    });

    it('disables the refresh button based on the refreshDisabled input', async () => {
        expect(button().disabled).toBe(false);

        fixture.componentRef.setInput('refreshDisabled', true);
        await fixture.whenStable();

        expect(button().disabled).toBe(true);
    });
});
