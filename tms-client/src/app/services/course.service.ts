import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map } from "rxjs/operators";
import { Course, CourseDetail, PagedResponse } from "../models/course.model";

@Injectable({
  providedIn: "root"
})
export class CourseService {
  // Gets Angular's HttpClient service.
  private http = inject(HttpClient);

  // Your .NET TMS API uses port 5094 and API version 2.
  private baseUrl = "http://localhost:5094/api/Courses";

  // Gets all courses from the API.
  getAll(page = 1, pageSize = 50) {
    return this.http
      .get<PagedResponse<Course>>(this.baseUrl, {
        params: {
          page: page.toString(),
          pageSize: pageSize.toString()
        }
      })
      .pipe(
        map((p) => p.items)
      );
  }

  // Gets one course by its ID.
  getById(id: string) {
    return this.http.get<CourseDetail>(`${this.baseUrl}/${id}`);
  }
}