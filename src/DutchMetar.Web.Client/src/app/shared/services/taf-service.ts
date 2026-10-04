import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
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

    private readonly tafEndpoint = '/api/taf';

    public get tafHistory$(): Observable<TafHistory> {
        return this.tafHistorySubject.asObservable();
    }

    public get status$(): Observable<LoadingStatus> {
        return this.statusSubject.asObservable();
    }

    public getTafHistory(request: TafHistoryRequest): void {
        this.statusSubject.next('loading');
        let url = this.tafEndpoint + `/${request.icao}?page=${request.page}`;

        if (request.startDate) {
            url += `&startDate=${request.startDate}`;
        }

        if (request.endDate) {
            url += `&endDate=${request.endDate}`;
        }

        this.httpClient.get<TafHistory>(url).subscribe({
            next: (data) => {
                this.tafHistorySubject.next(data);
                this.statusSubject.next('success');
            },
            error: () => this.statusSubject.next('error'),
        });
    }
}
