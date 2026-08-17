import { Service, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map } from "rxjs/operators";
import {
  Course,
  CourseDetail,
  PagedResponse
} from "../models/course.model";
import { environment } from "../../environments/environment";

@Service()
export class CourseService {
  private http = inject(HttpClient);

  private readonly base = `${environment.apiUrl}/Courses`;

  getAll() {
    return this.http
      .get<PagedResponse<Course>>(this.base, {
        params: {
          page: "1",
          pageSize: "50"
        }
      })
      .pipe(
        map(response => response.items)
      );
  }

  getById(id: string) {
    return this.http.get<CourseDetail>(
      `${this.base}/${id}`
    );
  }

  delete(id: number) {
    return this.http.delete<void>(
      `${this.base}/${id}`
    );
  }
}