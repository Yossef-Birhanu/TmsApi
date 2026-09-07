
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  accessToken: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);

  // ASP.NET Core API
  private apiUrl = 'http://localhost:5094/api/Auth';

  /**
   * Login user
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {

    return this.http
      .post<LoginResponse>(
        `${this.apiUrl}/login`,
        credentials
      )
      .pipe(

        tap((response) => {

          // Store authentication information
          localStorage.setItem(
            'accessToken',
            response.accessToken
          );

          localStorage.setItem(
            'refreshToken',
            response.refreshToken
          );

          localStorage.setItem(
            'userId',
            response.userId
          );

          localStorage.setItem(
            'email',
            response.email
          );

          localStorage.setItem(
            'firstName',
            response.firstName
          );

          localStorage.setItem(
            'lastName',
            response.lastName
          );

          // Decode JWT and store role
          const role = this.getRoleFromToken(
            response.accessToken
          );

          if (role) {
            localStorage.setItem('role', role);
          }

          console.log('Login response:', response);
          console.log('User role:', role);
        })
      );
  }


  /**
   * Get access token
   */
  getAccessToken(): string | null {

    return localStorage.getItem('accessToken');
  }


  /**
   * Get refresh token
   */
  getRefreshToken(): string | null {

    return localStorage.getItem('refreshToken');
  }


  /**
   * Get logged-in user's role
   */
  getRole(): string | null {

    return localStorage.getItem('role');
  }


/**
 * Check whether the logged-in user has a specific role
 */
hasRole(requiredRole: string): boolean {

  const userRole = this.getRole();

  if (!userRole) {
    return false;
  }

  return userRole.toLowerCase() === requiredRole.toLowerCase();
}


  /**
   * Get role directly from JWT
   */
  private getRoleFromToken(token: string): string | null {

    try {

      const payload = JSON.parse(
        atob(
          token.split('.')[1]
            .replace(/-/g, '+')
            .replace(/_/g, '/')
        )
      );

      console.log('JWT payload:', payload);

      // ASP.NET Core commonly uses this claim
      const role =
        payload[
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        ];

      // Also support simple "role"
      return role || payload['role'] || null;

    } catch (error) {

      console.error(
        'Could not decode JWT:',
        error
      );

      return null;
    }
  }


  /**
   * Logout
   */
  logout(): void {

    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userId');
    localStorage.removeItem('email');
    localStorage.removeItem('firstName');
    localStorage.removeItem('lastName');
    localStorage.removeItem('role');
  }


  /**
   * Check whether user is logged in
   */
  isLoggedIn(): boolean {

    return !!this.getAccessToken();
  }
}