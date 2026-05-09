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
    this.api.get<any[]>(`Permission/all-modules?roleId=${this.roleId}`).subscribe({
      next: (data) => {
        this.modules = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
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
        if (p.isSelected) selectedIds.push(p.permissionId);
      });
    });

    this.api.post('Permission/update-role-permissions', {
      roleId: this.roleId,
      permissionIds: selectedIds
    }).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.router.navigate(['/dashboard/administration/roles']);
      },
      error: () => {
        alert('Update failed');
        this.isLoading = false;
      }
    });
  }
}
