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

  menuGroups = [
    {
      title: 'Main',
      items: [
        { name: 'Control Panel', icon: 'shield', route: '/dashboard/admin-portal', permission: 'Permissions.Administration.ManageUsers' },
        { name: 'Dashboard', icon: 'grid', route: '/dashboard/home', permission: null },
        { name: 'Reports', icon: 'bar-chart-2', route: '/dashboard/reports', permission: 'Permissions.Administration.ViewDailySummary' }
      ]
    },
    {
      title: 'Operations',
      items: [
        { name: 'Sales POS', icon: 'shopping-cart', route: '/dashboard/sales/new', permission: 'Permissions.Sales.Create' },
        { name: 'Sales History', icon: 'file-text', route: '/dashboard/sales', permission: 'Permissions.Sales.View' },
        { name: 'Purchases', icon: 'truck', route: '/dashboard/purchase', permission: 'Permissions.Purchase.View' },
        { name: 'Sales Returns', icon: 'rotate-ccw', route: '/dashboard/returns/sales', permission: 'Permissions.SalesReturn.View' },
        { name: 'Purchase Returns', icon: 'rotate-ccw', route: '/dashboard/returns/purchase', permission: 'Permissions.PurchaseReturn.View' },
        { name: 'Transfers', icon: 'repeat', route: '/dashboard/transfers', permission: 'Permissions.Transfer.View' },
        { name: 'Invoice Payments', icon: 'credit-card', route: '/dashboard/payments', permission: 'Permissions.InvoicePayment.View' }
      ]
    },
    {
      title: 'Inventory',
      items: [
        { name: 'Products', icon: 'package', route: '/dashboard/inventory', permission: 'Permissions.Product.View' },
        { name: 'Stock Ledger', icon: 'list', route: '/dashboard/inventory/ledger', permission: 'Permissions.StockLedger.View' },
        { name: 'Batches/Lots', icon: 'layers', route: '/dashboard/inventory/lots', permission: 'Permissions.LotInfo.View' },
        { name: 'Branch Stock', icon: 'layers', route: '/dashboard/inventory/branch-stock', permission: 'Permissions.BranchLotStock.View' },
        { name: 'Stock Adjustment', icon: 'sliders', route: '/dashboard/inventory/adjustment', permission: 'Permissions.Product.Edit' },
        { name: 'Expiry Tracker', icon: 'alert-triangle', route: '/dashboard/inventory/expiry', permission: 'Permissions.Product.View' }
      ]
    },
    {
      title: 'Setup & Masters',
      items: [
        { name: 'Branches', icon: 'map-pin', route: '/dashboard/setup/branches', permission: 'Permissions.Branch.View' },
        { name: 'Categories', icon: 'settings', route: '/dashboard/setup/categories', permission: 'Permissions.Category.View' },
        { name: 'UOM (Units)', icon: 'layers', route: '/dashboard/setup/uoms', permission: 'Permissions.UOM.View' },
        { name: 'Unit Conversions', icon: 'refresh-cw', route: '/dashboard/setup/unit-conversions', permission: 'Permissions.ProductUnitConversion.View' },
        { name: 'Global Conversions', icon: 'globe', route: '/dashboard/setup/global-conversions', permission: 'Permissions.GlobalUnitConversion.View' },
        { name: 'Manufacturers', icon: 'briefcase', route: '/dashboard/setup/manufacturers', permission: 'Permissions.Manufacturer.View' },
        { name: 'Suppliers', icon: 'truck', route: '/dashboard/setup/suppliers', permission: 'Permissions.Supplier.View' },
        { name: 'Customers', icon: 'user-plus', route: '/dashboard/setup/customers', permission: 'Permissions.Customer.View' },
        { name: 'Payment Methods', icon: 'credit-card', route: '/dashboard/setup/payment-methods', permission: 'Permissions.PaymentMethod.View' },
        { name: 'Generics', icon: 'file-text', route: '/dashboard/setup/generics', permission: 'Permissions.GenericInfo.View' },
        { name: 'Store Locations', icon: 'map', route: '/dashboard/setup/store-locations', permission: 'Permissions.StoreLocation.View' }
      ]
    },
    {
      title: 'Administration',
      items: [
        { name: 'Users', icon: 'users', route: '/dashboard/administration/users', permission: 'Permissions.Administration.ManageUsers' },
        { name: 'Roles', icon: 'shield', route: '/dashboard/administration/roles', permission: 'Permissions.Administration.ManageRoles' }
      ]
    }
  ];

  filteredGroups: any[] = [];

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
      this.currentUser = { 
        name: user.name || user.Name || 'User', 
        role: user.role || user.Role || 'Staff' 
      };
    }
  }

  filterMenu() {
    const isSuperAdmin = this.currentUser.role === 'SuperAdmin';
    
    this.filteredGroups = this.menuGroups.map(group => {
      return {
        ...group,
        items: group.items.filter(item => {
          if (isSuperAdmin) return true; // SuperAdmin sees everything
          if (!item.permission) return true;
          return this.permService.hasPermission(item.permission);
        })
      };
    }).filter(group => group.items.length > 0);
  }

  isLinkActive(route: string): boolean {
    const currentUrl = this.router.url.split('?')[0];
    
    if (route === '/dashboard/inventory') {
      return currentUrl === '/dashboard/inventory' || 
             currentUrl === '/dashboard/inventory/add' || 
             currentUrl.startsWith('/dashboard/inventory/edit/');
    }
    
    if (route === '/dashboard/sales') {
      return currentUrl === '/dashboard/sales';
    }
    if (route === '/dashboard/purchase') {
      return currentUrl === '/dashboard/purchase';
    }
    if (route === '/dashboard/transfers') {
      return currentUrl === '/dashboard/transfers';
    }

    return currentUrl === route;
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout() {
    this.auth.logout();
  }
}
