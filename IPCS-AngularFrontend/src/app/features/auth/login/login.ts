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
  templateUrl: './login.html',
})
export class LoginComponent {
  loginData = { emailOrMobile: '', password: '' };
  isLoading = false;
  errorMsg = '';

  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  /**
   * Submit the login form to the AuthService
   */
  onSubmit() {
    this.isLoading = true;
    this.errorMsg = '';

    this.auth.login(this.loginData).subscribe({
      next: (res) => {
        this.isLoading = false;
        const token = res.token || res.Token || res.securityKey || res.SecurityKey;
        if (token) {
          localStorage.setItem('token', token);
        }
        const user = res.user || res.User;
        if (user) {
          localStorage.setItem('user_data', JSON.stringify(user));
        }
        const role = (user?.role || user?.Role || 'Staff').toLowerCase();

        // Redirect to Admin Portal if role is any kind of Admin
        if (role.includes('admin') || role.includes('superadmin') || role.includes('manager')) {
          this.router.navigate(['/dashboard/admin-portal']);
        } else {
          this.router.navigate(['/dashboard/home']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        // Displaying error message from API if available
        this.errorMsg = err.error?.message || err.error?.Message || 'Invalid Username or Password';
      },
    });
  }
}
