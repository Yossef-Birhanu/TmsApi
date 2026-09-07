
import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { Enrollment } from "../models/enrollment.model";

@Injectable({
  providedIn: "root",
})
export class EnrollmentService {

  // Inject Angular HttpClient.
  private http = inject(HttpClient);

  // Base URL of the TMS API enrollment endpoints.
  private baseUrl = "http://localhost:5094/api/v2/enrollments";


  // ==========================================================
  // GET ALL ENROLLMENTS
  // ==========================================================
  getAll(): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(this.baseUrl);
  }


  // ==========================================================
  // APPROVE ENROLLMENT
  // ==========================================================
  approve(id: number): Observable<Enrollment> {
    return this.http.post<Enrollment>(
      `${this.baseUrl}/${id}/approve`,
      {}
    );
  }
}



