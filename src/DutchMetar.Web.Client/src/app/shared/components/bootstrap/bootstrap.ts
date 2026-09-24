import { AsyncPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { LoadingStatus } from '../../types/status';
import { AirportService } from '../../services/airport-service';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-bootstrap',
    imports: [AsyncPipe, MatProgressBarModule],
    templateUrl: './bootstrap.html',
    styleUrl: './bootstrap.scss',
})
export class Bootstrap implements OnInit {
    private readonly airportService = inject(AirportService);

    protected get loadingStatus$(): Observable<LoadingStatus> {
        return this.airportService.loadingStatus$;
    }

    public ngOnInit(): void {
        this.airportService.initializeAirports();
    }
}
