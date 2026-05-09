import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * SalesFormComponent
 * POS (Point of Sale) system to process customer sales.
 * Handles product selection, tax/discount calculations, and payments.
 */
@Component({
  selector: 'app-sales-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './sales-form.html'
})
export class SalesFormComponent implements OnInit {
  // Main Sales Master Object
  sale: any = {
    salesDate: new Date().toISOString().split('T')[0],
    branchId: 1, // Default branch
    customerId: null,
    totalAmount: 0,
    discountAmount: 0,
    netAmount: 0,
    paidAmount: 0,
    dueAmount: 0,
    changeAmount: 0,
    isChangeConvertedToCredit: false,
    isChangeTakenAsIncome: false,
    paymentStatus: 'Paid',
    remarks: '',
    salesDetails: [],
    payments: []
  };

  // UI State Variables
  customers: any[] = [];
  products: any[] = [];
  paymentMethods: any[] = [];
  searchTerm: string = '';
  searchResults: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.loadLookups();
  }

  /**
   * Loads lookup data for Customers and Payment Methods
   */
  loadLookups() {
    this.api.get<any[]>('Customer/active').subscribe(data => this.customers = data);
    this.api.get<any[]>('PaymentMethod').subscribe(data => this.paymentMethods = data);
  }

  /**
   * Searches for products by Name or SKU
   */
  searchProduct() {
    if (this.searchTerm.length < 2) {
      this.searchResults = [];
      return;
    }
    // Fetching products from API based on search term
    this.api.get<any[]>(`Product?search=${this.searchTerm}`).subscribe(data => {
      this.searchResults = data;
    });
  }

  /**
   * Adds a product to the sales items list
   */
  addProduct(product: any) {
    const existing = this.sale.salesDetails.find((d: any) => d.productId === product.productId);
    if (existing) {
      existing.quantity += 1;
      this.calculateLineTotal(existing);
    } else {
      this.sale.salesDetails.push({
        productId: product.productId,
        productName: product.productName,
        sku: product.sku,
        quantity: 1,
        uomId: product.uomId,
        uomName: product.uomName,
        unitPrice: product.salesPrice,
        discountPerUnit: 0,
        lineTotal: product.salesPrice
      });
    }
    this.searchTerm = '';
    this.searchResults = [];
    this.calculateTotals();
  }

  /**
   * Removes an item from the sales list
   */
  removeItem(index: number) {
    this.sale.salesDetails.splice(index, 1);
    this.calculateTotals();
  }

  /**
   * Calculates the total for a single row
   */
  calculateLineTotal(item: any) {
    item.lineTotal = item.quantity * (item.unitPrice - item.discountPerUnit);
    this.calculateTotals();
  }

  /**
   * Calculates grand totals for the entire sale
   */
  calculateTotals() {
    this.sale.totalAmount = this.sale.salesDetails.reduce((sum: number, item: any) => sum + item.lineTotal, 0);
    this.sale.netAmount = this.sale.totalAmount - this.sale.discountAmount;
    this.updatePaymentInfo();
  }

  /**
   * Updates due and change amounts based on paid amount
   */
  updatePaymentInfo() {
    if (this.sale.paidAmount >= this.sale.netAmount) {
      this.sale.changeAmount = this.sale.paidAmount - this.sale.netAmount;
      this.sale.dueAmount = 0;
      this.sale.paymentStatus = 'Paid';
    } else {
      this.sale.dueAmount = this.sale.netAmount - this.sale.paidAmount;
      this.sale.changeAmount = 0;
      this.sale.paymentStatus = this.sale.paidAmount > 0 ? 'Partial' : 'Due';
    }
  }

  /**
   * Submits the sale to the backend
   */
  onSubmit() {
    if (this.sale.salesDetails.length === 0) {
      alert('Please add at least one product');
      return;
    }

    this.isLoading = true;
    
    // Preparing the payment object for the API
    this.sale.payments = [{
      paymentMethodId: this.paymentMethods[0]?.paymentMethodId || 1, // Default to Cash
      amount: this.sale.paidAmount,
      paymentDate: this.sale.salesDate
    }];

    this.api.post('Sales', this.sale).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.printReceipt(res); // Print thermal receipt
        this.resetForm();
      },
      error: (err) => {
        alert(err.error?.message || 'Error processing sale');
        this.isLoading = false;
      }
    });
  }

  /**
   * Generates a thermal-friendly receipt and opens the browser's print dialog.
   */
  printReceipt(saleResponse: any) {
    const printContent = `
      <div style="font-family: 'Courier New', Courier, monospace; width: 80mm; padding: 10px; font-size: 12px;">
        <h2 style="text-align: center; margin-bottom: 5px;">PharmaCare</h2>
        <p style="text-align: center; margin: 0;">Multi-Branch Pharmacy System</p>
        <p style="text-align: center; margin: 0;">Dhaka, Bangladesh</p>
        <hr style="border-top: 1px dashed #000;">
        <p><strong>Invoice:</strong> ${saleResponse.invoiceNo || 'N/A'}</p>
        <p><strong>Date:</strong> ${new Date().toLocaleString()}</p>
        <p><strong>Customer:</strong> ${this.customers.find(c => c.customerId === this.sale.customerId)?.name || 'Walk-in'}</p>
        <hr style="border-top: 1px dashed #000;">
        <table style="width: 100%; border-collapse: collapse;">
          <thead>
            <tr>
              <th style="text-align: left;">Item</th>
              <th style="text-align: center;">Qty</th>
              <th style="text-align: right;">Total</th>
            </tr>
          </thead>
          <tbody>
            ${this.sale.salesDetails.map((item: any) => `
              <tr>
                <td>${item.productName}</td>
                <td style="text-align: center;">${item.quantity}</td>
                <td style="text-align: right;">৳${item.lineTotal.toFixed(2)}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
        <hr style="border-top: 1px dashed #000;">
        <div style="display: flex; justify-content: space-between;">
          <span>Sub Total:</span>
          <span>৳${this.sale.totalAmount.toFixed(2)}</span>
        </div>
        <div style="display: flex; justify-content: space-between;">
          <span>Discount:</span>
          <span>৳${this.sale.discountAmount.toFixed(2)}</span>
        </div>
        <div style="display: flex; justify-content: space-between; font-weight: bold;">
          <span>Net Total:</span>
          <span>৳${this.sale.netAmount.toFixed(2)}</span>
        </div>
        <div style="display: flex; justify-content: space-between;">
          <span>Paid:</span>
          <span>৳${this.sale.paidAmount.toFixed(2)}</span>
        </div>
        <div style="display: flex; justify-content: space-between;">
          <span>Change:</span>
          <span>৳${this.sale.changeAmount.toFixed(2)}</span>
        </div>
        <hr style="border-top: 1px dashed #000;">
        <p style="text-align: center; margin-top: 10px;">Thank you for shopping with us!</p>
        <p style="text-align: center; font-size: 10px;">Software by PharmaCare AI</p>
      </div>
    `;

    const printWindow = window.open('', '_blank', 'width=600,height=800');
    if (printWindow) {
      printWindow.document.write(`
        <html>
          <head><title>Print Receipt</title></head>
          <body onload="window.print();window.close()">${printContent}</body>
        </html>
      `);
      printWindow.document.close();
    }
  }

  /**
   * Resets the POS form for the next customer
   */
  resetForm() {
    this.sale = {
      salesDate: new Date().toISOString().split('T')[0],
      branchId: 1,
      customerId: null,
      totalAmount: 0,
      discountAmount: 0,
      netAmount: 0,
      paidAmount: 0,
      dueAmount: 0,
      changeAmount: 0,
      salesDetails: [],
      payments: []
    };
    this.searchTerm = '';
    this.isLoading = false;
  }
}
