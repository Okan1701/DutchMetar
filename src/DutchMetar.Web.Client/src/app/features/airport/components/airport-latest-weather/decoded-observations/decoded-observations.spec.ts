import { ComponentFixture, TestBed } from '@angular/core/testing';

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
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
