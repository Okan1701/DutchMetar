import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AirportDetails } from '../../shared/models/airport-details';
import { LoadingStatus } from '../../shared/types/status';
import { AirportService } from '../../shared/services/airport-service';
import { StatusDisplay } from '../../shared/components/status-display/status-display';

@Component({
    selector: 'app-airport',
    imports: [StatusDisplay],
    templateUrl: './airport.html',
    styleUrl: './airport.scss',
})
export class Airport implements OnInit {
    protected airportIcao: string | null = null;
    protected airportDetails?: AirportDetails;
    protected loadingStatus: LoadingStatus = 'loading';

    constructor(
        private readonly route: ActivatedRoute,
        private readonly airportService: AirportService,
    ) {}

    public ngOnInit(): void {
        this.airportIcao = this.route.snapshot.paramMap.get('icao');
    }
}
