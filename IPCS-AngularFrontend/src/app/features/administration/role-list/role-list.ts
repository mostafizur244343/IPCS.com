import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './role-list.html'
})
export class RoleListComponent implements OnInit {
  public roles: any[] = [];
  public newRoleName: string = '';
  public isLoading: boolean = false;
  public errorMessage: string = '';
  
  // Explicitly defined editing state
  public editingRoleId: string | null = null;
  public editRoleName: string = '';

  // Details state
  public selectedRoleForDetails: any = null;

  constructor(private api: ApiService) {}

  public showDetails(role: any): void {
    this.selectedRoleForDetails = role;
  }

  public closeDetails(): void {
    this.selectedRoleForDetails = null;
  }

  ngOnInit() {
    this.loadRoles();
  }

  public loadRoles(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.api.get<any[]>('Role/get-all-roles').subscribe({
      next: (data) => {
        this.roles = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load roles.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  public createRole(): void {
    if (!this.newRoleName) return;
    this.isLoading = true;
    this.api.post('Role/create-role', { roleName: this.newRoleName }).subscribe({
      next: () => {
        this.newRoleName = '';
        this.loadRoles();
      },
      error: (err) => {
        this.errorMessage = 'Error creating role.';
        this.isLoading = false;
      }
    });
  }

  public editRole(role: any): void {
    if (role) {
      this.editingRoleId = role.id;
      this.editRoleName = role.roleName;
    }
  }

  public cancelEdit(): void {
    this.editingRoleId = null;
    this.editRoleName = '';
  }

  public updateRole(): void {
    if (!this.editingRoleId || !this.editRoleName) return;
    this.isLoading = true;
    this.api.post('Role/update-role', { id: this.editingRoleId, roleName: this.editRoleName }).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadRoles();
      },
      error: (err) => {
        this.errorMessage = 'Update failed.';
        this.isLoading = false;
      }
    });
  }

  public deleteRole(id: string): void {
    if (id && confirm('Are you sure you want to delete this role?')) {
      this.api.delete(`Role/delete-role/${id}`).subscribe(() => {
        this.loadRoles();
      });
    }
  }
}
