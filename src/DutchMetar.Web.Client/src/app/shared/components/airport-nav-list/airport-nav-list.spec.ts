import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AirportNavList } from './airport-nav-list';

describe('AirportNavList', () => {
    let component: AirportNavList;
    let fixture: ComponentFixture<AirportNavList>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AirportNavList],
        }).compileComponents();

        fixture = TestBed.createComponent(AirportNavList);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
