// import {
//   Component,
//   inject,
//   OnInit
// } from '@angular/core';
// import { DatePipe } from '@angular/common';

// import { EnrollmentStore } from '../../store/enrollment.store';
// import { EnrollmentStatus } from '../../models/enrollment.model';

// @Component({
//   selector: 'app-enrollment-list',
//   standalone: true,
//   imports: [DatePipe],
//   templateUrl: './enrollment-list.component.html',
//   styleUrl: './enrollment-list.component.scss'
// })
// export class EnrollmentListComponent implements OnInit {
//   enrollmentStatus = EnrollmentStatus;

//   // Get the centralized EnrollmentStore.
//   // Every component that injects this store shares
//   // the same enrollment state.
//   store = inject(EnrollmentStore);

//   // Load enrollments when this component starts.
//   ngOnInit(): void {
//     this.store.loadEnrollments();
//   }

//   // Approve an enrollment through the centralized store.
//   onApprove(id: string): void {
//     this.store.approveEnrollment(id);
//   }
// }



import { Component, effect,inject, viewChild,} from "@angular/core";

import { MatTableDataSource, MatTableModule,} from "@angular/material/table";

import {MatPaginator,MatPaginatorModule,} from "@angular/material/paginator";

import {MatSort,MatSortModule,} from "@angular/material/sort";

import { EnrollmentStore } from "../../store/enrollment.store";
import { Enrollment } from "../../models/enrollment.model";

@Component({
  selector: "tms-enrollment-list",
  standalone: true,

  // Angular Material modules required by the table.
  imports: [MatTableModule, MatPaginatorModule,MatSortModule,
  ],

  templateUrl: "./enrollment-list.component.html",
  styleUrl: "./enrollment-list.component.scss",
})
export class EnrollmentListComponent {
auth: any;
deleteCourse(arg0: any) {
throw new Error('Method not implemented.');
}
courses() {
throw new Error('Method not implemented.');
}

  // ============================================================
  // ENROLLMENT STORE
  // ============================================================

  // Inject the EnrollmentStore.
  // The store contains the application's enrollment state.
  store = inject(EnrollmentStore);


  // ============================================================
  // TABLE COLUMNS
  // ============================================================

  // These names must match the matColumnDef values
  // in enrollment-list.component.html.
  displayedColumns = [
    "studentId",
    "courseId",
    "status",
    "actions",
  ];


  // ============================================================
  // MATERIAL TABLE DATA SOURCE
  // ============================================================

  // MatTableDataSource connects our Enrollment array
  // to Angular Material's table, sorting, and pagination.
  dataSource = new MatTableDataSource<Enrollment>();


  // ============================================================
  // SIGNAL-BASED VIEW QUERIES
  // ============================================================

  // Angular 22 signal-based replacement for @ViewChild(MatPaginator).
  readonly paginator = viewChild.required(MatPaginator);

  // Angular 22 signal-based replacement for @ViewChild(MatSort).
  readonly sort = viewChild.required(MatSort);
EnrollmentStatus: any;


  // ============================================================
  // CONSTRUCTOR
  // ============================================================

  constructor() {

    // ----------------------------------------------------------
    // EFFECT 1: STORE → TABLE
    // ----------------------------------------------------------
    //
    // Whenever the enrollment store changes,
    // update the Material table's data source.
    //
    // This means that when an enrollment is approved,
    // loaded, or otherwise changed, the table automatically
    // receives the latest data.
    //
    effect(() => {
      this.dataSource.data = this.store.entities();
    });


    // ----------------------------------------------------------
    // EFFECT 2: TABLE CONTROLS
    // ----------------------------------------------------------
    //
    // Connect the Material paginator and sorting controls
    // to MatTableDataSource.
    //
    // viewChild() returns signals, so this effect runs when
    // Angular resolves these elements in the template.
    //
    effect(() => {
      this.dataSource.paginator = this.paginator();
      this.dataSource.sort = this.sort();
    });


    // ----------------------------------------------------------
    // LOAD ENROLLMENTS
    // ----------------------------------------------------------
    //
    // Load enrollment data when the component is created.
    //
    this.store.loadEnrollments();
  }
}

