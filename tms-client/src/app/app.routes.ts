import { Routes } from '@angular/router';
import { roleGuard } from './guards/role.guard';
import { AdminCourseListComponent } from './features/admin/admin-course-list/admin-course-list.component';
import { MissionComponent } from './pages/mission/mission.component';
import { VisionComponent } from './pages/vision/vision.component';
import { ValuesComponent } from './pages/values/values.component';
import { AboutComponent } from './pages/about/about.component';

export const routes: Routes = [

  // ==========================================================
  // Login
  // URL: /login
  // ==========================================================
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component')
        .then(m => m.LoginComponent),
  },

  // ==========================================================
  // Student Dashboard
  // URL: /student-dashboard
  // ==========================================================
  {
    path: 'student-dashboard',
    loadComponent: () =>
      import('./features/student-dashboard/student-dashboard.component')
        .then(m => m.StudentDashboardComponent),
  },


// ==========================================================
  // Instructor Dashboard
  // URL: /instructor-dashboard
  // ==========================================================
  {
    path: 'instructor-dashboard',
    loadComponent: () =>
      import('./features/instructor-dashboard/instructor-dashboard.component')
        .then(m => m.InstructorDashboardComponent),
  },
  // ==========================================================
  // Course Detail
  // Example: /courses/1
  // ==========================================================
  {
    path: 'courses/:id',
    loadComponent: () =>
      import('./features/course-detail/course-detail.component')
        .then(m => m.CourseDetailComponent),
  },

  // ==========================================================
  // Enrollment Form
  // URL: /enrollment-form
  // ==========================================================
  {
    path: 'enrollment-form',
    loadComponent: () =>
      import('./features/enrollment-form/enrollment-form.component')
        .then(m => m.EnrollmentFormComponent),
  },

  // ==========================================================
  // Enrollment List
  // URL: /enrollments-list
  // ==========================================================
  {
    path: 'enrollments-list',
    loadComponent: () =>
      import('./features/enrollment-list/enrollment-list.component')
        .then(m => m.EnrollmentListComponent),
  },

  // ==========================================================
  // Grade Submission
  // URL: /grade-submission
  // ==========================================================
  {
    path: 'grade-submission',
    loadComponent: () =>
      import('./features/grade-submission/grade-submission.component')
        .then(m => m.GradeSubmissionComponent),
  },

  // ==========================================================
  // Admin Course List
  // URL: /admin/courses-list
  // ==========================================================
  {
    path: 'admin/courses-list',
    component: AdminCourseListComponent,
    canActivate: [roleGuard('Admin')],
  },
   {
    path: 'signup',
    loadComponent: () =>
      import(
        './features/student-registration/student-registration.component'
      ).then(
        m => m.StudentRegistrationComponent
      )
  },


  {
    path: '',
    redirectTo: 'signup',
    pathMatch: 'full'
  },

   {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },

  {
    path: 'home',
    loadComponent: () =>
      import('./pages/home/home.component')
        .then(m => m.HomeComponent)
  },
  {
    path: 'about',
    component: AboutComponent
  },

  {
    path: 'mission',
    component: MissionComponent
  },

  {
    path: 'vision',
    component: VisionComponent
  },

  {
    path: 'values',
    component: ValuesComponent
  },

  // ==========================================================
  // Default Route
  // ==========================================================
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },


  // ==========================================================
  // Unknown Route
  // ==========================================================
  {
    path: '**',
    redirectTo: 'login',
  },
];