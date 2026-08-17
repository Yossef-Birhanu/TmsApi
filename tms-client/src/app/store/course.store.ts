import { inject } from '@angular/core';
import {
  signalStore,
  withMethods,
  withState,
  patchState
} from '@ngrx/signals';

import {
  withEntities,
  removeEntity,
  setAllEntities
} from '@ngrx/signals/entities';

import { catchError, EMPTY } from 'rxjs';

import { CourseService } from '../services/course.service';
import { Course } from '../models/course.model';

export const CourseStore = signalStore(
  { providedIn: 'root' },

  withEntities<Course>(),

  withState({
    error: null as string | null
  }),

  withMethods((store, svc = inject(CourseService)) => ({

    deleteCourse(id: number) {

      // 1. Snapshot BEFORE removing the course
      const previousSnapshot = store.entities();

      // 2. Optimistic UI update
      patchState(store, removeEntity(id));

      // 3. Delete from backend
      svc.delete(id).pipe(

        catchError(err => {

          // 4. Rollback
          patchState(
            store,
            setAllEntities(previousSnapshot)
          );

          // 5. Store error message
          patchState(store, {
            error:
              'Cannot delete course: active student enrollments exist.'
          });

          return EMPTY;
        })

      ).subscribe();
    }

  }))
);