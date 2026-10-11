import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, EMPTY, Observable, Subject, switchMap, tap } from 'rxjs';
import { MetarHistory } from '../models/metar/metar-history';
import { LoadingStatus } from '../types/status';
import { MetarHistoryRequest } from '../models/metar/metar-history-request';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root',
})
export class MetarService {
    private readonly httpClient = inject(HttpClient);
    private readonly statusSubject = new BehaviorSubject<LoadingStatus>('loading');
    private readonly metarHistorySubject = new BehaviorSubject<MetarHistory>({
        icao: '',
        currentPage: 0,
        maxPages: 0,
        metarReports: [],
        totalItems: 0,
        airportName: '',
    });
    private readonly historyRequests = new Subject<MetarHistoryRequest>();
    private readonly metarEndpoint = '/api/metar';

    constructor() {
        this.historyRequests
            .pipe(
                switchMap((request) => {
                    this.statusSubject.next('loading');
                    return this.httpClient.get<MetarHistory>(this.buildUrl(request)).pipe(
                        tap((data) => {
                            this.metarHistorySubject.next(data);
                            this.statusSubject.next('success');
                        }),
                        catchError(() => {
                            this.statusSubject.next('error');
                            return EMPTY;
                        }),
                    );
                }),
            )
            .subscribe();
    }

    public get metarHistory$(): Observable<MetarHistory> {
        return this.metarHistorySubject.asObservable();
    }

    public get status$(): Observable<LoadingStatus> {
        return this.statusSubject.asObservable();
    }

    public getMetarHistory(request: MetarHistoryRequest): void {
        this.historyRequests.next(request);
    }

    private buildUrl(request: MetarHistoryRequest): string {
        let url = this.metarEndpoint + `/${request.icao}?page=${request.page}`;

        if (request.startDate) {
            url += `&startDate=${request.startDate}`;
        }

        if (request.endDate) {
            url += `&endDate=${request.endDate}`;
        }

        return url;
    }
}