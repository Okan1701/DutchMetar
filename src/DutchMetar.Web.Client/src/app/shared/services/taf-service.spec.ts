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

    it('cancels superseded TAF history requests', () => {
        let result: TafHistory | undefined;
        let status: string | undefined;
        service.tafHistory$.subscribe((history) => (result = history));
        service.status$.subscribe((currentStatus) => (status = currentStatus));

        service.getTafHistory({ icao: 'EHAM', page: 0 });
        const firstRequest = httpTestingController.expectOne('/api/taf/EHAM?page=0');

        service.getTafHistory({ icao: 'EHAM', page: 1 });
        expect(firstRequest.cancelled).toBe(true);
        expect(status).toBe('loading');

        const latestResponse: TafHistory = {
            icao: 'EHAM',
            currentPage: 1,
            maxPages: 2,
            totalItems: 51,
            tafReports: [{ tafId: 2, rawTaf: 'TAF EHAM latest', issuedAt: null }],
        };
        httpTestingController.expectOne('/api/taf/EHAM?page=1').flush(latestResponse);

        expect(result).toEqual(latestResponse);
        expect(status).toBe('success');
    });
});
