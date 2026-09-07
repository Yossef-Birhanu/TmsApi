import { firstValueFrom } from "rxjs";
import { EnrollmentService } from "./enrollment.service";
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import { provideHttpClient } from "@angular/common/http";
import { TestBed } from "@angular/core/testing";


describe("EnrollmentService", () => {
  let httpMock: HttpTestingController;
  let service: EnrollmentService;
   
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(),provideHttpClientTesting()],
    });
    httpMock = TestBed.inject(HttpTestingController);
    service = TestBed.inject(EnrollmentService);
  });
  
  afterEach(() => {
    httpMock.verify();
  });
   it ("getAll() issues GET/api/enrollments and maps the response",
    async ()=> {
      const result = firstValueFrom(service.getAll());
      const req= httpMock.expectOne((r)=>
      r.url.endsWith("/api/v2/enrollments")); 

    expect(req.request.method).toBe("GET");
    req.flush([{id:1,studentId:11,studentName:"Ababa",courseId:101,courseName:"Intro  to CS",status:"pending",enrolledAt:"2026-08-12T10:00:00Z"},
      {id:2,studentId:12,studentName:"Kebede",courseId:102,courseName:"Data Structures",status:"approved",enrolledAt:"2026-08-12T10:00:00Z"}
    ]);

    const enrollments = await result;
    expect(enrollments).toHaveLength(2);
    expect(enrollments[0].courseName).toBe("Intro  to CS");

    });
    it("approve(id) issues POT /api/enrollments/{id}/approve", async ()=> {
      const result = firstValueFrom(service.approve(42));
      const req = httpMock.expectOne((r) =>
        r.url.endsWith("/api/v2/enrollments/42/approve"));
      expect(req.request.method).toBe("POST");
      req.flush({
        id: 42,
        studentId: 11,
        studentName: "Ababa",
        courseId: 101,
        courseName: "Intro to CS",
        status: "approved",
        enrolledAt: "2026-08-12T10:00:00Z"
      });
      const approvedEnrollment = await result;
      expect(approvedEnrollment.status).toBe("approved");

      
    }); 
      });

  
