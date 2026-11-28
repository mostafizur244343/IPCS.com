import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth';
import { LogoComponent } from '../../../shared/components/logo/logo';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher';

/**
 * LoginComponent
 * Handles user login using real API authentication.
 * Integrates with RBAC by fetching user permissions upon successful login.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, LogoComponent, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './login.html',
})
export class LoginComponent {
  loginData = { emailOrMobile: '', password: '' };
  isLoading = false;
  errorMsg = '';
  showPassword = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    private translate: TranslateService,
  ) {}

  /** Show/hide the password (UI-only state). */
  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  /**
   * Submit the login form to the AuthService
   */
  onSubmit() {
    this.isLoading = true;
    this.errorMsg = '';

    const payload = {
      emailOrMobile: this.loginData.emailOrMobile.trim(),
      password: this.loginData.password.trim()
    };

    this.auth.login(payload).subscribe({
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
        this.errorMsg = err.error?.message || err.error?.Message || this.translate.instant('auth.invalidCredentials');
      },
    });
  }
}
