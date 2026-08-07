export interface Enrollment {
  id: string;
  studentId: number;
  studentName: string;
  courseId: number;
  courseName: string;
  status: EnrollmentStatus;
  enrolledAt: string;
}

export enum EnrollmentStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3,
}
