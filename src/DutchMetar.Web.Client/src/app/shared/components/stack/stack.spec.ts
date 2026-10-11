import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Stack } from './stack';
import { By } from '@angular/platform-browser';

@Component({
    imports: [Stack],
    template: '<app-stack>Test Child</app-stack>',
})
class StackHost {}

describe('Stack', () => {
    let component: Stack;
    let fixture: ComponentFixture<StackHost>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [StackHost],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(StackHost);
        component = fixture.debugElement.query(By.directive(Stack)).componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have default direction as vertical', () => {
        expect(component.direction).toBe('vertical');
    });

    it('should have default wrap as false', () => {
        expect(component.wrap).toBe(false);
    });

    it('should set direction to horizontal when input is set', () => {
        component.direction = 'horizontal';
        fixture.detectChanges();
        expect(component.direction).toBe('horizontal');
    });

    it('should set wrap to true when input is set', () => {
        component.wrap = true;
        fixture.detectChanges();
        expect(component.wrap).toBe(true);
    });

    it('should apply horizontal styles when direction is horizontal', () => {
        component.direction = 'horizontal';
        fixture.detectChanges();
        expect(component.direction).toBe('horizontal');
        expect(component.wrap).toBe(false);
    });

    it('should apply vertical styles when direction is vertical', () => {
        component.direction = 'vertical';
        fixture.detectChanges();
        expect(component.direction).toBe('vertical');
        expect(component.wrap).toBe(false);
    });

    it('should apply wrap styles when wrap is true and direction is horizontal', () => {
        component.direction = 'horizontal';
        component.wrap = true;
        fixture.detectChanges();
        expect(component.direction).toBe('horizontal');
        expect(component.wrap).toBe(true);
    });

    it('should render children', () => {
        expect(fixture.nativeElement.textContent).toContain('Test Child');
    });
});
