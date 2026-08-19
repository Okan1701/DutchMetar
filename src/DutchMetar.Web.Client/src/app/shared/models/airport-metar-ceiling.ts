import { CeilingType } from '../enums/ceiling-type';

export type AirportMetarCeiling = {
    type: CeilingType;
    height: number;
}