import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * SalesReturnComponent
 * Handles the return of products from customers.
 * Requires an existing invoice to validate return quantities and prices.
 */
@Component({
  selector: 'app-sales-return',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './sales-return.html'
})
export class SalesReturnComponent implements OnInit {
  // Main Return Object
  returnMaster: any = {
    returnDate: new Date().toISOString().split('T')[0],
    salesId: null,
    customerId: null,
    branchId: 1,
    totalAmount: 0,
    refundAmount: 0,
    refundType: 'Cash',
    paymentMethodId: 1,
    reason: '',
    returnDetails: []
  };

  // UI State
  invoiceSearch: string = '';
  originalInvoice: any = null;
  paymentMethods: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.api.get<any[]>('PaymentMethod').subscribe(data => this.paymentMethods = data);
  }

  /**
   * Searches for an invoice by number to initiate a return
   */
  findInvoice() {
    if (!this.invoiceSearch) return;
    this.isLoading = true;
    this.api.get<any[]>(`Sales?search=${this.invoiceSearch}`).subscribe({
      next: (data: any[]) => {
        const found = data.find(s => s.invoiceNo === this.invoiceSearch);
        if (found) {
          this.loadInvoiceDetails(found.salesId);
        } else {
          alert('Invoice not found');
          this.isLoading = false;
        }
      },
      error: () => { this.isLoading = false; }
    });
  }

  /**
   * Loads the full details of the invoice including items
   */
  loadInvoiceDetails(id: number) {
    this.api.get<any>(`Sales/${id}`).subscribe(data => {
      this.originalInvoice = data;
      this.returnMaster.salesId = data.salesId;
      this.returnMaster.customerId = data.customerId;
      
      // Map original items to returnable items
      this.returnMaster.returnDetails = data.salesDetails.map((d: any) => ({
        productId: d.productId,
        productName: d.productName,
        lotId: d.lotId,
        originalQty: d.quantity,
        quantity: 0, // Quantity to return
        uomId: d.uomId,
        uomName: d.uomName,
        unitPrice: d.unitPrice,
        lineTotal: 0
      }));
      this.isLoading = false;
    });
  }

  /**
   * Calculates the return total for a row
   */
  calculateLineTotal(item: any) {
    if (item.quantity > item.originalQty) {
      alert(`Cannot return more than purchased (${item.originalQty})`);
      item.quantity = item.originalQty;
    }
    item.lineTotal = item.quantity * item.unitPrice;
    this.calculateTotals();
  }

  /**
   * Calculates the grand total for the return
   */
  calculateTotals() {
    this.returnMaster.totalAmount = this.returnMaster.returnDetails.reduce((sum: number, item: any) => sum + item.lineTotal, 0);
    this.returnMaster.refundAmount = this.returnMaster.totalAmount;
  }

  /**
   * Submits the sales return to the API
   */
  onSubmit() {
    const itemsToReturn = this.returnMaster.returnDetails.filter((d: any) => d.quantity > 0);
    if (itemsToReturn.length === 0) {
      alert('Please specify quantity for at least one item to return');
      return;
    }

    this.isLoading = true;
    const payload = { ...this.returnMaster, returnDetails: itemsToReturn };
    
    this.api.post('SalesReturn', payload).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.router.navigate(['/dashboard/sales']);
      },
      error: (err) => {
        alert(err.error?.message || 'Return failed');
        this.isLoading = false;
      }
    });
  }
}
