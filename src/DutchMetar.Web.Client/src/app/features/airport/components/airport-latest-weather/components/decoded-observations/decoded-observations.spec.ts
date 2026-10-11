import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MeteoCondition } from '../../../../../../shared/types/meteo-condition';
import { DecodedObservations } from './decoded-observations';

describe('DecodedObservations', () => {
    let component: DecodedObservations;
    let fixture: ComponentFixture<DecodedObservations>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [DecodedObservations],
        }).compileComponents();

        fixture = TestBed.createComponent(DecodedObservations);
        component = fixture.componentInstance;
        fixture.componentRef.setInput('airportDetails', {
            icao: 'EHAM',
            meteoCondition: MeteoCondition.None,
            lastUpdated: new Date(),
        });
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
