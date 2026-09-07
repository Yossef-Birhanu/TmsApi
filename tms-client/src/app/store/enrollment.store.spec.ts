import { TestBed } from '@angular/core/testing';

import { EnrollmentStore } from './enrollment.store';

describe('EnrollmentStore', () => {
  let store: any;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        EnrollmentStore
      ],
    });

    store = TestBed.inject(EnrollmentStore);
  });

  it('should create', () => {
    expect(store).toBeTruthy();
  });
});