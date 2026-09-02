import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MetarList } from './metar-list';

describe('MetarList', () => {
    let component: MetarList;
    let fixture: ComponentFixture<MetarList>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [MetarList],
        }).compileComponents();

        fixture = TestBed.createComponent(MetarList);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
