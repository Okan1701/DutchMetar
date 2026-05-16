import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Airport } from './airport';

describe('Airport', () => {
    let component: Airport;
    let fixture: ComponentFixture<Airport>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [Airport],
        }).compileComponents();

        fixture = TestBed.createComponent(Airport);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
