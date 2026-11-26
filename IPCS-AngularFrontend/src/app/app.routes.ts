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
    loadComponent: () =>
      import('./features/dashboard/dashboard-layout/dashboard-layout').then(
        (m) => m.DashboardLayoutComponent,
      ),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      {
        path: 'home',
        loadComponent: () =>
          import('./features/dashboard/dashboard-home/dashboard-home').then(
            (m) => m.DashboardHomeComponent,
          ),
      },
      {
        path: 'admin-portal',
        loadComponent: () =>
          import('./features/dashboard/admin-portal/admin-portal').then(
            (m) => m.AdminPortalComponent,
          ),
      },
      // Inventory
      {
        path: 'inventory',
        loadComponent: () =>
          import('./features/inventory/product-list/product-list').then(
            (m) => m.ProductListComponent,
          ),
      },
      {
        path: 'inventory/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Products',
            subtitle: 'Products in the trash. Restoring a product brings it back to the inventory list.',
            trashEndpoint: 'Product/trash',
            restoreEndpoint: 'Product/restore',
            idField: 'productId',
            backRoute: '/dashboard/inventory',
            columns: [
              { field: 'productName', label: 'Product Name', bold: true },
              { field: 'sKU', fallbackField: 'sku', label: 'SKU' },
              { field: 'categoryName', label: 'Category' },
              { field: 'salesPrice', label: 'Price', format: 'money' },
              { field: 'currentStock', label: 'Stock' },
            ],
          },
        },
      },
      {
        path: 'inventory/add',
        loadComponent: () =>
          import('./features/inventory/product-form/product-form').then(
            (m) => m.ProductFormComponent,
          ),
      },
      {
        path: 'inventory/edit/:id',
        loadComponent: () =>
          import('./features/inventory/product-form/product-form').then(
            (m) => m.ProductFormComponent,
          ),
      },
      {
        path: 'inventory/ledger',
        loadComponent: () =>
          import('./features/inventory/stock-ledger/stock-ledger').then(
            (m) => m.StockLedgerComponent,
          ),
      },
      {
        path: 'inventory/adjustment',
        loadComponent: () =>
          import('./features/inventory/stock-adjustment/stock-adjustment').then(
            (m) => m.StockAdjustmentComponent,
          ),
      },
      {
        path: 'inventory/expiry',
        loadComponent: () =>
          import('./features/inventory/expiry-tracker/expiry-tracker').then(
            (m) => m.ExpiryTrackerComponent,
          ),
      },
      {
        path: 'inventory/lots',
        loadComponent: () =>
          import('./features/inventory/lot-list/lot-list').then((m) => m.LotListComponent),
      },
      {
        path: 'inventory/branch-stock',
        loadComponent: () =>
          import('./features/inventory/branch-stock/branch-stock').then((m) => m.BranchStockComponent),
      },
      // Sales
      {
        path: 'sales',
        loadComponent: () =>
          import('./features/sales/sales-list/sales-list').then((m) => m.SalesListComponent),
      },
      {
        path: 'sales/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Sales',
            subtitle: 'Sales in the trash. Restoring a sale takes its stock, customer due and daily totals back.',
            trashEndpoint: 'Sales/trash',
            restoreEndpoint: 'Sales/restore',
            idField: 'salesId',
            backRoute: '/dashboard/sales',
            columns: [
              { field: 'invoiceNo', label: 'Invoice No', bold: true },
              { field: 'salesDate', label: 'Date', format: 'date' },
              { field: 'customer.customerName', label: 'Customer' },
              { field: 'totalAmount', label: 'Total Amount', format: 'money' },
              { field: 'paidAmount', label: 'Paid', format: 'money' },
              { field: 'dueAmount', label: 'Due', format: 'money' },
              { field: 'paymentStatus', label: 'Status' },
            ],
          },
        },
      },
      {
        path: 'sales/new',
        loadComponent: () =>
          import('./features/sales/sales-form/sales-form').then((m) => m.SalesFormComponent),
      },
      // Purchase
      {
        path: 'purchase',
        loadComponent: () =>
          import('./features/purchase/purchase-list/purchase-list').then(
            (m) => m.PurchaseListComponent,
          ),
      },
      {
        path: 'purchase/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Purchases',
            subtitle: 'Purchases in the trash. Restoring a purchase puts its stock, supplier due and daily totals back.',
            trashEndpoint: 'Purchase/trash',
            restoreEndpoint: 'Purchase/restore',
            idField: 'purchaseId',
            backRoute: '/dashboard/purchase',
            columns: [
              { field: 'supplierInvoiceNo', label: 'Invoice No', bold: true },
              { field: 'purchaseDate', label: 'Date', format: 'date' },
              { field: 'supplier.supplierName', label: 'Supplier' },
              { field: 'totalAmount', label: 'Total Amount', format: 'money' },
              { field: 'paidAmount', label: 'Paid', format: 'money' },
              { field: 'dueAmount', label: 'Due', format: 'money' },
              { field: 'paymentStatus', label: 'Status' },
            ],
          },
        },
      },
      {
        path: 'purchase/new',
        loadComponent: () =>
          import('./features/purchase/purchase-form/purchase-form').then(
            (m) => m.PurchaseFormComponent,
          ),
      },
      // Reports
      {
        path: 'reports',
        loadComponent: () =>
          import('./features/reports/reports-dashboard/reports-dashboard').then(
            (m) => m.ReportsDashboardComponent,
          ),
      },
      // Transfers & Returns
      {
        path: 'transfers',
        loadComponent: () =>
          import('./features/transfers/transfer-list/transfer-list').then(
            (m) => m.TransferListComponent,
          ),
      },
      {
        path: 'transfers/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Transfers',
            subtitle: 'Transfers in the trash. Restoring a transfer brings it back to the list (stock is untouched).',
            trashEndpoint: 'Transfer/trash',
            restoreEndpoint: 'Transfer/restore',
            idField: 'transferId',
            backRoute: '/dashboard/transfers',
            columns: [
              { field: 'transferCode', label: 'Transfer Code', bold: true },
              { field: 'transferDate', label: 'Date', format: 'date' },
              { field: 'status', label: 'Status' },
              { field: 'remarks', label: 'Remarks' },
            ],
          },
        },
      },
      {
        path: 'transfers/requisitions/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Requisitions',
            subtitle: 'Transfer requisitions in the trash. Restoring a requisition brings it back to the list.',
            trashEndpoint: 'Transfer/requisition/trash',
            restoreEndpoint: 'Transfer/requisition/restore',
            idField: 'requisitionId',
            backRoute: '/dashboard/transfers',
            columns: [
              { field: 'requisitionCode', label: 'Requisition Code', bold: true },
              { field: 'requisitionDate', label: 'Date', format: 'date' },
              { field: 'expectedDate', label: 'Expected', format: 'date' },
              { field: 'status', label: 'Status' },
              { field: 'remarks', label: 'Remarks' },
            ],
          },
        },
      },
      {
        path: 'payments/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Invoice Payments',
            subtitle: 'Payments in the trash. Restoring a payment settles its invoice again (paid, due, status and daily totals).',
            trashEndpoint: 'Payment/trash',
            restoreEndpoint: 'Payment/restore',
            idField: 'paymentId',
            backRoute: '/dashboard/home',
            columns: [
              { field: 'transactionType', label: 'Type', bold: true },
              { field: 'paymentDate', label: 'Date', format: 'date' },
              { field: 'paymentMethod.methodName', label: 'Method' },
              { field: 'amount', label: 'Amount', format: 'money' },
              { field: 'transactionNo', label: 'Transaction No' },
              { field: 'purchaseId', label: 'Purchase #' },
              { field: 'saleId', label: 'Sale #' },
              { field: 'remarks', label: 'Remarks' },
            ],
          },
        },
      },
      {
        path: 'transfers/new',
        loadComponent: () =>
          import('./features/transfers/transfer-form/transfer-form').then(
            (m) => m.TransferFormComponent,
          ),
      },
      {
        path: 'returns/sales',
        loadComponent: () =>
          import('./features/returns/sales-return/sales-return').then(
            (m) => m.SalesReturnComponent,
          ),
      },
      {
        path: 'returns/sales/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Sales Returns',
            subtitle: 'Sales returns in the trash. Restoring a return puts its stock, customer balance and daily totals back.',
            trashEndpoint: 'SalesReturn/trash',
            restoreEndpoint: 'SalesReturn/restore',
            idField: 'returnId',
            backRoute: '/dashboard/returns/sales',
            columns: [
              { field: 'returnNo', label: 'Return No', bold: true },
              { field: 'returnDate', label: 'Date', format: 'date' },
              { field: 'customer.customerName', label: 'Customer' },
              { field: 'totalAmount', label: 'Total Amount', format: 'money' },
              { field: 'refundAmount', label: 'Refund', format: 'money' },
              { field: 'refundType', label: 'Refund Type' },
              { field: 'reason', label: 'Reason' },
            ],
          },
        },
      },
      {
        path: 'returns/purchase',
        loadComponent: () =>
          import('./features/returns/purchase-return/purchase-return').then(
            (m) => m.PurchaseReturnComponent,
          ),
      },
      {
        path: 'returns/purchase/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Purchase Returns',
            subtitle: 'Purchase returns in the trash. Restoring a return puts its stock, supplier due and daily totals back.',
            trashEndpoint: 'PurchaseReturn/trash',
            restoreEndpoint: 'PurchaseReturn/restore',
            idField: 'returnId',
            backRoute: '/dashboard/returns/purchase',
            columns: [
              { field: 'returnNo', label: 'Return No', bold: true },
              { field: 'returnDate', label: 'Date', format: 'date' },
              { field: 'supplier.supplierName', label: 'Supplier' },
              { field: 'totalAmount', label: 'Total Amount', format: 'money' },
              { field: 'refundAmount', label: 'Refund', format: 'money' },
              { field: 'refundType', label: 'Refund Type' },
              { field: 'reason', label: 'Reason' },
            ],
          },
        },
      },
      // Administration
      {
        path: 'administration/roles',
        loadComponent: () =>
          import('./features/administration/role-list/role-list').then((m) => m.RoleListComponent),
      },
      {
        path: 'administration/role-permissions/:id',
        loadComponent: () =>
          import('./features/administration/role-permissions/role-permissions').then(
            (m) => m.RolePermissionsComponent,
          ),
      },
      {
        path: 'administration/users',
        loadComponent: () =>
          import('./features/administration/user-list/user-list').then((m) => m.UserListComponent),
      },
      {
        path: 'administration/users/new',
        loadComponent: () =>
          import('./features/administration/user-form/user-form').then((m) => m.UserFormComponent),
      },
      // Setup / Lookups
      {
        path: 'setup/branches',
        loadComponent: () =>
          import('./features/setup/branch-list/branch-list').then((m) => m.BranchListComponent),
      },
      {
        path: 'setup/branches/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Branches',
            subtitle: 'Branches in the trash. Restoring a branch brings it back to the list.',
            trashEndpoint: 'Branch/trash',
            restoreEndpoint: 'Branch/restore',
            idField: 'branchId',
            backRoute: '/dashboard/setup/branches',
            columns: [
              { field: 'branchName', label: 'Branch Name', bold: true },
              { field: 'branchCode', label: 'Code' },
              { field: 'address', label: 'Address' },
              { field: 'contactNumber', label: 'Phone' },
            ],
          },
        },
      },
      {
        path: 'setup/categories',
        loadComponent: () =>
          import('./features/setup/category-list/category-list').then(
            (m) => m.CategoryListComponent,
          ),
      },
      {
        path: 'setup/categories/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Categories',
            subtitle: 'Categories in the trash. Restoring a category brings it back to the list.',
            trashEndpoint: 'Category/trash',
            restoreEndpoint: 'Category/restore',
            idField: 'categoryId',
            backRoute: '/dashboard/setup/categories',
            columns: [
              { field: 'categoryName', label: 'Category Name', bold: true },
              { field: 'description', label: 'Description' },
            ],
          },
        },
      },
      {
        path: 'setup/unit-conversions',
        loadComponent: () =>
          import('./features/setup/unit-conversion-list/unit-conversion-list').then(
            (m) => m.UnitConversionListComponent,
          ),
      },
      {
        path: 'setup/unit-conversions/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Unit Conversions',
            subtitle: 'Unit conversion rules in the trash. Restoring a rule brings it back to the list.',
            trashEndpoint: 'ProductUnitConversion/trash',
            restoreEndpoint: 'ProductUnitConversion/restore',
            idField: 'conversionId',
            backRoute: '/dashboard/setup/unit-conversions',
            columns: [
              { field: 'product.productName', label: 'Product', bold: true },
              { field: 'fromUnit.uomName', label: 'From Unit' },
              { field: 'toUnit.uomName', label: 'To Unit' },
              { field: 'factor', label: 'Factor' },
            ],
          },
        },
      },
      {
        path: 'setup/global-conversions',
        loadComponent: () =>
          import('./features/setup/global-unit-conversion/global-unit-conversion').then(
            (m) => m.GlobalUnitConversionComponent,
          ),
      },
      {
        path: 'setup/global-conversions/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Global Conversions',
            subtitle: 'Global conversion rules in the trash. Restoring a rule makes it active again.',
            trashEndpoint: 'GlobalUnitConversion/trash',
            restoreEndpoint: 'GlobalUnitConversion/restore',
            idField: 'id',
            backRoute: '/dashboard/setup/global-conversions',
            columns: [
              { field: 'fromUnitName', label: 'From Unit', bold: true },
              { field: 'conversionFactor', label: 'Equals To' },
              { field: 'toUnitName', label: 'To Unit' },
              { field: 'isActive', label: 'Status', format: 'bool', trueText: 'Active', falseText: 'Inactive' },
            ],
          },
        },
      },
      {
        path: 'setup/manufacturers',
        loadComponent: () =>
          import('./features/setup/manufacturer-list/manufacturer-list').then(
            (m) => m.ManufacturerListComponent,
          ),
      },
      {
        path: 'setup/manufacturers/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Manufacturers',
            subtitle: 'Manufacturers in the trash. Restoring a manufacturer brings it back to the list.',
            trashEndpoint: 'Manufacturer/trash',
            restoreEndpoint: 'Manufacturer/restore',
            idField: 'brandId',
            backRoute: '/dashboard/setup/manufacturers',
            columns: [
              { field: 'brandName', label: 'Brand Name', bold: true },
              { field: 'origin', label: 'Origin' },
              { field: 'contactPerson', label: 'Contact Person' },
              { field: 'phoneNumber', label: 'Phone' },
            ],
          },
        },
      },
      {
        path: 'setup/suppliers',
        loadComponent: () =>
          import('./features/setup/supplier-list/supplier-list').then(
            (m) => m.SupplierListComponent,
          ),
      },
      {
        path: 'setup/suppliers/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Suppliers',
            subtitle: 'Suppliers in the trash. Restoring a supplier brings it back to the list.',
            trashEndpoint: 'Supplier/deleted-list',
            restoreEndpoint: 'Supplier/restore',
            idField: 'supplierId',
            backRoute: '/dashboard/setup/suppliers',
            columns: [
              { field: 'supplierCode', label: 'Code' },
              { field: 'supplierName', label: 'Supplier Name', bold: true },
              { field: 'mobile', label: 'Mobile' },
              { field: 'openingBalance', label: 'Balance', format: 'money' },
              { field: 'isDue', label: 'Due / Advance', format: 'bool', trueText: 'Due', falseText: 'Adv' },
            ],
          },
        },
      },
      {
        path: 'setup/customers',
        loadComponent: () =>
          import('./features/setup/customer-list/customer-list').then(
            (m) => m.CustomerListComponent,
          ),
      },
      {
        path: 'setup/customers/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Customers',
            subtitle: 'Customers in the trash. Restoring a customer brings it back to the list.',
            trashEndpoint: 'Customer/trash',
            restoreEndpoint: 'Customer/restore',
            idField: 'customerId',
            backRoute: '/dashboard/setup/customers',
            columns: [
              { field: 'customerName', label: 'Name', bold: true },
              { field: 'mobile', label: 'Phone' },
              { field: 'address', label: 'Address' },
              { field: 'currentDue', label: 'Due', format: 'money' },
              { field: 'advanceBalance', label: 'Advance', format: 'money' },
            ],
          },
        },
      },
      {
        path: 'setup/customer-ledger/:id',
        loadComponent: () =>
          import('./features/setup/customer-ledger/customer-ledger').then(
            (m) => m.CustomerLedgerComponent,
          ),
      },
      {
        path: 'setup/payment-methods',
        loadComponent: () =>
          import('./features/setup/payment-method-list/payment-method-list').then(
            (m) => m.PaymentMethodListComponent,
          ),
      },
      {
        path: 'setup/payment-methods/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Payment Methods',
            subtitle: 'Payment methods in the trash. Restoring a method brings it back to the list.',
            trashEndpoint: 'PaymentMethod/trash',
            restoreEndpoint: 'PaymentMethod/restore',
            idField: 'methodId',
            backRoute: '/dashboard/setup/payment-methods',
            columns: [
              { field: 'methodName', label: 'Method Name', bold: true },
              { field: 'description', label: 'Description' },
              { field: 'isDigital', label: 'Type', format: 'bool', trueText: 'Digital', falseText: 'Cash' },
              { field: 'extraChargePercentage', label: 'Extra Charge', format: 'money' },
              { field: 'minimumAmount', label: 'Minimum Amount', format: 'money' },
              { field: 'isActive', label: 'Status', format: 'bool', trueText: 'Active', falseText: 'Inactive' },
            ],
          },
        },
      },
      {
        path: 'setup/uoms',
        loadComponent: () =>
          import('./features/setup/uom-list/uom-list').then((m) => m.UomListComponent),
      },
      {
        path: 'setup/uoms/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Units',
            subtitle: 'Units of measurement in the trash. Restoring a unit brings it back to the list.',
            trashEndpoint: 'UOM/trash',
            restoreEndpoint: 'UOM/restore',
            idField: 'uomId',
            backRoute: '/dashboard/setup/uoms',
            columns: [
              { field: 'uomName', label: 'Unit Name', bold: true },
              { field: 'description', label: 'Description' },
            ],
          },
        },
      },
      {
        path: 'setup/generics',
        loadComponent: () =>
          import('./features/setup/generic-list/generic-list').then((m) => m.GenericListComponent),
      },
      {
        path: 'setup/generics/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Generics',
            subtitle: 'Generic names in the trash. Restoring a generic brings it back to the list.',
            trashEndpoint: 'GenericInfo/trash',
            restoreEndpoint: 'GenericInfo/restore',
            idField: 'genericId',
            backRoute: '/dashboard/setup/generics',
            columns: [
              { field: 'genericName', label: 'Name', bold: true },
              { field: 'indication', label: 'Indications' },
            ],
          },
        },
      },
      {
        path: 'setup/store-locations',
        loadComponent: () =>
          import('./features/setup/store-location-list/store-location-list').then(
            (m) => m.StoreLocationListComponent,
          ),
      },
      {
        path: 'setup/store-locations/trash',
        loadComponent: () =>
          import('./features/trash/trash-list/trash-list').then(
            (m) => m.TrashListComponent,
          ),
        data: {
          trash: {
            title: 'Deleted Store Locations',
            subtitle: 'Store locations in the trash. Restoring a location brings it back to the list.',
            trashEndpoint: 'StoreLocation/trash',
            restoreEndpoint: 'StoreLocation/restore',
            idField: 'locationId',
            backRoute: '/dashboard/setup/store-locations',
            columns: [
              { field: 'locationName', label: 'Location Name', bold: true },
              { field: 'description', label: 'Description' },
            ],
          },
        },
      },
    ],
  },
];
