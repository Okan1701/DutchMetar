import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

type StackDirection = 'vertical' | 'horizontal';

@Component({
    selector: 'app-stack',
    templateUrl: './stack.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./stack.scss'],
})
export class Stack {
    @Input() direction: StackDirection = 'vertical';
    @Input() wrap: boolean = false;
}
