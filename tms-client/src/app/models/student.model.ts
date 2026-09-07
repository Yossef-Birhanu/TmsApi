export interface Student {
  id: number;
  registrationNumber: string;
  name: string;
  gpa: number;
  isActive: boolean;
}

export interface CreateStudentRequest {
  registrationNumber: string;
  name: string;
  gpa: number;
  isActive: boolean;
}