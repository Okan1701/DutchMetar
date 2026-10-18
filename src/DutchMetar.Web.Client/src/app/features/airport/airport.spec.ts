import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { AirportService } from '../../shared/services/airport-service';
import { provideNativeDateAdapter } from '@angular/material/core';
import { Airport } from './airport';
import { By } from '@angular/platform-browser';
import { AirportHistoryData } from './components/airport-history-data/airport-history-data';
import { AirportDayHistory } from '../../shared/models/airport/airport-day-history';
import { MeteoCondition } from '../../shared/types/meteo-condition';
import { StatusDisplay } from '../../shared/components/status-display/status-display';
import { LoadingStatus } from '../../shared/types/status';

describe('Airport', () => {
    let component: Airport;
    let fixture: ComponentFixture<Airport>;
    let params: BehaviorSubject<{ icao: string }>;
    let requests: Subject<AirportDayHistory>[];
    let airportLoadingStatus: BehaviorSubject<LoadingStatus>;
    let service: {
        currentAirport$: AirportService['currentAirport$'];
        loadingStatus$: AirportService['loadingStatus$'];
        setCurrentAirport: ReturnType<typeof vi.fn<AirportService['setCurrentAirport']>>;
        initializeAirports: ReturnType<typeof vi.fn<AirportService['initializeAirports']>>;
        getAirportHistory: ReturnType<typeof vi.fn<AirportService['getAirportHistory']>>;
    };
    const history: AirportDayHistory = {
        icao: 'EHAM',
        isMissingData: false,
        history: [{ dateTime: '2026-10-17T12:00:00Z', temperatureCelsius: 15 }],
    };

    function historyChild(): AirportHistoryData {
        return fixture.debugElement.query(By.directive(AirportHistoryData)).componentInstance;
    }

    function historyStatus(): StatusDisplay {
        return fixture.debugElement.query(By.directive(StatusDisplay)).componentInstance;
    }

    function refresh(): void {
        fixture.debugElement.query(By.css('button[mat-icon-button]')).nativeElement.click();
        fixture.detectChanges();
    }

    function finishRefreshDelay(): void {
        fixture.detectChanges();
        vi.advanceTimersByTime(1000);
        fixture.detectChanges();
    }

    function refreshButton(): HTMLButtonElement {
        return fixture.debugElement.query(By.css('button[mat-icon-button]')).nativeElement;
    }

    beforeEach(async () => {
        params = new BehaviorSubject({ icao: 'EHAM' });
        requests = [];
        airportLoadingStatus = new BehaviorSubject<LoadingStatus>('success');
        service = {
            currentAirport$: of({
                name: 'Schiphol',
                icao: 'EHAM',
                meteoCondition: MeteoCondition.None,
                lastUpdated: new Date(),
            }),
            loadingStatus$: airportLoadingStatus,
            setCurrentAirport: vi.fn(),
            initializeAirports: vi.fn(),
            getAirportHistory: vi.fn(() => {
                const request = new Subject<AirportDayHistory>();
                requests.push(request);
                return request;
            }),
        };
        await TestBed.configureTestingModule({
            imports: [Airport],
            providers: [
                provideNativeDateAdapter(),
                { provide: ActivatedRoute, useValue: { params } },
                { provide: Router, useValue: { navigate: () => Promise.resolve(true) } },
                {
                    provide: AirportService,
                    useValue: service,
                },
            ],
        });
        TestBed.overrideComponent(AirportHistoryData, { set: { template: '' } });
        await TestBed.compileComponents();

        fixture = TestBed.createComponent(Airport);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('loads history once initially using the local calendar day in UTC', () => {
        const today = new Date();
        expect(service.setCurrentAirport).toHaveBeenCalledWith('EHAM');
        expect(service.getAirportHistory).toHaveBeenCalledExactlyOnceWith(
            'EHAM',
            new Date(Date.UTC(today.getFullYear(), today.getMonth(), today.getDate())),
        );
        expect(historyStatus().status()).toBe('loading');
        expect(fixture.nativeElement.textContent).toContain('Retrieving history');
    });

    it('passes retrieved history to the child and displays it inside the status wrapper', () => {
        requests[0].next(history);
        fixture.detectChanges();

        expect(historyChild().airportHistory()).toBe(history);
        expect(historyStatus().status()).toBe('success');
        expect(fixture.nativeElement.querySelector('app-status-display app-airport-history-data')).not.toBeNull();
    });

    it('reloads summaries and history when refresh is clicked', () => {
        requests[0].next(history);
        fixture.detectChanges();
        const initialDate = service.getAirportHistory.mock.calls[0][1];

        finishRefreshDelay();
        refresh();

        expect(service.initializeAirports).toHaveBeenCalledOnce();
        expect(service.getAirportHistory).toHaveBeenCalledTimes(2);
        expect(service.getAirportHistory).toHaveBeenLastCalledWith('EHAM', initialDate);
        expect(historyStatus().status()).toBe('loading');
        expect(fixture.nativeElement.querySelector('app-airport-history-data')).toBeNull();
    });

    it('retrieves child-selected dates and preserves that date on refresh', () => {
        requests[0].next(history);
        fixture.detectChanges();
        const child = historyChild();
        const selectedDate = new Date('2026-10-17T00:00:00Z');
        child.dateSelected.emit(selectedDate);

        expect(service.getAirportHistory).toHaveBeenLastCalledWith('EHAM', selectedDate);
        requests[1].next(history);
        finishRefreshDelay();
        refresh();

        expect(service.getAirportHistory).toHaveBeenCalledTimes(3);
        expect(service.getAirportHistory).toHaveBeenLastCalledWith('EHAM', selectedDate);

        requests[2].next(history);
        fixture.detectChanges();
        expect(historyChild()).toBe(child);
    });

    it('reloads history on airport changes and cancels the previous request', async () => {
        requests[0].next(history);
        fixture.detectChanges();
        const selectedDate = new Date('2026-10-17T00:00:00Z');
        historyChild().dateSelected.emit(selectedDate);
        const previousRequest = requests[1];
        params.next({ icao: 'EHRD' });
        fixture.detectChanges();
        await fixture.whenStable();

        expect(previousRequest.observed).toBe(false);
        expect(service.setCurrentAirport).toHaveBeenLastCalledWith('EHRD');
        expect(service.getAirportHistory).toHaveBeenLastCalledWith('EHRD', selectedDate);
    });

    it('cancels obsolete date requests and ignores their responses', () => {
        requests[0].next(history);
        fixture.detectChanges();
        const previousRequest = requests[0];
        historyChild().dateSelected.emit(new Date('2026-10-17T00:00:00Z'));
        previousRequest.next(history);
        fixture.detectChanges();

        expect(previousRequest.observed).toBe(false);
        expect(historyStatus().status()).toBe('loading');
        expect(fixture.nativeElement.querySelector('app-airport-history-data')).toBeNull();
    });

    it('shows notfound without old history when a response is empty', () => {
        requests[0].next(history);
        fixture.detectChanges();
        finishRefreshDelay();
        refresh();
        requests[1].next({ ...history, history: [] });
        fixture.detectChanges();

        expect(historyStatus().status()).toBe('notfound');
        expect(fixture.nativeElement.textContent).toContain('No historial data found');
        expect(fixture.nativeElement.querySelector('app-airport-history-data')).toBeNull();
    });

    it('reports errors and can recover through refresh and subsequent date changes', () => {
        const logError = vi.spyOn(console, 'error').mockImplementation(() => {});
        try {
            requests[0].error(new Error('History unavailable'));
            fixture.detectChanges();

            expect(historyStatus().status()).toBe('error');
            expect(fixture.nativeElement.textContent).toContain('Something went wrong');
            expect(fixture.nativeElement.querySelector('app-airport-history-data')).toBeNull();
            expect(logError).toHaveBeenCalledWith('Failed to retrieve history', expect.any(Error));

            finishRefreshDelay();
            refresh();
            requests[1].next(history);
            fixture.detectChanges();
            expect(historyStatus().status()).toBe('success');

            historyChild().dateSelected.emit(new Date('2026-10-16T00:00:00Z'));
            requests[2].error(new Error('History unavailable again'));
            historyChild().dateSelected.emit(new Date('2026-10-15T00:00:00Z'));
            requests[3].next(history);
            fixture.detectChanges();
            expect(historyStatus().status()).toBe('success');
        } finally {
            logError.mockRestore();
        }
    });

    it('cancels active requests when destroyed', () => {
        const request = requests[0];
        fixture.destroy();

        expect(request.observed).toBe(false);
    });

    it('stays disabled while either request is loading', () => {
        expect(refreshButton().disabled).toBe(true);
        finishRefreshDelay();
        expect(refreshButton().disabled).toBe(true);

        airportLoadingStatus.next('loading');
        requests[0].next(history);
        finishRefreshDelay();
        expect(refreshButton().disabled).toBe(true);

        airportLoadingStatus.next('success');
        finishRefreshDelay();
        expect(refreshButton().disabled).toBe(false);

        historyChild().dateSelected.emit(new Date('2026-10-17T00:00:00Z'));
        finishRefreshDelay();
        expect(refreshButton().disabled).toBe(true);
    });

    it('waits a full second before enabling refresh and blocks repeated clicks', () => {
        requests[0].next(history);
        fixture.detectChanges();
        vi.advanceTimersByTime(999);
        fixture.detectChanges();
        expect(refreshButton().disabled).toBe(true);
        refresh();
        expect(service.initializeAirports).not.toHaveBeenCalled();

        vi.advanceTimersByTime(1);
        fixture.detectChanges();
        expect(refreshButton().disabled).toBe(false);
        refresh();
        refresh();
        expect(service.initializeAirports).toHaveBeenCalledOnce();

        requests[1].next(history);
        fixture.detectChanges();
        refresh();
        expect(service.initializeAirports).toHaveBeenCalledOnce();
        finishRefreshDelay();
        expect(refreshButton().disabled).toBe(false);
    });

    it('cancels a pending enable delay if loading restarts', () => {
        requests[0].next(history);
        fixture.detectChanges();
        vi.advanceTimersByTime(500);
        airportLoadingStatus.next('loading');
        fixture.detectChanges();
        vi.advanceTimersByTime(500);
        fixture.detectChanges();
        expect(refreshButton().disabled).toBe(true);

        airportLoadingStatus.next('success');
        fixture.detectChanges();
        vi.advanceTimersByTime(999);
        fixture.detectChanges();
        expect(refreshButton().disabled).toBe(true);
        vi.advanceTimersByTime(1);
        fixture.detectChanges();
        expect(refreshButton().disabled).toBe(false);
    });

    it('allows retries after non-loading error and notfound statuses', () => {
        airportLoadingStatus.next('error');
        requests[0].next({ ...history, history: [] });
        finishRefreshDelay();

        expect(refreshButton().disabled).toBe(false);
        refresh();
        expect(service.initializeAirports).toHaveBeenCalledOnce();
    });
});
