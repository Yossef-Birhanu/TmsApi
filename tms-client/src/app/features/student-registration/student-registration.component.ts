
import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';

import { StudentService } from '../../services/student.service';

@Component({
  selector: 'app-student-registration',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './student-registration.component.html',
  styleUrl: './student-registration.component.scss'
})
export class StudentRegistrationComponent {

  // ============================================================
  // DEPENDENCY INJECTION
  // ============================================================

  private fb = inject(FormBuilder);
  private studentService = inject(StudentService);
  private router = inject(Router);

  // ============================================================
  // COMPONENT STATE
  // ============================================================

  isSubmitting = false;

  successMessage = '';

  errorMessage = '';

  // ============================================================
  // STUDENT REGISTRATION FORM
  // ============================================================

  studentForm = this.fb.nonNullable.group({

    // Registration Number
    registrationNumber: [
      '',
      [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50)
      ]
    ],

    // Student Name
    name: [
      '',
      [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]
    ],

    // GPA
    gpa: [
      0,
      [
        Validators.required,
        Validators.min(0),
        Validators.max(4)
      ]
    ],

    // Active Status
    isActive: [true]

  });

  // ============================================================
  // FORM CONTROL GETTERS
  // ============================================================

  get registrationNumber() {
    return this.studentForm.controls.registrationNumber;
  }

  get name() {
    return this.studentForm.controls.name;
  }

  get gpa() {
    return this.studentForm.controls.gpa;
  }

  // ============================================================
  // SUBMIT REGISTRATION
  // ============================================================

  submit(): void {

    // Clear previous messages
    this.successMessage = '';
    this.errorMessage = '';

    // Validate form
    if (this.studentForm.invalid) {

      this.studentForm.markAllAsTouched();

      return;
    }

    // Show submitting state
    this.isSubmitting = true;

    // Get form values
    const student = this.studentForm.getRawValue();

    // Send student to API
    this.studentService.registerStudent(student).subscribe({

      // ========================================================
      // SUCCESS
      // ========================================================

      next: (response) => {

        this.isSubmitting = false;

        this.successMessage =
          `Student ${response.name} registered successfully.`;

        // Reset form
        this.studentForm.reset({
          registrationNumber: '',
          name: '',
          gpa: 0,
          isActive: true
        });

      },

      // ========================================================
      // ERROR
      // ========================================================

      error: (error) => {

        this.isSubmitting = false;

        console.error(
          'Student registration failed:',
          error
        );

        // Duplicate registration number
        if (error.status === 409) {

          this.errorMessage =
            'Registration number already exists.';

        }

        // Validation error
        else if (error.status === 400) {

          this.errorMessage =
            'Please check the entered information.';

        }

        // Other errors
        else {

          this.errorMessage =
            'Unable to register student. Please try again.';

        }

      }

    });
  }

  // ============================================================
  // GO TO LOGIN PAGE
  // ============================================================

  goToLogin(): void {

    this.router.navigate(['/login']);

  }

  // ============================================================
  // CANCEL REGISTRATION
  // ============================================================

  cancel(): void {

    this.router.navigate(['/']);

  }

}