import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * UserFormComponent
 * Handles the creation and modification of staff accounts.
 * Includes assignment of security roles and specific branch affiliation.
 */
@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './user-form.html',
})
export class UserFormComponent implements OnInit {
  // New User data model
  user: any = {
    fullName: '',
    email: '',
    mobileNumber: '',
    password: '',
    confirmPassword: '',
    branchId: null,
    roleName: '',
  };

  // Lookup data
  roles: any[] = [];
  branches: any[] = [];
  isLoading = false;

  constructor(
    private api: ApiService,
    private router: Router,
  ) {}

  ngOnInit() {
    this.loadLookups();
  }

  /**
   * Fetches roles and branches to populate dropdowns
   */
  loadLookups() {
    this.api.get<any[]>('Role/get-all-roles').subscribe((data) => (this.roles = data));
    this.api.get<any[]>('Branch').subscribe((data) => (this.branches = data));
  }

  /**
   * Submits the registration request to the Account API
   */
  onSubmit() {
    this.isLoading = true;
    this.api.post('Account/register', this.user).subscribe({
      next: (res: any) => {
        alert(res.message || res.Message);
        this.router.navigate(['/dashboard/administration/users']);
      },
      error: (err) => {
        alert(err.error?.message || err.error?.Message || 'Failed to create user');
        this.isLoading = false;
      },
    });
  }
}
