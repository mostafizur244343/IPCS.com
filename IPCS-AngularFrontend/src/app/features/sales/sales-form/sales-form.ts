import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import Swal from 'sweetalert2';

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
    branchId: null, // Selected branch
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
    payments: [],
    selectedPaymentMethodId: null,
    extraChargeAmount: 0
  };

  // UI State Variables
  customers: any[] = [];
  branches: any[] = []; // New Branch Lookup
  paymentMethods: any[] = [];
  searchTerm: string = '';
  searchResults: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.loadLookups();
  }

  /**
   * Loads lookup data for Customers, Branches and Payment Methods
   */
  loadLookups() {
    this.api.get<any[]>('Customer/active').subscribe(data => this.customers = data);
    
    this.api.get<any[]>('Branch').subscribe(data => {
      this.branches = data;
      if (this.branches.length > 0) {
        this.sale.branchId = this.branches[0].branchId;
      }
    });

    this.api.get<any[]>('PaymentMethod').subscribe(data => {
      this.paymentMethods = data;
      if (this.paymentMethods.length > 0) {
        this.sale.selectedPaymentMethodId = this.paymentMethods[0].paymentMethodId;
      }
    });
  }

  /**
   * Called when branch is changed. Resets current items as stock is branch-specific.
   */
  onBranchChange() {
    if (this.sale.salesDetails.length > 0) {
      if (confirm('Changing branch will reset your current items. Continue?')) {
        this.sale.salesDetails = [];
        this.calculateTotals();
      }
    }
  }

  /**
   * Calculates extra charge based on selected payment method
   */
  calculateExtraCharge() {
    if (!this.sale.selectedPaymentMethodId || this.sale.paidAmount <= 0) {
      this.sale.extraChargeAmount = 0;
      return;
    }
    
    const paymentMethod = this.paymentMethods.find(m => m.paymentMethodId === this.sale.selectedPaymentMethodId);
    if (paymentMethod && paymentMethod.isDigital && paymentMethod.extraChargePercentage) {
      this.sale.extraChargeAmount = (this.sale.paidAmount * paymentMethod.extraChargePercentage) / 100;
    } else {
      this.sale.extraChargeAmount = 0;
    }
  }

  /**
   * Searches for products by Name or SKU filtering by selected branch stock
   */
  searchProduct() {
    if (this.searchTerm.length < 2) {
      this.searchResults = [];
      return;
    }
    
    if (!this.sale.branchId) {
      alert('Please select a branch first');
      this.searchTerm = '';
      return;
    }

    // Fetching products from API based on search term and branchId
    this.api.get<any[]>(`Product?search=${this.searchTerm}&branchId=${this.sale.branchId}`).subscribe(data => {
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
      const newItem = {
        productId: product.productId || product.ProductId,
        productName: product.productName || product.ProductName,
        sku: product.sku || product.SKU,
        quantity: 1,
        uomId: product.uomId || product.UOMId || product.baseUOMId || product.BaseUOMId || 1,
        uomName: product.uomName || product.UOMName || product.baseUOMName || product.BaseUOMName || 'Unit',
        unitPrice: product.salesPrice || product.SalesPrice,
        discountPerUnit: 0,
        lineTotal: product.salesPrice || product.SalesPrice,
        lotId: null,
        availableLots: [] // To store lots for this specific product in the selected branch
      };
      this.sale.salesDetails.push(newItem);
      this.loadLots(newItem);
    }
    this.searchTerm = '';
    this.searchResults = [];
    this.calculateTotals();
  }

  /**
   * Fetches lots for a specific item based on selected branch
   */
  loadLots(item: any) {
    const productId = item.productId || item.ProductId;
    this.api.get<any[]>(`BranchLotStock/product/${productId}/branch/${this.sale.branchId}`).subscribe(data => {
      item.availableLots = data;
      if (data.length > 0) {
        item.lotId = data[0].lotId || data[0].LotId; // Default to first available lot
      }
    });
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
    this.calculateExtraCharge();
    
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

    // Ensure all items have a lot selected
    const missingLot = this.sale.salesDetails.find((d: any) => !d.lotId);
    if (missingLot) {
      alert(`Please select a lot for ${missingLot.productName}`);
      return;
    }

    this.isLoading = true;
    
    // Preparing the payment object for the API
    this.sale.payments = [{
      paymentMethodId: this.sale.selectedPaymentMethodId || this.paymentMethods[0]?.paymentMethodId || 1,
      amount: this.sale.paidAmount,
      paymentDate: this.sale.salesDate
    }];

    console.log('Submitting Sale:', this.sale);
    this.api.post('Sales', this.sale).subscribe({
      next: (res: any) => {
        Swal.fire({
          icon: 'success',
          title: 'Sale Completed',
          text: res.message || 'Invoice generated successfully!',
          timer: 2000,
          showConfirmButton: false
        });
        this.printReceipt(res); // Print thermal receipt
        this.resetForm();
      },
      error: (err) => {
        console.error('Full Error Trace:', err);
        let errorMsg = 'Error processing sale';
        
        if (err.error) {
          if (typeof err.error === 'string') {
            errorMsg = err.error;
          } else if (err.error.errors) {
            errorMsg = Object.keys(err.error.errors)
              .map(key => `${key}: ${err.error.errors[key].join(', ')}`)
              .join('\n');
          } else {
            errorMsg = err.error.message || err.error.Message || 'Insufficient stock or data inconsistency';
          }
        }
        
        Swal.fire({
          icon: 'error',
          title: 'Transaction failed',
          text: errorMsg,
          footer: 'Please check stock availability and customer selection.'
        });
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
        ${this.sale.extraChargeAmount > 0 ? `
        <div style="display: flex; justify-content: space-between; color: #ef4444;">
          <span>Extra Charge:</span>
          <span>৳${this.sale.extraChargeAmount.toFixed(2)}</span>
        </div>
        ` : ''}
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
      branchId: this.branches.length > 0 ? this.branches[0].branchId : null,
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
      payments: [],
      selectedPaymentMethodId: this.paymentMethods.length > 0 ? this.paymentMethods[0].paymentMethodId : null,
      extraChargeAmount: 0
    };
    this.searchTerm = '';
    this.isLoading = false;
  }
}
