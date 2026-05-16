import { Component, Input } from '@angular/core';
import { LoadingStatus } from '../../types/status';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
    selector: 'app-status-display',
    imports: [MatProgressSpinnerModule, MatIconModule],
    templateUrl: './status-display.html',
    styleUrls: ['./status-display.scss'],
})
export class StatusDisplay {
    @Input() status: LoadingStatus = 'success';
}
