import { Component, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {FormBuilder,ReactiveFormsModule,Validators} from '@angular/forms';
import { Router,RouterLink} from '@angular/router';

import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,

  imports: [ReactiveFormsModule,RouterLink],

  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);


  // =====================================================
  // LOGIN FORM
  // =====================================================

  loginForm = this.fb.nonNullable.group({

    email: ['', [Validators.required,Validators.email]],

    password: ['', [ Validators.required ]]
  });

  // =====================================================
  // VARIABLES
  // =====================================================

  errorMessage = '';

  showPassword = false;


  // =====================================================
  // SHOW / HIDE PASSWORD
  // =====================================================

  togglePassword(): void {

    this.showPassword = !this.showPassword;

  }


  // =====================================================
  // LOGIN
  // =====================================================

  login(): void{
    //check form validation
    if(this.loginForm.invalid){
      this.loginForm.markAllAsTouched();
      return;
    }
    //Clear previous error
    this.errorMessage='';
    const credentials=this.loginForm.getRawValue();
    console.log('Sending login request:',{
      email: credentials.email
    });
    this.authService.login(credentials).subscribe({
      next:(response)=>{
        console.log("Login successful");
        console.log('Login response:', response);

        const role=this.authService.getRole();
        console.log('User role:', role);

          if (!role) {
            console.error('ROLE IS MISSING');
             this.errorMessage ='Login successful, but user role was not found.';
             return;
             }
        switch(role?.toLowerCase()){
          case 'admin':
            this.router.navigate(['/admin']);
            break;
          case 'student': 
          this.router.navigate(['/student-dashboard'])
          break;
          case 'instructor':
            this.router.navigate(['/instructor-dashboard']);
            break;
           default:
            this.errorMessage='Login successful, but user role was not found.';
            console.error('Unkown or missing user role:', role);
            break;


        }
      },
      error: (error: HttpErrorResponse)=>{
        console.error('Login failed:', error);
        if(error.status ===400){
          this.errorMessage='Please check your email and password';
        }
        else if(error.status===401){
          this.errorMessage='Invalid email or password.';
        }
        else if (error.status===423){
          this.errorMessage='Your account is temporarily lockedk.please try again later.';
        }
        else if (error.status === 503) {
           this.errorMessage ='The authentication service is currently unavailable. Please try again later.';
          }
        else if (error.status===0){
          this.errorMessage='Cannot connect to the API . Make sure the API is running.';
        }
        else{
          this.errorMessage='Login failed. please try again.';
        }
      }
    });
 
  }
}

