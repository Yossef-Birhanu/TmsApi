import { Component, input, output } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Course } from "../../models/course.model";

@Component({
  selector: "tms-course-card",
  standalone: true,
  // RouterLink is needed because the template
  // uses [routerLink].
  imports: [RouterLink],
  templateUrl: "./course-card.component.html",
  styleUrl: "./course-card.component.scss",
})
export class CourseCardComponent {
  // Receives a Course from the parent component.
  course = input.required<Course>();

  // Sends the Course back to the parent.
  enrollClick= output<Course>();
}