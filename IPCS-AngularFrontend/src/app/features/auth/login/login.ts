import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

/**
 * LoginComponent
 * Handles user login using real API authentication.
 * Integrates with RBAC by fetching user permissions upon successful login.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html'
})
export class LoginComponent {
  loginData = { emailOrMobile: '', password: '' };
  isLoading = false;
  errorMsg = '';

  constructor(private auth: AuthService, private router: Router) {}

  /**
   * Submit the login form to the AuthService
   */
  onSubmit() {
    this.isLoading = true;
    this.errorMsg = '';

    this.auth.login(this.loginData).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/dashboard']); // Redirect to dashboard on success
      },
      error: (err) => {
        this.isLoading = false;
        // Displaying error message from API if available
        this.errorMsg = err.error?.message || 'Invalid Username or Password';
      }
    });
  }
}
