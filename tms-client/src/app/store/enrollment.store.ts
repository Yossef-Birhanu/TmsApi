// Import Angular utilities.
import { computed, inject } from '@angular/core';

// Import NgRx Signal Store features.
import {
  signalStore,
  withComputed,
  withMethods,
  patchState,
  withState,
} from '@ngrx/signals';

// Import entity-management utilities.
import {
  withEntities,
  setAllEntities,
  updateEntity,
} from '@ngrx/signals/entities';

// rxMethod allows store methods to work with RxJS Observables.
import { rxMethod } from '@ngrx/signals/rxjs-interop';

// Import RxJS operators and utilities.
import {
  pipe,
  concatMap,
  tap,
  catchError,
  EMPTY,
} from 'rxjs';

// Import the service responsible for communicating with the API.
import { EnrollmentService } from '../services/enrollment.service';

// Import the Enrollment model/interface.
import {
  Enrollment,
  EnrollmentStatus,
} from '../models/enrollment.model';


// ==========================================================
// ENROLLMENT SIGNAL STORE
// ==========================================================

export const EnrollmentStore = signalStore(

  // Makes the store available throughout the application.
  { providedIn: 'root' },


  // ========================================================
  // BASIC STATE
  // ========================================================

  withState({

    // Indicates whether enrollment data is loading.
    isLoading: false,

    // Stores an error message if an API operation fails.
    error: null as string | null,

  }),


  // ========================================================
  // ENTITY COLLECTION
  // ========================================================

  // Stores Enrollment entities.
  withEntities<Enrollment>(),


  // ========================================================
  // COMPUTED STATE
  // ========================================================

  withComputed((store) => ({

    // Counts enrollments whose status is Pending.
    pendingCount: computed(() =>
      store.entities().filter(
        enrollment =>
          enrollment.status === EnrollmentStatus.Pending
      ).length
    ),

  })),


  // ========================================================
  // STORE METHODS
  // ========================================================

  withMethods(
    (
      store,
      api = inject(EnrollmentService)
    ) => ({


      // ======================================================
      // LOAD ENROLLMENTS
      // ======================================================
      //
      // Gets enrollment records from the API and puts them
      // into the NgRx entity collection.
      //

      loadEnrollments: rxMethod<void>(
        pipe(

          // --------------------------------------------------
          // STEP 1: Start loading
          // --------------------------------------------------

          tap(() =>
            patchState(store, {
              isLoading: true,
              error: null,
            })
          ),


          // --------------------------------------------------
          // STEP 2: Call API
          // --------------------------------------------------

          concatMap(() =>
            api.getAll().pipe(

              // ------------------------------------------------
              // STEP 3: API successfully returned data
              // ------------------------------------------------

              tap((rows) => {

                console.log(
                  'Enrollment data from API:',
                  rows
                );

                patchState(
                  store,

                  // Replace all existing entities with
                  // the API response.
                  setAllEntities(rows),

                  // Loading completed.
                  {
                    isLoading: false,
                  }
                );
              }),


              // ------------------------------------------------
              // STEP 4: Handle API errors
              // ------------------------------------------------

              catchError((err) => {

                console.error(
                  'Error loading enrollments:',
                  err
                );

                patchState(store, {
                  isLoading: false,
                  error:
                    err?.message ??
                    'Failed to load enrollments.',
                });

                return EMPTY;
              })

            )
          )

        )
      ),


      // ======================================================
      // APPROVE ENROLLMENT
      // ======================================================
      //
      // Approves an enrollment.
      //

      approveEnrollment: rxMethod<number>(
        pipe(

          // --------------------------------------------------
          // STEP 1: Optimistically update the UI
          // --------------------------------------------------

          tap((id) => {

            const enrollmentId = Number(id);

            patchState(
              store,

              updateEntity({
                id: enrollmentId,

                changes: {
                  status: EnrollmentStatus.Approved,
                },

              })
            );

          }),


          // --------------------------------------------------
          // STEP 2: Send approval request to API
          // --------------------------------------------------

          concatMap((id) => {

            const enrollmentId = Number(id);

            return api
              .approve(enrollmentId)
              .pipe(

                // ----------------------------------------------
                // STEP 3: Handle server error
                // ----------------------------------------------

                catchError((err) => {

                  console.error(
                    'Approval failed:',
                    err
                  );


                  // Roll back the optimistic update.
                  patchState(
                    store,

                    updateEntity({
                      id: enrollmentId,

                      changes: {
                        status:
                          EnrollmentStatus.Pending,
                      },

                    })
                  );


                  // Display error.
                  patchState(store, {
                    error:
                      'Server rejected the approval. Check enrollment constraints.',
                  });


                  return EMPTY;
                })

              );

          })

        )
      ),

    })
  )

);