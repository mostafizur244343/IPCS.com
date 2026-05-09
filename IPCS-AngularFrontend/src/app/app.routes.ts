import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login';
import { AuthGuard } from './core/guards/auth';

/**
 * Main routing configuration.
 */
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { 
    path: 'dashboard', 
    canActivate: [AuthGuard],
    loadComponent: () => import('./features/dashboard/dashboard-layout/dashboard-layout').then(m => m.DashboardLayoutComponent),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', loadComponent: () => import('./features/dashboard/dashboard-home/dashboard-home').then(m => m.DashboardHomeComponent) },
      // Inventory
      { path: 'inventory', loadComponent: () => import('./features/inventory/product-list/product-list').then(m => m.ProductListComponent) },
      { path: 'inventory/add', loadComponent: () => import('./features/inventory/product-form/product-form').then(m => m.ProductFormComponent) },
      { path: 'inventory/edit/:id', loadComponent: () => import('./features/inventory/product-form/product-form').then(m => m.ProductFormComponent) },
      { path: 'inventory/ledger', loadComponent: () => import('./features/inventory/stock-ledger/stock-ledger').then(m => m.StockLedgerComponent) },
      { path: 'inventory/adjustment', loadComponent: () => import('./features/inventory/stock-adjustment/stock-adjustment').then(m => m.StockAdjustmentComponent) },
      { path: 'inventory/expiry', loadComponent: () => import('./features/inventory/expiry-tracker/expiry-tracker').then(m => m.ExpiryTrackerComponent) },
      // Sales
      { path: 'sales', loadComponent: () => import('./features/sales/sales-list/sales-list').then(m => m.SalesListComponent) },
      { path: 'sales/new', loadComponent: () => import('./features/sales/sales-form/sales-form').then(m => m.SalesFormComponent) },
      // Purchase
      { path: 'purchase', loadComponent: () => import('./features/purchase/purchase-list/purchase-list').then(m => m.PurchaseListComponent) },
      { path: 'purchase/new', loadComponent: () => import('./features/purchase/purchase-form/purchase-form').then(m => m.PurchaseFormComponent) },
      // Reports
      { path: 'reports', loadComponent: () => import('./features/reports/reports-dashboard/reports-dashboard').then(m => m.ReportsDashboardComponent) },
      // Transfers & Returns
      { path: 'transfers', loadComponent: () => import('./features/transfers/transfer-list/transfer-list').then(m => m.TransferListComponent) },
      { path: 'transfers/new', loadComponent: () => import('./features/transfers/transfer-form/transfer-form').then(m => m.TransferFormComponent) },
      { path: 'returns/sales', loadComponent: () => import('./features/returns/sales-return/sales-return').then(m => m.SalesReturnComponent) },
      { path: 'returns/purchase', loadComponent: () => import('./features/returns/purchase-return/purchase-return').then(m => m.PurchaseReturnComponent) },
      // Administration
      { path: 'administration/roles', loadComponent: () => import('./features/administration/role-list/role-list').then(m => m.RoleListComponent) },
      { path: 'administration/role-permissions/:id', loadComponent: () => import('./features/administration/role-permissions/role-permissions').then(m => m.RolePermissionsComponent) },
      { path: 'administration/users', loadComponent: () => import('./features/administration/user-list/user-list').then(m => m.UserListComponent) },
      { path: 'administration/users/new', loadComponent: () => import('./features/administration/user-form/user-form').then(m => m.UserFormComponent) },
      // Setup / Lookups
      { path: 'setup/branches', loadComponent: () => import('./features/setup/branch-list/branch-list').then(m => m.BranchListComponent) },
      { path: 'setup/categories', loadComponent: () => import('./features/setup/category-list/category-list').then(m => m.CategoryListComponent) },
      { path: 'setup/unit-conversions', loadComponent: () => import('./features/setup/unit-conversion-list/unit-conversion-list').then(m => m.UnitConversionListComponent) },
      { path: 'setup/global-conversions', loadComponent: () => import('./features/setup/global-unit-conversion/global-unit-conversion').then(m => m.GlobalUnitConversionComponent) },
      { path: 'setup/manufacturers', loadComponent: () => import('./features/setup/manufacturer-list/manufacturer-list').then(m => m.ManufacturerListComponent) },
      { path: 'setup/suppliers', loadComponent: () => import('./features/setup/supplier-list/supplier-list').then(m => m.SupplierListComponent) },
      { path: 'setup/customers', loadComponent: () => import('./features/setup/customer-list/customer-list').then(m => m.CustomerListComponent) },
      { path: 'setup/customer-ledger/:id', loadComponent: () => import('./features/setup/customer-ledger/customer-ledger').then(m => m.CustomerLedgerComponent) },
      { path: 'setup/payment-methods', loadComponent: () => import('./features/setup/payment-method-list/payment-method-list').then(m => m.PaymentMethodListComponent) },
      { path: 'setup/uoms', loadComponent: () => import('./features/setup/uom-list/uom-list').then(m => m.UomListComponent) },
      { path: 'setup/generics', loadComponent: () => import('./features/setup/generic-list/generic-list').then(m => m.GenericListComponent) },
      { path: 'setup/store-locations', loadComponent: () => import('./features/setup/store-location-list/store-location-list').then(m => m.StoreLocationListComponent) }
    ]
  }
];
