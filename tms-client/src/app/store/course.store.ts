import { inject } from '@angular/core';
import { EMPTY } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {patchState, signalStore, withMethods, withState} from '@ngrx/signals';

import {
  withEntities,
  removeEntity,
  setAllEntities
} from '@ngrx/signals/entities';

import { CourseService } from '../services/course.service';
import { Course } from '../models/course.model';

export const CourseStore = signalStore(
  { providedIn: 'root' },

  withEntities<Course>(),
   withState({error: null as string | null }),


  withMethods((store, svc = inject(CourseService)) => ({

    deleteCourse(id: number) {

      // Save current courses
      const previousSnapshot = store.entities();

      // Optimistically remove from UI
      patchState(store, removeEntity(id));

      // Delete from backend
      svc.delete(id).pipe(
        catchError(err => {

          // Roll back if server rejects deletion
          patchState(
            store,
            setAllEntities(previousSnapshot)
          );

          patchState(store, {
            error: 'Cannot delete course: active student enrollments exist.'
          });

          return EMPTY;
        })
      ).subscribe();
    }

  }))
);