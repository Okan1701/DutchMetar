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
            totalItems: 1,
            tafReports: [{ tafId: 1, rawTaf: 'TAF EHAM', issuedAt: null }],
        });
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it('shows TAF history, including a fallback for a missing issue time', () => {
        expect(fixture.nativeElement.textContent).toContain('TAF EHAM');
        expect(fixture.nativeElement.textContent).toContain('Unknown');
    });
});
