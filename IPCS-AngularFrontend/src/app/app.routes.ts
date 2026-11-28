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
            title: 'trash.products.title',
            subtitle: 'trash.products.subtitle',
            trashEndpoint: 'Product/trash',
            restoreEndpoint: 'Product/restore',
            idField: 'productId',
            backRoute: '/dashboard/inventory',
            columns: [
              { field: 'productName', label: 'trash.columns.productName', bold: true },
              { field: 'sKU', fallbackField: 'sku', label: 'trash.columns.sku' },
              { field: 'categoryName', label: 'trash.columns.category' },
              { field: 'salesPrice', label: 'common.price', format: 'money' },
              { field: 'currentStock', label: 'trash.columns.stock' },
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
            title: 'trash.sales.title',
            subtitle: 'trash.sales.subtitle',
            trashEndpoint: 'Sales/trash',
            restoreEndpoint: 'Sales/restore',
            idField: 'salesId',
            backRoute: '/dashboard/sales',
            columns: [
              { field: 'invoiceNo', label: 'trash.columns.invoiceNo', bold: true },
              { field: 'salesDate', label: 'common.date', format: 'date' },
              { field: 'customer.customerName', label: 'trash.columns.customer' },
              { field: 'totalAmount', label: 'trash.columns.totalAmount', format: 'money' },
              { field: 'paidAmount', label: 'common.paid', format: 'money' },
              { field: 'dueAmount', label: 'common.due', format: 'money' },
              { field: 'paymentStatus', label: 'common.status' },
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
            title: 'trash.purchases.title',
            subtitle: 'trash.purchases.subtitle',
            trashEndpoint: 'Purchase/trash',
            restoreEndpoint: 'Purchase/restore',
            idField: 'purchaseId',
            backRoute: '/dashboard/purchase',
            columns: [
              { field: 'supplierInvoiceNo', label: 'trash.columns.invoiceNo', bold: true },
              { field: 'purchaseDate', label: 'common.date', format: 'date' },
              { field: 'supplier.supplierName', label: 'trash.columns.supplier' },
              { field: 'totalAmount', label: 'trash.columns.totalAmount', format: 'money' },
              { field: 'paidAmount', label: 'common.paid', format: 'money' },
              { field: 'dueAmount', label: 'common.due', format: 'money' },
              { field: 'paymentStatus', label: 'common.status' },
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
            title: 'trash.transfers.title',
            subtitle: 'trash.transfers.subtitle',
            trashEndpoint: 'Transfer/trash',
            restoreEndpoint: 'Transfer/restore',
            idField: 'transferId',
            backRoute: '/dashboard/transfers',
            columns: [
              { field: 'transferCode', label: 'trash.columns.transferCode', bold: true },
              { field: 'transferDate', label: 'common.date', format: 'date' },
              { field: 'status', label: 'common.status' },
              { field: 'remarks', label: 'trash.columns.remarks' },
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
            title: 'trash.requisitions.title',
            subtitle: 'trash.requisitions.subtitle',
            trashEndpoint: 'Transfer/requisition/trash',
            restoreEndpoint: 'Transfer/requisition/restore',
            idField: 'requisitionId',
            backRoute: '/dashboard/transfers',
            columns: [
              { field: 'requisitionCode', label: 'trash.columns.requisitionCode', bold: true },
              { field: 'requisitionDate', label: 'common.date', format: 'date' },
              { field: 'expectedDate', label: 'trash.columns.expected', format: 'date' },
              { field: 'status', label: 'common.status' },
              { field: 'remarks', label: 'trash.columns.remarks' },
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
            title: 'trash.payments.title',
            subtitle: 'trash.payments.subtitle',
            trashEndpoint: 'Payment/trash',
            restoreEndpoint: 'Payment/restore',
            idField: 'paymentId',
            backRoute: '/dashboard/home',
            columns: [
              { field: 'transactionType', label: 'trash.columns.type', bold: true },
              { field: 'paymentDate', label: 'common.date', format: 'date' },
              { field: 'paymentMethod.methodName', label: 'trash.columns.method' },
              { field: 'amount', label: 'common.amount', format: 'money' },
              { field: 'transactionNo', label: 'trash.columns.transactionNo' },
              { field: 'purchaseId', label: 'trash.columns.purchaseNo' },
              { field: 'saleId', label: 'trash.columns.saleNo' },
              { field: 'remarks', label: 'trash.columns.remarks' },
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
            title: 'trash.salesReturns.title',
            subtitle: 'trash.salesReturns.subtitle',
            trashEndpoint: 'SalesReturn/trash',
            restoreEndpoint: 'SalesReturn/restore',
            idField: 'returnId',
            backRoute: '/dashboard/returns/sales',
            columns: [
              { field: 'returnNo', label: 'trash.columns.returnNo', bold: true },
              { field: 'returnDate', label: 'common.date', format: 'date' },
              { field: 'customer.customerName', label: 'trash.columns.customer' },
              { field: 'totalAmount', label: 'trash.columns.totalAmount', format: 'money' },
              { field: 'refundAmount', label: 'trash.columns.refund', format: 'money' },
              { field: 'refundType', label: 'trash.columns.refundType' },
              { field: 'reason', label: 'trash.columns.reason' },
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
            title: 'trash.purchaseReturns.title',
            subtitle: 'trash.purchaseReturns.subtitle',
            trashEndpoint: 'PurchaseReturn/trash',
            restoreEndpoint: 'PurchaseReturn/restore',
            idField: 'returnId',
            backRoute: '/dashboard/returns/purchase',
            columns: [
              { field: 'returnNo', label: 'trash.columns.returnNo', bold: true },
              { field: 'returnDate', label: 'common.date', format: 'date' },
              { field: 'supplier.supplierName', label: 'trash.columns.supplier' },
              { field: 'totalAmount', label: 'trash.columns.totalAmount', format: 'money' },
              { field: 'refundAmount', label: 'trash.columns.refund', format: 'money' },
              { field: 'refundType', label: 'trash.columns.refundType' },
              { field: 'reason', label: 'trash.columns.reason' },
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
            title: 'trash.branches.title',
            subtitle: 'trash.branches.subtitle',
            trashEndpoint: 'Branch/trash',
            restoreEndpoint: 'Branch/restore',
            idField: 'branchId',
            backRoute: '/dashboard/setup/branches',
            columns: [
              { field: 'branchName', label: 'trash.columns.branchName', bold: true },
              { field: 'branchCode', label: 'common.code' },
              { field: 'address', label: 'common.address' },
              { field: 'contactNumber', label: 'common.phone' },
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
            title: 'trash.categories.title',
            subtitle: 'trash.categories.subtitle',
            trashEndpoint: 'Category/trash',
            restoreEndpoint: 'Category/restore',
            idField: 'categoryId',
            backRoute: '/dashboard/setup/categories',
            columns: [
              { field: 'categoryName', label: 'trash.columns.categoryName', bold: true },
              { field: 'description', label: 'common.description' },
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
            title: 'trash.unitConversions.title',
            subtitle: 'trash.unitConversions.subtitle',
            trashEndpoint: 'ProductUnitConversion/trash',
            restoreEndpoint: 'ProductUnitConversion/restore',
            idField: 'conversionId',
            backRoute: '/dashboard/setup/unit-conversions',
            columns: [
              { field: 'product.productName', label: 'trash.columns.product', bold: true },
              { field: 'fromUnit.uomName', label: 'trash.columns.fromUnit' },
              { field: 'toUnit.uomName', label: 'trash.columns.toUnit' },
              { field: 'factor', label: 'trash.columns.factor' },
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
            title: 'trash.globalConversions.title',
            subtitle: 'trash.globalConversions.subtitle',
            trashEndpoint: 'GlobalUnitConversion/trash',
            restoreEndpoint: 'GlobalUnitConversion/restore',
            idField: 'id',
            backRoute: '/dashboard/setup/global-conversions',
            columns: [
              { field: 'fromUnitName', label: 'trash.columns.fromUnit', bold: true },
              { field: 'conversionFactor', label: 'trash.columns.equalsTo' },
              { field: 'toUnitName', label: 'trash.columns.toUnit' },
              { field: 'isActive', label: 'common.status', format: 'bool', trueText: 'common.active', falseText: 'common.inactive' },
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
            title: 'trash.manufacturers.title',
            subtitle: 'trash.manufacturers.subtitle',
            trashEndpoint: 'Manufacturer/trash',
            restoreEndpoint: 'Manufacturer/restore',
            idField: 'brandId',
            backRoute: '/dashboard/setup/manufacturers',
            columns: [
              { field: 'brandName', label: 'trash.columns.brandName', bold: true },
              { field: 'origin', label: 'trash.columns.origin' },
              { field: 'contactPerson', label: 'trash.columns.contactPerson' },
              { field: 'phoneNumber', label: 'common.phone' },
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
            title: 'trash.suppliers.title',
            subtitle: 'trash.suppliers.subtitle',
            trashEndpoint: 'Supplier/deleted-list',
            restoreEndpoint: 'Supplier/restore',
            idField: 'supplierId',
            backRoute: '/dashboard/setup/suppliers',
            columns: [
              { field: 'supplierCode', label: 'common.code' },
              { field: 'supplierName', label: 'trash.columns.supplierName', bold: true },
              { field: 'mobile', label: 'trash.columns.mobile' },
              { field: 'openingBalance', label: 'trash.columns.balance', format: 'money' },
              { field: 'isDue', label: 'trash.columns.dueAdvance', format: 'bool', trueText: 'common.due', falseText: 'common.advanceShort' },
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
            title: 'trash.customers.title',
            subtitle: 'trash.customers.subtitle',
            trashEndpoint: 'Customer/trash',
            restoreEndpoint: 'Customer/restore',
            idField: 'customerId',
            backRoute: '/dashboard/setup/customers',
            columns: [
              { field: 'customerName', label: 'common.name', bold: true },
              { field: 'mobile', label: 'common.phone' },
              { field: 'address', label: 'common.address' },
              { field: 'currentDue', label: 'common.due', format: 'money' },
              { field: 'advanceBalance', label: 'common.advance', format: 'money' },
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
            title: 'trash.paymentMethods.title',
            subtitle: 'trash.paymentMethods.subtitle',
            trashEndpoint: 'PaymentMethod/trash',
            restoreEndpoint: 'PaymentMethod/restore',
            idField: 'methodId',
            backRoute: '/dashboard/setup/payment-methods',
            columns: [
              { field: 'methodName', label: 'trash.columns.methodName', bold: true },
              { field: 'description', label: 'common.description' },
              { field: 'isDigital', label: 'trash.columns.type', format: 'bool', trueText: 'common.digital', falseText: 'common.cash' },
              { field: 'extraChargePercentage', label: 'trash.columns.extraCharge', format: 'money' },
              { field: 'minimumAmount', label: 'trash.columns.minimumAmount', format: 'money' },
              { field: 'isActive', label: 'common.status', format: 'bool', trueText: 'common.active', falseText: 'common.inactive' },
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
            title: 'trash.units.title',
            subtitle: 'trash.units.subtitle',
            trashEndpoint: 'UOM/trash',
            restoreEndpoint: 'UOM/restore',
            idField: 'uomId',
            backRoute: '/dashboard/setup/uoms',
            columns: [
              { field: 'uomName', label: 'trash.columns.unitName', bold: true },
              { field: 'description', label: 'common.description' },
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
            title: 'trash.generics.title',
            subtitle: 'trash.generics.subtitle',
            trashEndpoint: 'GenericInfo/trash',
            restoreEndpoint: 'GenericInfo/restore',
            idField: 'genericId',
            backRoute: '/dashboard/setup/generics',
            columns: [
              { field: 'genericName', label: 'common.name', bold: true },
              { field: 'indication', label: 'trash.columns.indications' },
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
            title: 'trash.storeLocations.title',
            subtitle: 'trash.storeLocations.subtitle',
            trashEndpoint: 'StoreLocation/trash',
            restoreEndpoint: 'StoreLocation/restore',
            idField: 'locationId',
            backRoute: '/dashboard/setup/store-locations',
            columns: [
              { field: 'locationName', label: 'trash.columns.locationName', bold: true },
              { field: 'description', label: 'common.description' },
            ],
          },
        },
      },
    ],
  },
];
