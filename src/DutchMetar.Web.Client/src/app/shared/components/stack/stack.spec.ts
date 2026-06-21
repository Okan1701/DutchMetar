import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Stack } from './stack';
import { By } from '@angular/platform-browser';

describe('Stack', () => {
    let component: Stack;
    let fixture: ComponentFixture<Stack>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            declarations: [Stack],
        }).compileComponents();
    });

    beforeEach(() => {
        fixture = TestBed.createComponent(Stack);
        component = fixture.componentInstance;
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
        const element = fixture.debugElement.nativeElement;
        expect(element.style.flexDirection).toBe('row');
        expect(element.style.flexWrap).toBe('wrap');
    });

    it('should apply vertical styles when direction is vertical', () => {
        component.direction = 'vertical';
        fixture.detectChanges();
        const element = fixture.debugElement.nativeElement;
        expect(element.style.flexDirection).toBe('column');
        expect(element.style.flexWrap).toBe('nowrap');
    });

    it('should apply wrap styles when wrap is true and direction is horizontal', () => {
        component.direction = 'horizontal';
        component.wrap = true;
        fixture.detectChanges();
        const element = fixture.debugElement.nativeElement;
        expect(element.style.flexWrap).toBe('wrap');
    });

    it('should render children', () => {
        const childText = 'Test Child';
        fixture.componentInstance.direction = 'vertical';
        fixture.debugElement.query(By.css('ng-content')).nativeElement.textContent = childText;
        fixture.detectChanges();
        const compiled = fixture.nativeElement;
        expect(compiled.textContent).toContain(childText);
    });
});
