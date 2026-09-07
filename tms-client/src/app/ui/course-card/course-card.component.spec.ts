// import { ComponentFixture, TestBed } from '@angular/core/testing';

// import { CourseCardComponent } from './course-card.component';

// describe('CourseCardComponent', () => {
//   let component: CourseCardComponent;
//   let fixture: ComponentFixture<CourseCardComponent>;

//   beforeEach(async () => {
//     await TestBed.configureTestingModule({
//       imports: [CourseCardComponent],
//     }).compileComponents();

//     fixture = TestBed.createComponent(CourseCardComponent);
//     component = fixture.componentInstance;
//     await fixture.whenStable();
//   });

//   it('should create', () => {
//     expect(component).toBeTruthy();
//   });
// });


import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { CourseCardComponent } from './course-card.component';

describe('CourseCardComponent', () => {
  let fixture: ComponentFixture<CourseCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CourseCardComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(CourseCardComponent);
  });

  it('should display the course title', async () => {
    fixture.componentRef.setInput('course', {
      id: 1,
      code: 'CSE-101',
      title: 'Advanced Web Dev',
      maxCapacity: 30,
      enrollmentCount: 12,
    });

    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;

    expect(el.textContent).toContain('Advanced Web Dev');
  });

  it('should emit enrollClicked event when button is clicked', async () => {
    fixture.componentRef.setInput('course', {
      id: 1,
      code: 'CSE-101',
      title: 'Advanced Web Dev',
      maxCapacity: 30,
      enrollmentCount: 12,
    });

    await fixture.whenStable();

    const component = fixture.componentInstance;

    let emittedCourse: any = null;

    component.enrollClick.subscribe((course: any) => {
      emittedCourse = course;
    });

    const button = fixture.nativeElement.querySelector(
      'button'
    ) as HTMLButtonElement;

    button.click();

    await fixture.whenStable();

    expect(emittedCourse).toBeTruthy();
    expect(emittedCourse.title).toBe('Advanced Web Dev');
  });
});