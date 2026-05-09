import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * RoleListComponent
 * Allows administrators to manage system roles (e.g., Admin, Staff, Manager).
 */
@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './role-list.html'
})
export class RoleListComponent implements OnInit {
  roles: any[] = []; // List of roles from Identity system
  newRoleName: string = '';
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadRoles();
  }

  /**
   * Fetches all available roles from the API
   */
  loadRoles() {
    this.isLoading = true;
    this.api.get<any[]>('Role/get-all-roles').subscribe({
      next: (data) => {
        this.roles = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Creates a new role in the system
   */
  createRole() {
    if (!this.newRoleName) return;
    this.isLoading = true;
    this.api.post('Role/create-role', { roleName: this.newRoleName }).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.newRoleName = '';
        this.loadRoles();
      },
      error: (err) => {
        alert('Error creating role');
        this.isLoading = false;
      }
    });
  }

  /**
   * Deletes a role from the system
   */
  deleteRole(id: string) {
    if (confirm('Are you sure you want to delete this role? Users assigned to this role will lose their permissions.')) {
      this.api.delete(`Role/delete-role/${id}`).subscribe(() => {
        this.loadRoles();
      });
    }
  }
}
