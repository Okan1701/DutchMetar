import { Component, Input } from '@angular/core';

type StackDirection = 'vertical' | 'horizontal';

@Component({
    selector: 'app-stack',
    templateUrl: './stack.html',
    styleUrls: ['./stack.scss'],
})
export class Stack {
    @Input() direction: StackDirection = 'vertical';
    @Input() wrap: boolean = false;
}
