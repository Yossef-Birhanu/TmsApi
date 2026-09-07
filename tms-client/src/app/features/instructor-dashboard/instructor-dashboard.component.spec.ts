import { vi } from 'vitest';
import { of } from 'rxjs';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InstructorDashboardComponent } from './instructor-dashboard.component';
import { LiveSyncService } from '../../services/live-sync.service';

describe('InstructorDashboardComponent', () => {
  let component: InstructorDashboardComponent;
  let fixture: ComponentFixture<InstructorDashboardComponent>;

  const liveSyncServiceMock = {
  connect: vi.fn(),

  // Must be an Observable because EnrollmentStore
  // uses switchMap(() => sync.events$)
  events$: of(),

  connectionState: {
    asReadonly: vi.fn()
  }
};

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InstructorDashboardComponent],
      providers: [
        {
          provide: LiveSyncService,
          useValue: liveSyncServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InstructorDashboardComponent);
    component = fixture.componentInstance;

    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});