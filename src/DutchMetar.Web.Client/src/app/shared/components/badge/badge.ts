import { Component, input } from '@angular/core';

export type BadgeType = 'success' | 'warning' | 'danger' | 'primary' | 'neutral';

@Component({
    selector: 'app-badge',
    imports: [],
    templateUrl: './badge.html',
    styleUrl: './badge.scss',
})
export class Badge {
    public type = input.required<BadgeType>();
}
