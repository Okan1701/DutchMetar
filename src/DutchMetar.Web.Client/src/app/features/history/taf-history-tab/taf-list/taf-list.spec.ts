import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TafList } from './taf-list';

describe('TafList', () => {
    let fixture: ComponentFixture<TafList>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [TafList],
        }).compileComponents();

        fixture = TestBed.createComponent(TafList);
        fixture.componentRef.setInput('status', 'success');
        fixture.componentRef.setInput('tafHistory', {
            icao: 'EHAM',
            currentPage: 0,
            maxPages: 1,
            totalItems: 2,
            tafReports: [
                { tafId: 1, rawTaf: 'TAF EHAM', issuedAt: null },
                { tafId: 2, rawTaf: 'TAF EHAM issued', issuedAt: '2026-10-06T12:34:00Z' },
            ],
        });
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('shows TAF history, including a fallback for a missing issue time', () => {
        expect(fixture.nativeElement.textContent).toContain('TAF EHAM');
        expect(fixture.nativeElement.textContent).toContain('Unknown');
        expect(fixture.nativeElement.textContent).toContain('2026-10-06 12:34z');
    });
});
