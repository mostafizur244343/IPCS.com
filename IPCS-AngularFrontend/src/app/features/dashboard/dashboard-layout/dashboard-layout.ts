import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { PermissionService } from '../../../core/services/permission';
import { AuthService } from '../../../core/services/auth';

/**
 * DashboardLayoutComponent
 * Main layout containing the sidebar with RBAC-filtered navigation.
 */
@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-layout.html'
})
export class DashboardLayoutComponent implements OnInit {
  isSidebarOpen = true;
  currentUser = { name: 'User', role: 'Staff' };

  // Comprehensive menu items for the entire system
  allMenuItems = [
    { name: 'Dashboard', icon: 'grid', route: '/dashboard/home', permission: null },
    { name: 'Inventory', icon: 'package', route: '/dashboard/inventory', permission: 'Permissions.Inventory.View' },
    { name: 'Sales POS', icon: 'shopping-cart', route: '/dashboard/sales/new', permission: 'Permissions.Sales.Create' },
    { name: 'Sales List', icon: 'list', route: '/dashboard/sales', permission: 'Permissions.Sales.View' },
    { name: 'Purchase', icon: 'truck', route: '/dashboard/purchase', permission: 'Permissions.Purchase.View' },
    { name: 'Transfers', icon: 'repeat', route: '/dashboard/transfers', permission: 'Permissions.Transfer.View' },
    { name: 'Returns', icon: 'rotate-ccw', route: '/dashboard/returns/sales', permission: 'Permissions.SalesReturn.View' },
    { name: 'Reports', icon: 'bar-chart-2', route: '/dashboard/reports', permission: 'Permissions.Administration.ViewDailySummary' },
    
    // Administration
    { name: 'Users', icon: 'users', route: '/dashboard/administration/users', permission: 'Permissions.Administration.ManageUsers' },
    { name: 'Roles', icon: 'shield', route: '/dashboard/administration/roles', permission: 'Permissions.Administration.ManageRoles' },
    
    // Setup Sub-menu
    { name: 'Customers', icon: 'user-plus', route: '/dashboard/setup/customers', permission: 'Permissions.Inventory.View' },
    { name: 'Suppliers', icon: 'briefcase', route: '/dashboard/setup/suppliers', permission: 'Permissions.Inventory.View' },
    { name: 'Branches', icon: 'map-pin', route: '/dashboard/setup/branches', permission: 'Permissions.Administration.ManagePermissions' },
    { name: 'Global Units', icon: 'layers', route: '/dashboard/setup/global-conversions', permission: 'Permissions.Administration.ManagePermissions' },
    { name: 'Settings', icon: 'settings', route: '/dashboard/setup/categories', permission: 'Permissions.Administration.ManagePermissions' },
  ];

  filteredMenuItems: any[] = [];

  constructor(
    private router: Router, 
    public permService: PermissionService,
    private auth: AuthService
  ) {}

  ngOnInit() {
    this.loadUserData();
    this.filterMenu();
  }

  loadUserData() {
    const data = localStorage.getItem('user_data');
    if (data) {
      const user = JSON.parse(data);
      this.currentUser = { name: user.name || 'User', role: user.role || 'Staff' };
    }
  }

  filterMenu() {
    this.filteredMenuItems = this.allMenuItems.filter(item => {
      if (!item.permission) return true;
      return this.permService.hasPermission(item.permission);
    });
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout() {
    this.auth.logout();
  }
}
