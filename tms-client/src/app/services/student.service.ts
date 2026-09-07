import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  Student,
  CreateStudentRequest
} from '../models/student.model';

@Injectable({
  providedIn: 'root'
})
export class StudentService {

  private http = inject(HttpClient);

  private readonly apiUrl = 'http://localhost:5094/api/v2/Students';

  registerStudent(
    student: CreateStudentRequest
  ): Observable<Student> {
    return this.http.post<Student>(
      this.apiUrl,
      student
    );
  }

  getStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(this.apiUrl);
  }

  getStudent(id: number): Observable<Student> {
    return this.http.get<Student>(
      `${this.apiUrl}/${id}`
    );
  }
}