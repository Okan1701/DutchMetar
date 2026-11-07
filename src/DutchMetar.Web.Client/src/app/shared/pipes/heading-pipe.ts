import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'heading',
})
export class HeadingPipe implements PipeTransform {
    public transform(value: number): string {
        return value.toString().padStart(3, '0');
    }
}
