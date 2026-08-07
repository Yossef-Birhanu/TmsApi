import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EnrollmentStore } from './enrollment.store';

describe('EnrollmentStore', () => {
  let component: EnrollmentStore;
  let fixture: ComponentFixture<EnrollmentStore>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EnrollmentStore],
    }).compileComponents();

    fixture = TestBed.createComponent(EnrollmentStore);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
