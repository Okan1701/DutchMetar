import { Component, input, InputSignal } from '@angular/core';
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
    public status: InputSignal<LoadingStatus> = input<LoadingStatus>('success');
    public loadingText: InputSignal<string> = input<string>('Retrieving data');
    public notFoundText: InputSignal<string> = input<string>(
        'The page you are trying to find does not exist.',
    );
}
