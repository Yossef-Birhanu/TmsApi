// ==========================================================
// ANGULAR
// ==========================================================

import {
  computed,
  inject
} from '@angular/core';


// ==========================================================
// NGRX SIGNAL STORE
// ==========================================================

import {
  signalStore,
  withComputed,
  withMethods,
  patchState,
  withState
} from '@ngrx/signals';


// ==========================================================
// NGRX ENTITY MANAGEMENT
// ==========================================================

import {
  withEntities,
  setAllEntities,
  updateEntity
} from '@ngrx/signals/entities';


// ==========================================================
// RXJS SIGNAL STORE
// ==========================================================

import {
  rxMethod
} from '@ngrx/signals/rxjs-interop';


// ==========================================================
// RXJS
// ==========================================================

import {
  pipe,
  concatMap,
  switchMap,
  tap,
  catchError,
  EMPTY
} from 'rxjs';


// ==========================================================
// SERVICES
// ==========================================================

import {
  EnrollmentService
} from '../services/enrollment.service';

import {
  LiveSyncService
} from '../services/live-sync.service';


// ==========================================================
// MODELS
// ==========================================================

import {
  Enrollment,
  EnrollmentStatus
} from '../models/enrollment.model';


// ==========================================================
// ENROLLMENT SIGNAL STORE
// ==========================================================

export const EnrollmentStore = signalStore(

  // ========================================================
  // PROVIDED IN ROOT
  // ========================================================

  {
    providedIn: 'root'
  },


  // ========================================================
  // BASIC STATE
  // ========================================================

  withState({

    // Indicates whether enrollment data is loading.
    isLoading: false,

    // Stores API error message.
    error: null as string | null

  }),


  // ========================================================
  // ENTITY COLLECTION
  // ========================================================

  withEntities<Enrollment>(),


  // ========================================================
  // COMPUTED STATE
  // ========================================================

  withComputed((store) => ({

    // Count Pending enrollments.
    pendingCount: computed(() =>

      store.entities().filter(
        enrollment =>
          enrollment.status ===
          EnrollmentStatus.Pending
      ).length

    )

  })),


  // ========================================================
  // STORE METHODS
  // ========================================================

  withMethods(

    (
      store,

      // API service
      api = inject(EnrollmentService),

      // SignalR service
      sync = inject(LiveSyncService)

    ) => ({


      // ======================================================
      // LIVE SIGNALR UPDATES
      // ======================================================
      //
      // Starts SignalR and listens for enrollment status
      // changes coming from the .NET backend.
      //

      listenForLiveUpdates: rxMethod<void>(

        pipe(

          // --------------------------------------------------
          // STEP 1
          // Start SignalR connection.
          // --------------------------------------------------

          tap(() => {

            console.log(
              'Starting live enrollment sync...'
            );

            sync.connect();

          }),


          // --------------------------------------------------
          // STEP 2
          // Listen to SignalR event stream.
          // --------------------------------------------------

          switchMap(() =>

            sync.events$

          ),


          // --------------------------------------------------
          // STEP 3
          // Update SignalStore.
          // --------------------------------------------------

          tap((event) => {

            console.log(
              'Updating enrollment from SignalR:',
              event.id,
              event.status
            );


            patchState(

              store,

              updateEntity({

                // Enrollment ID must be number.
                id: event.id,

                changes: {

                  // Convert string status from SignalR
                  // into the existing EnrollmentStatus enum.
                  status:
                    event.status === 'Pending'
                      ? EnrollmentStatus.Pending
                      : event.status === 'Approved'
                        ? EnrollmentStatus.Approved
                        : EnrollmentStatus.Rejected

                }

              })

            );

          })

        )

      ),


      // ======================================================
      // LOAD ENROLLMENTS
      // ======================================================
      //
      // Gets enrollment records from the API.
      //

      loadEnrollments: rxMethod<void>(

        pipe(

          // --------------------------------------------------
          // STEP 1
          // Start loading.
          // --------------------------------------------------

          tap(() => {

            patchState(

              store,

              {
                isLoading: true,
                error: null
              }

            );

          }),


          // --------------------------------------------------
          // STEP 2
          // Call API.
          // --------------------------------------------------

          concatMap(() =>

            api.getAll().pipe(


              // ------------------------------------------------
              // STEP 3
              // API successfully returned data.
              // ------------------------------------------------

              tap((rows) => {

                console.log(
                  'Enrollment data from API:',
                  rows
                );


                patchState(

                  store,

                  // Replace all entities.
                  setAllEntities(rows),

                  // Loading finished.
                  {
                    isLoading: false
                  }

                );

              }),


              // ------------------------------------------------
              // STEP 4
              // Handle API error.
              // ------------------------------------------------

              catchError((err) => {

                console.error(
                  'Error loading enrollments:',
                  err
                );


                patchState(

                  store,

                  {
                    isLoading: false,

                    error:
                      err?.message ??
                      'Failed to load enrollments.'
                  }

                );


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
          // STEP 1
          // Optimistically update UI.
          // --------------------------------------------------

          tap((id) => {

            const enrollmentId =
              Number(id);


            console.log(
              'Optimistically approving:',
              enrollmentId
            );


            patchState(

              store,

              updateEntity({

                id: enrollmentId,

                changes: {

                  status:
                    EnrollmentStatus.Approved

                }

              })

            );

          }),


          // --------------------------------------------------
          // STEP 2
          // Send approval request to API.
          // --------------------------------------------------

          concatMap((id) => {

            const enrollmentId =
              Number(id);


            return api
              .approve(enrollmentId)

              .pipe(


                // ----------------------------------------------
                // STEP 3
                // Handle server error.
                // ----------------------------------------------

                catchError((err) => {

                  console.error(
                    'Approval failed:',
                    err
                  );


                  // --------------------------------------------
                  // Roll back optimistic update.
                  // --------------------------------------------

                  patchState(

                    store,

                    updateEntity({

                      id: enrollmentId,

                      changes: {

                        status:
                          EnrollmentStatus.Pending

                      }

                    })

                  );


                  // --------------------------------------------
                  // Show error.
                  // --------------------------------------------

                  patchState(

                    store,

                    {

                      error:
                        'Server rejected the approval. Check enrollment constraints.'

                    }

                  );


                  return EMPTY;

                })

              );

          })

        )

      )

    })

  )

);