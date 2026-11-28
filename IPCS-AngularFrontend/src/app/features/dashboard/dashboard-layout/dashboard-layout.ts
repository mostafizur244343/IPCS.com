import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { PermissionService } from '../../../core/services/permission';
import { AuthService } from '../../../core/services/auth';
import { LogoComponent } from '../../../shared/components/logo/logo';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher';

/**
 * DashboardLayoutComponent
 * Main layout containing the sidebar with RBAC-filtered navigation.
 */
@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, LogoComponent, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './dashboard-layout.html'
})
export class DashboardLayoutComponent implements OnInit {
  isSidebarOpen = true;
  currentUser = { name: 'User', role: 'Staff' };

  menuGroups = [
    {
      title: 'nav.main',
      items: [
        { name: 'nav.controlPanel', icon: 'shield', route: '/dashboard/admin-portal', permission: 'Permissions.Administration.ManageUsers' },
        { name: 'nav.dashboard', icon: 'grid', route: '/dashboard/home', permission: null },
        { name: 'nav.reports', icon: 'bar-chart-2', route: '/dashboard/reports', permission: 'Permissions.Administration.ViewDailySummary' }
      ]
    },
    {
      title: 'nav.operations',
      items: [
        { name: 'nav.salesPos', icon: 'shopping-cart', route: '/dashboard/sales/new', permission: 'Permissions.Sales.Create' },
        { name: 'nav.salesHistory', icon: 'file-text', route: '/dashboard/sales', permission: 'Permissions.Sales.View' },
        { name: 'nav.purchases', icon: 'truck', route: '/dashboard/purchase', permission: 'Permissions.Purchase.View' },
        { name: 'nav.salesReturns', icon: 'rotate-ccw', route: '/dashboard/returns/sales', permission: 'Permissions.SalesReturn.View' },
        { name: 'nav.purchaseReturns', icon: 'rotate-ccw', route: '/dashboard/returns/purchase', permission: 'Permissions.PurchaseReturn.View' },
        { name: 'nav.transfers', icon: 'repeat', route: '/dashboard/transfers', permission: 'Permissions.Transfer.View' },
        { name: 'nav.invoicePayments', icon: 'credit-card', route: '/dashboard/payments/trash', permission: 'Permissions.InvoicePayment.View' }
      ]
    },
    {
      title: 'nav.inventory',
      items: [
        { name: 'nav.products', icon: 'package', route: '/dashboard/inventory', permission: 'Permissions.Product.View' },
        { name: 'nav.stockLedger', icon: 'list', route: '/dashboard/inventory/ledger', permission: 'Permissions.StockLedger.View' },
        { name: 'nav.batches', icon: 'layers', route: '/dashboard/inventory/lots', permission: 'Permissions.LotInfo.View' },
        { name: 'nav.branchStock', icon: 'layers', route: '/dashboard/inventory/branch-stock', permission: 'Permissions.BranchLotStock.View' },
        { name: 'nav.stockAdjustment', icon: 'sliders', route: '/dashboard/inventory/adjustment', permission: 'Permissions.Product.Edit' },
        { name: 'nav.expiryTracker', icon: 'alert-triangle', route: '/dashboard/inventory/expiry', permission: 'Permissions.Product.View' }
      ]
    },
    {
      title: 'nav.setup',
      items: [
        { name: 'nav.branches', icon: 'map-pin', route: '/dashboard/setup/branches', permission: 'Permissions.Branch.View' },
        { name: 'nav.categories', icon: 'settings', route: '/dashboard/setup/categories', permission: 'Permissions.Category.View' },
        { name: 'nav.uom', icon: 'layers', route: '/dashboard/setup/uoms', permission: 'Permissions.UOM.View' },
        { name: 'nav.unitConversions', icon: 'refresh-cw', route: '/dashboard/setup/unit-conversions', permission: 'Permissions.ProductUnitConversion.View' },
        { name: 'nav.globalConversions', icon: 'globe', route: '/dashboard/setup/global-conversions', permission: 'Permissions.GlobalUnitConversion.View' },
        { name: 'nav.manufacturers', icon: 'briefcase', route: '/dashboard/setup/manufacturers', permission: 'Permissions.Manufacturer.View' },
        { name: 'nav.suppliers', icon: 'truck', route: '/dashboard/setup/suppliers', permission: 'Permissions.Supplier.View' },
        { name: 'nav.customers', icon: 'user-plus', route: '/dashboard/setup/customers', permission: 'Permissions.Customer.View' },
        { name: 'nav.paymentMethods', icon: 'credit-card', route: '/dashboard/setup/payment-methods', permission: 'Permissions.PaymentMethod.View' },
        { name: 'nav.generics', icon: 'file-text', route: '/dashboard/setup/generics', permission: 'Permissions.GenericInfo.View' },
        { name: 'nav.storeLocations', icon: 'map', route: '/dashboard/setup/store-locations', permission: 'Permissions.StoreLocation.View' }
      ]
    },
    {
      title: 'nav.administration',
      items: [
        { name: 'nav.users', icon: 'users', route: '/dashboard/administration/users', permission: 'Permissions.Administration.ManageUsers' },
        { name: 'nav.roles', icon: 'shield', route: '/dashboard/administration/roles', permission: 'Permissions.Administration.ManageRoles' }
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
