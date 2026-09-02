import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Metar } from './metar';

describe('Metar', () => {
    let component: Metar;
    let fixture: ComponentFixture<Metar>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Metar],
        }).compileComponents();

        fixture = TestBed.createComponent(Metar);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
