import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * RolePermissionsComponent
 * Provides a matrix-style interface to manage permissions for a specific role.
 * Organizes permissions by module (Inventory, Sales, etc.).
 */
@Component({
  selector: 'app-role-permissions',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './role-permissions.html'
})
export class RolePermissionsComponent implements OnInit {
  roleId: string = '';
  roleName: string = '';
  modules: any[] = []; // List of modules with their nested permissions
  isLoading = false;
  errorMessage: string = '';

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.roleId = this.route.snapshot.params['id'];
    this.loadPermissions();
  }

  /**
   * Loads all modules and permissions, highlighting those already assigned to this role
   */
  loadPermissions() {
    this.isLoading = true;
    this.errorMessage = '';
    this.api.get<any[]>(`Permission/all-modules?roleId=${this.roleId}`).subscribe({
      next: (data) => {
        // Map IsAssigned to isSelected for internal UI state if needed, 
        // or just ensure we use the correct names.
        // We'll also ensure displayName is mapped if we want to keep template names.
        this.modules = data.map(m => ({
          ...m,
          permissions: m.permissions.map((p: any) => ({
            ...p,
            isSelected: p.isAssigned // Mapping backend IsAssigned to frontend isSelected
          }))
        }));
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load permissions. Please check if you have administrative rights.';
        this.isLoading = false;
        console.error('Permission Load Error:', err);
      }
    });
  }

  /**
   * Toggles all permissions within a specific module
   */
  toggleModule(module: any, checked: boolean) {
    module.permissions.forEach((p: any) => p.isSelected = checked);
  }

  /**
   * Saves the updated permission set for the role
   */
  savePermissions() {
    this.isLoading = true;
    // Extracting only selected permission IDs
    const selectedIds: number[] = [];
    this.modules.forEach(m => {
      m.permissions.forEach((p: any) => {
        if (p.isSelected) selectedIds.push(p.id); // Fixed: backend uses 'id', not 'permissionId'
      });
    });

    this.api.post('Permission/update-role-permissions', {
      roleId: this.roleId,
      permissionIds: selectedIds
    }).subscribe({
      next: (res: any) => {
        this.router.navigate(['/dashboard/administration/roles']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Update failed. You might not have permission to modify roles.';
        this.isLoading = false;
        console.error('Permission Update Error:', err);
      }
    });
  }
}
