import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TafService } from './taf-service';
import { TafHistory } from '../models/taf/taf-history';

describe('TafService', () => {
    let service: TafService;
    let httpTestingController: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [provideHttpClient(), provideHttpClientTesting()],
        });

        service = TestBed.inject(TafService);
        httpTestingController = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpTestingController.verify());

    it('requests filtered, paginated TAF history and publishes the result', () => {
        const response: TafHistory = {
            icao: 'EHAM',
            currentPage: 2,
            maxPages: 3,
            totalItems: 101,
            tafReports: [{ tafId: 1, rawTaf: 'TAF EHAM', issuedAt: null }],
        };
        let result: TafHistory | undefined;
        let status: string | undefined;
        service.tafHistory$.subscribe((history) => (result = history));
        service.status$.subscribe((currentStatus) => (status = currentStatus));

        service.getTafHistory({
            icao: 'EHAM',
            page: 2,
            startDate: '2026-10-01',
            endDate: '2026-10-04',
        });

        const request = httpTestingController.expectOne(
            '/api/taf/EHAM?page=2&startDate=2026-10-01&endDate=2026-10-04',
        );
        expect(request.request.method).toBe('GET');
        request.flush(response);

        expect(result).toEqual(response);
        expect(status).toBe('success');
    });
});
