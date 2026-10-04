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
        fixture.componentRef.setInput('status', 'success');
        fixture.componentRef.setInput('metarHistory', {
            icao: 'EHAM',
            currentPage: 0,
            maxPages: 0,
            totalItems: 0,
            metarReports: [],
        });
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
