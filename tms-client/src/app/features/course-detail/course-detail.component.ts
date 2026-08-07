// Component creates the Angular component.
// input creates a reactive input.
// effect reacts when the input value changes.
import {Component,input,effect} from "@angular/core";

// RouterLink allows us to navigate using Angular routing
// without reloading the entire application.
import { RouterLink } from "@angular/router";

@Component({
  // HTML selector for this component.
  selector: "app-course-detail",

  // This is a standalone component.
  standalone: true,

  // RouterLink is required because the HTML template
  // uses the routerLink directive.
  imports: [RouterLink],

  // HTML template for the component.
  templateUrl: "./course-detail.component.html",

  // Component-specific stylesheet.
  styleUrl: "./course-detail.component.scss",
})
export class CourseDetailComponent {

  // Receives the :id parameter from the route.
  //
  // The route is:
  //     /courses/:id
  //
  // Therefore the input must be named:
  //     id
  //
  // Example:
  //     /courses/1
  //
  // Then:
  //     id() === "1"
  id = input.required<string>();

  constructor() {

    // effect() watches the id signal.
    //
    // When the component receives a new ID,
    // this code runs.
    effect(() => {
      console.log(
        `Loading course detail for ID: ${this.id()}`
      );
    });
  }
}