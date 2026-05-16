import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AirportDetails } from '../../shared/models/airport-details';
import { LoadingStatus } from '../../shared/types/status';

@Component({
    selector: 'app-airport',
    imports: [],
    templateUrl: './airport.html',
    styleUrl: './airport.scss',
})
export class Airport implements OnInit {
    protected airportIcao: string | null = null;
    protected airportDetails?: AirportDetails;
    protected loadingStatus: LoadingStatus = 'loading';
    
    constructor(private route: ActivatedRoute) {}

    public ngOnInit(): void {
        this.airportIcao = this.route.snapshot.paramMap.get('icao');
    }
}
