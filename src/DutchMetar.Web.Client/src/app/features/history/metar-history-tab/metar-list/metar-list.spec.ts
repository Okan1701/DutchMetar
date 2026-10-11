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
            totalItems: 1,
            metarReports: [
                {
                    metarId: 1,
                    rawMetar: 'METAR EHAM',
                    issuedAt: '2026-10-06T12:34:00Z',
                },
            ],
        });
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('displays issue timestamps in UTC', () => {
        expect(fixture.nativeElement.textContent).toContain('2026-10-06 12:34z');
    });
});
