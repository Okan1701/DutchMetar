import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MetarService } from './metar-service';
import { MetarHistory } from '../models/metar/metar-history';

describe('MetarService', () => {
    let service: MetarService;
    let httpTestingController: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [provideHttpClient(), provideHttpClientTesting()],
        });

        service = TestBed.inject(MetarService);
        httpTestingController = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpTestingController.verify());

    it('cancels superseded METAR history requests', () => {
        let result: MetarHistory | undefined;
        let status: string | undefined;
        service.metarHistory$.subscribe((history) => (result = history));
        service.status$.subscribe((currentStatus) => (status = currentStatus));

        service.getMetarHistory({ icao: 'EHAM', page: 0 });
        const firstRequest = httpTestingController.expectOne('/api/metar/EHAM?page=0');

        service.getMetarHistory({ icao: 'EHAM', page: 1 });
        expect(firstRequest.cancelled).toBe(true);
        expect(status).toBe('loading');

        const latestResponse: MetarHistory = {
            icao: 'EHAM',
            currentPage: 1,
            maxPages: 2,
            totalItems: 51,
            metarReports: [{ metarId: 2, rawMetar: 'METAR EHAM latest', issuedAt: '2026-10-06T12:00:00Z' }],
        };
        httpTestingController.expectOne('/api/metar/EHAM?page=1').flush(latestResponse);

        expect(result).toEqual(latestResponse);
        expect(status).toBe('success');
    });
});
