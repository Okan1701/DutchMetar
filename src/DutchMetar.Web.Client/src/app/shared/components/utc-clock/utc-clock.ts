import { Component, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

@Component({
    selector: 'app-utc-clock',
    imports: [DatePipe],
    templateUrl: './utc-clock.html',
})
export class UtcClock {
    protected now = signal(new Date());

    constructor() {
        this.scheduleNextMinute();
    }

    private scheduleNextMinute(): void {
        const now = new Date();

        // Milliseconds until the next minute starts
        const delay = 60_000 - (now.getSeconds() * 1_000 + now.getMilliseconds());

        setTimeout(() => {
            this.now.set(new Date());
            this.scheduleNextMinute();
        }, delay);
    }
}
