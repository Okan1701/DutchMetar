import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
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
    
    private readonly metarEndpoint = "/api/metar"
    
    public get metarHistory$(): Observable<MetarHistory> {
        return this.metarHistorySubject.asObservable();
    }
    
    public get status$(): Observable<LoadingStatus> {
        return this.statusSubject.asObservable();
    }
    
    public getMetarHistory(request: MetarHistoryRequest): void {
        this.statusSubject.next('loading')
        let url = this.metarEndpoint + `/${request.icao}?page=${request.page}`;
        
        if (request.startDate) {
            url += `&startDate=${request.startDate}`;
        }

        if (request.endDate) {
            url += `&endDate=${request.endDate}`;
        }
        
        this.httpClient.get<MetarHistory>(url).subscribe({
            next: (data) => {
                this.metarHistorySubject.next(data);
                this.statusSubject.next('success');
            },
            error: () => this.statusSubject.next('error'),
        });
    }
}