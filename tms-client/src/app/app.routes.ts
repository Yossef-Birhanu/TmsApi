import { Routes } from "@angular/router";

export const routes: Routes = [
  {
    // ==========================================================
    // Student Dashboard
    // URL: /student-dashboard
    // ==========================================================
    path: "student-dashboard",

    // Lazy-load the StudentDashboardComponent
    loadComponent: () =>
      import("./features/student-dashboard/student-dashboard.component")
        .then((m) => m.StudentDashboardComponent),
  },

  {
    // ==========================================================
    // Instructor Dashboard
    // URL: /dashboard
    // ==========================================================
    path: "dashboard",

    // Lazy-load the InstructorDashboardComponent
    loadComponent: () =>
      import(
        "./features/instructor-dashboard/instructor-dashboard.component"
      ).then((m) => m.InstructorDashboardComponent),
  },

  {
    // ==========================================================
    // Course Detail
    // Example: /courses/1
    // ==========================================================
    path: "courses/:id",

    // Lazy-load the CourseDetailComponent
    loadComponent: () =>
      import("./features/course-detail/course-detail.component")
        .then((m) => m.CourseDetailComponent),
  },

  {
    // ==========================================================
    // Enrollment Form
    // URL: /enroll
    // ==========================================================
    path: "enroll",

    // Lazy-load the EnrollmentFormComponent
    loadComponent: () =>
      import("./features/enrollment-form/enrollment-form.component")
        .then((m) => m.EnrollmentFormComponent),
  },

  {
    // ==========================================================
    // Enrollment List
    // URL: /enrollments
    // ==========================================================
    path: "enrollments",

    // Lazy-load the EnrollmentListComponent
    loadComponent: () =>
      import("./features/enrollment-list/enrollment-list.component")
        .then((m) => m.EnrollmentListComponent),
  },

  {
    // ==========================================================
    // Default Route
    // ==========================================================
    // When the user visits:
    // http://localhost:4200/
    //
    // redirect to the student dashboard.
    path: "",
    redirectTo: "student-dashboard",
    pathMatch: "full",
  },
];

