import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusDisplay } from './status-display';

describe('StatusDisplay', () => {
    let component: StatusDisplay;
    let fixture: ComponentFixture<StatusDisplay>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [StatusDisplay],
        }).compileComponents();

        fixture = TestBed.createComponent(StatusDisplay);
        component = fixture.componentInstance;
        await fixture.whenStable();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
