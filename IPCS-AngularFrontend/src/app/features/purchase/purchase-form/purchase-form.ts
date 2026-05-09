import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * PurchaseFormComponent
 * Handles stock procurement from suppliers.
 * Allows entry of Batch numbers, Expiry dates, and Purchase costs.
 */
@Component({
  selector: 'app-purchase-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './purchase-form.html'
})
export class PurchaseFormComponent implements OnInit {
  // Main Purchase Master Object
  purchase: any = {
    supplierInvoiceNo: '',
    purchaseDate: new Date().toISOString().split('T')[0],
    branchId: 1, // Default branch
    supplierId: null,
    methodId: 1, // Default payment method
    totalAmount: 0,
    discountAmount: 0,
    netAmount: 0,
    paidAmount: 0,
    dueAmount: 0,
    paymentStatus: 'Paid',
    isShipment: false, // If true, stock won't be updated until received
    remarks: '',
    purchaseDetails: [],
    payments: []
  };

  // UI State Variables
  suppliers: any[] = [];
  products: any[] = [];
  units: any[] = [];
  paymentMethods: any[] = [];
  searchTerm: string = '';
  searchResults: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.loadLookups();
  }

  /**
   * Fetches lookup data for Suppliers, Units, and Payment Methods
   */
  loadLookups() {
    this.api.get<any[]>('Supplier/active').subscribe(data => this.suppliers = data);
    this.api.get<any[]>('UOM').subscribe(data => this.units = data);
    this.api.get<any[]>('PaymentMethod').subscribe(data => this.paymentMethods = data);
  }

  /**
   * Searches for products to add to the purchase
   */
  searchProduct() {
    if (this.searchTerm.length < 2) {
      this.searchResults = [];
      return;
    }
    this.api.get<any[]>(`Product?search=${this.searchTerm}`).subscribe(data => {
      this.searchResults = data;
    });
  }

  /**
   * Adds a product to the purchase list
   */
  addProduct(product: any) {
    this.purchase.purchaseDetails.push({
      productId: product.productId,
      productName: product.productName,
      sku: product.sku,
      batchNo: '',
      expiryDate: '',
      qty: 1,
      freeQty: 0,
      uomId: product.uomId,
      uomName: product.uomName,
      purchasePrice: 0,
      mrp: product.mrp,
      lineTotal: 0
    });
    this.searchTerm = '';
    this.searchResults = [];
  }

  /**
   * Removes an item from the purchase grid
   */
  removeItem(index: number) {
    this.purchase.purchaseDetails.splice(index, 1);
    this.calculateTotals();
  }

  /**
   * Calculates the line total for a purchase item
   */
  calculateLineTotal(item: any) {
    item.lineTotal = item.qty * item.purchasePrice;
    this.calculateTotals();
  }

  /**
   * Calculates grand totals for the purchase
   */
  calculateTotals() {
    this.purchase.totalAmount = this.purchase.purchaseDetails.reduce((sum: number, item: any) => sum + item.lineTotal, 0);
    this.purchase.netAmount = this.purchase.totalAmount - this.purchase.discountAmount;
    this.purchase.dueAmount = this.purchase.netAmount - this.purchase.paidAmount;
    this.purchase.paymentStatus = this.purchase.dueAmount <= 0 ? 'Paid' : 'Due';
  }

  /**
   * Submits the purchase record to the API
   */
  onSubmit() {
    if (this.purchase.purchaseDetails.length === 0) {
      alert('Please add at least one product');
      return;
    }

    this.isLoading = true;
    
    // Preparing payment info
    this.purchase.payments = [{
      paymentMethodId: this.purchase.methodId,
      amount: this.purchase.paidAmount,
      paymentDate: this.purchase.purchaseDate
    }];

    this.api.post('Purchase', this.purchase).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.router.navigate(['/dashboard/purchase']);
      },
      error: (err) => {
        alert(err.error?.message || 'Purchase failed');
        this.isLoading = false;
      }
    });
  }
}
