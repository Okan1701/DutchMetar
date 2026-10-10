import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, EMPTY, Observable, Subject, switchMap, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { LoadingStatus } from '../types/status';
import { TafHistory } from '../models/taf/taf-history';
import { TafHistoryRequest } from '../models/taf/taf-history-request';

@Injectable({
    providedIn: 'root',
})
export class TafService {
    private readonly httpClient = inject(HttpClient);
    private readonly statusSubject = new BehaviorSubject<LoadingStatus>('loading');
    private readonly tafHistorySubject = new BehaviorSubject<TafHistory>({
        icao: '',
        currentPage: 0,
        maxPages: 0,
        tafReports: [],
        totalItems: 0,
        airportName: '',
    });
    private readonly historyRequests = new Subject<TafHistoryRequest>();

    private readonly tafEndpoint = '/api/taf';

    constructor() {
        this.historyRequests
            .pipe(
                switchMap((request) => {
                    this.statusSubject.next('loading');
                    return this.httpClient.get<TafHistory>(this.buildUrl(request)).pipe(
                        tap((data) => {
                            this.tafHistorySubject.next(data);
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

    public get tafHistory$(): Observable<TafHistory> {
        return this.tafHistorySubject.asObservable();
    }

    public get status$(): Observable<LoadingStatus> {
        return this.statusSubject.asObservable();
    }

    public getTafHistory(request: TafHistoryRequest): void {
        this.historyRequests.next(request);
    }

    private buildUrl(request: TafHistoryRequest): string {
        let url = this.tafEndpoint + `/${request.icao}?page=${request.page}`;

        if (request.startDate) {
            url += `&startDate=${request.startDate}`;
        }

        if (request.endDate) {
            url += `&endDate=${request.endDate}`;
        }

        return url;
    }
}
