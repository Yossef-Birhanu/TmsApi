import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { GradePayload, GradeService } from '../../services/grade.service';
import { exhaustMap, Subject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
@Component({
  selector: 'tms-grade-submission',
  standalone: true,
  imports: [ReactiveFormsModule, MatCardModule,MatFormFieldModule,MatInputModule,MatButtonModule,MatProgressSpinnerModule],
  templateUrl: './grade-submission.component.html',
  styleUrl: './grade-submission.component.scss',
})
export class GradeSubmissionComponent {
  private api=inject(GradeService);
  private fb=inject(FormBuilder);
  //Reactive form definition with initial model values and validators
  gradeForm=this.fb.group({
    studentId:[101, [Validators.required,Validators.min(1)]],
    courseId:[302,[Validators.required, Validators.min(1)]],
    score:[88,[Validators.required, Validators.min(0),Validators.max(100)]]
  });
  isSubmitting=false;
  submissionStatus='';

  //A Subject is a manual event stream template clicks push payloads into it
  private submitClick$=new Subject<GradePayload>();
  constructor(){
    this.submitClick$.pipe(
      //exhaustMap: while the inner HTTP observable is active 
      // all new emissions from submitclick$ are silently dropped
      //dawit can click 50 times only one post request fires
      exhaustMap(payload=>{
        this.isSubmitting=true;
        this.submissionStatus='Submitting grade to server....';
        return this.api.postGrade(payload);
      }),

      //takeUntilDestoryed:automatically unsubscribs when angular 
      //destroyed thid component preventing memory leaks
      //placed inside constructor to inherit the active injectiom context
      takeUntilDestroyed()
    )
      .subscribe({
  next: result => {
    this.isSubmitting = false;

    this.submissionStatus =
      `Grade saved successfully! Record ID: ${result.id}`;
  },

  error: err => {
    this.isSubmitting = false;

    this.submissionStatus =
      `Submission failed: ${err.message || 'Server error'}`;
  }
});
  }
  //The template form submit handler pushes valid values into the protcted stream
  onSubmit(){
    if(this.gradeForm.valid){
      const rawValue=this.gradeForm.getRawValue();
      this.submitClick$.next({
        studentId: Number(rawValue.studentId),
        courseId: Number(rawValue.courseId),
        score: Number(rawValue.score)
      });
    }
  }
}
