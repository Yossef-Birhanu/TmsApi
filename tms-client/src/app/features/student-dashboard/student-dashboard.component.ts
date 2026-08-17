import { Component, computed, inject, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";

import { CourseService } from "../../services/course.service";
import { CourseCardComponent } from "../../ui/course-card/course-card.component";
import { Course } from "../../models/course.model";


 @Component({
  // The HTML selector for this component.
  selector: "app-student-dashboard",

  // This is a standalone Angular component.
  standalone: true,

  // Allows this component to use <tms-course-card>
  // in its HTML template.
  imports: [CourseCardComponent],

  // HTML template for this component.
  templateUrl: "./student-dashboard.component.html",

  // CSS/SCSS file for this component.
  styleUrl: "./student-dashboard.component.scss",
})

export class StudentDashboardComponent {
  
  // Inject CourseService.
  private api = inject(CourseService);

  studentName = signal("Liya Kebede");

  earnedCredits = signal(45);

  graduationStatus = computed(() =>
    this.earnedCredits() >= 120
      ? "Eligible for Graduation"
      : "In Progress"
  );

   // Add 3 credits when the student registers.
  registerForClass() {
    this.earnedCredits.update(credits => credits + 3);
  }
  // Loads courses from the .NET API.
  coursesResource = rxResource({
    stream: () => this.api.getAll()
  });

  // Keep your existing handleEnroll method here.
  handleEnroll(course: Course) {
    console.log("Enrolling in:", course);
  }
}



// // Import Angular features.
// // Component = defines an Angular component.
// // signal = creates reactive state.
// // computed = creates a value derived from another signal.
// import { Component, signal, computed } from "@angular/core";

// // Import the CourseCardComponent so we can use
// // <tms-course-card> inside this component.
// import { CourseCardComponent } from "../../ui/course-card/course-card.component";

// // Import the Course interface/model.
// import { Course } from "../../models/course.model";

// @Component({
//   // The HTML selector for this component.
//   selector: "app-student-dashboard",

//   // This is a standalone Angular component.
//   standalone: true,

//   // Allows this component to use <tms-course-card>
//   // in its HTML template.
//   imports: [CourseCardComponent],

//   // HTML template for this component.
//   templateUrl: "./student-dashboard.component.html",

//   // CSS/SCSS file for this component.
//   styleUrl: "./student-dashboard.component.scss",
// })
// export class StudentDashboardComponent {

//   // --------------------------------------------------
//   // Student Information
//   // --------------------------------------------------

//   // Stores the student's name as a reactive signal.
//   studentName = signal("Liya Kebede");

//   // Stores the number of credits the student has earned.
//   earnedCredits = signal(45);

//   // Calculates the student's graduation status automatically.
//   //
//   // If earned credits are 120 or more:
//   //     "Eligible for Graduation"
//   //
//   // Otherwise:
//   //     "In Progress"
//   graduationStatus = computed(() =>
//     this.earnedCredits() >= 120
//       ? "Eligible for Graduation"
//       : "In Progress"
//   );

//   // --------------------------------------------------
//   // Register for a Class
//   // --------------------------------------------------

//   // Adds 3 credits when the student registers for a class.
//   registerForClass() {
//     this.earnedCredits.update((c) => c + 3);
//   }

//   // --------------------------------------------------
//   // Selected Course
//   // --------------------------------------------------

//   // Stores the course selected by the student.
//   //
//   // Course | null means:
//   // - A Course object can be stored here.
//   // - null means no course has been selected yet.
//   selectedCourse = signal<Course | null>(null);

//   // --------------------------------------------------
//   // Sample Course
//   // --------------------------------------------------

//   // A sample course that can be used for testing
//   // the CourseCardComponent.
//   sampleCourse: Course = {
//     id: 1,
//     title: "Advanced Java Services",
//     code: "CSE-101",
//     maxCapacity: 30,
//     enrollmentCount: 12,
//   };

//   // --------------------------------------------------
//   // Handle Course Enrollment
//   // --------------------------------------------------

//   // Receives the Course emitted by CourseCardComponent.
//   //
//   // The selected course is stored inside selectedCourse.
//   handleEnroll(course: Course) {

//     // Store the selected course.
//     this.selectedCourse.set(course);

//     // Display the selected course in the browser console.
//     console.log(
//       "Enrollment requested for:",
//       course.title
//     );
//   }

//   // --------------------------------------------------
//   // Available Courses
//   // --------------------------------------------------

//   // Stores all courses currently available for students.
//   //
//   // Because this is a signal, the HTML template can
//   // react automatically when the courses change.
//   availableCourses = signal<Course[]>([
//     {
//       id: 1,
//       title: "Advanced Java Services",
//       code: "CSE-101",
//       maxCapacity: 30,
//       enrollmentCount: 12,
//     },
//     {
//       id: 2,
//       title: "Data Structures and Algorithms",
//       code: "CSE-102",
//       maxCapacity: 25,
//       enrollmentCount: 20,
//     },
//     {
//       id: 3,
//       title: "Web Development with Angular",
//       code: "CSE-103",
//       maxCapacity: 40,
//       enrollmentCount: 35,
//     },
//   ]);
// }
