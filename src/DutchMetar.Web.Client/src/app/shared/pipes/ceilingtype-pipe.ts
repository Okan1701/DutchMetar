import { Pipe, PipeTransform } from '@angular/core';
import { CeilingType } from '../enums/ceiling-type';

@Pipe({
    name: 'ceilingtype',
})
export class CeilingtypePipe implements PipeTransform {
    public transform(value: unknown, ...args: unknown[]): unknown {
        if (typeof value !== 'number') return null;
        
        let type = value as CeilingType;
        switch (type) {
            case CeilingType.Few:
                return 'FEW';
            case CeilingType.Scattered:
                return 'SCT';
            case CeilingType.Broken:
                return 'BKN';
            case CeilingType.Overcast:
                return 'OVC';
            default:
                return "N/A"
        }
    }
}
