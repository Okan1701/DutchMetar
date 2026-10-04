import { Component, computed, effect, output, signal } from '@angular/core';
import { MatCard, MatCardContent } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatDatepicker, MatDatepickerInput, MatDatepickerToggle } from '@angular/material/datepicker';
import { HistoryFilterModel } from '../../history-filter-model';
import { form, FormField, FormRoot } from '@angular/forms/signals';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
    selector: 'app-metar-filters',
    imports: [
        MatCard,
        MatCardContent,
        MatFormFieldModule,
        MatInput,
        MatDatepickerToggle,
        MatDatepicker,
        MatDatepickerInput,
        FormRoot,
        FormField,
        MatIconButton,
        MatIcon,
        MatTooltip,
    ],
    templateUrl: './metar-filters.html',
    styleUrl: './metar-filters.scss',
})
export class MetarFilters {
    public filtersChanged = output<HistoryFilterModel>();

    protected filtersModel = signal<HistoryFilterModel>({
        startDate: null,
        endDate: null,
    });
    protected filtersForm = form(this.filtersModel);
    protected isFiltered = computed(() => {
        let model = this.filtersModel();
        return model.startDate !== null || model.endDate !== null;
    });
    protected readonly maxDate = new Date(Date.now());

    constructor() {
        effect(() => {
            this.filtersChanged.emit(this.filtersModel());
        });
    }

    protected resetFilters(): void {
        this.filtersModel.set({
            startDate: null,
            endDate: null,
        });
    }
}
