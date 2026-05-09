import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * PurchaseReturnComponent
 * Handles the return of products to suppliers.
 * Used for returning damaged, near-expiry, or excess stock.
 */
@Component({
  selector: 'app-purchase-return',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './purchase-return.html'
})
export class PurchaseReturnComponent implements OnInit {
  // Main Purchase Return Object
  returnMaster: any = {
    returnDate: new Date().toISOString().split('T')[0],
    purchaseId: null,
    supplierId: null,
    branchId: 1,
    totalAmount: 0,
    refundAmount: 0,
    refundType: 'Cash',
    paymentMethodId: 1,
    reason: '',
    returnDetails: []
  };

  // UI State
  purchaseSearch: string = '';
  originalPurchase: any = null;
  paymentMethods: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.api.get<any[]>('PaymentMethod').subscribe(data => this.paymentMethods = data);
  }

  /**
   * Searches for a purchase invoice to start a return
   */
  findPurchase() {
    if (!this.purchaseSearch) return;
    this.isLoading = true;
    this.api.get<any[]>(`Purchase?search=${this.purchaseSearch}`).subscribe({
      next: (data: any[]) => {
        const found = data.find(p => p.supplierInvoiceNo === this.purchaseSearch);
        if (found) {
          this.loadPurchaseDetails(found.purchaseId);
        } else {
          alert('Purchase Invoice not found');
          this.isLoading = false;
        }
      },
      error: () => { this.isLoading = false; }
    });
  }

  /**
   * Loads full purchase details for returning items
   */
  loadPurchaseDetails(id: number) {
    this.api.get<any>(`Purchase/${id}`).subscribe(data => {
      this.originalPurchase = data;
      this.returnMaster.purchaseId = data.purchaseId;
      this.returnMaster.supplierId = data.supplierId;
      
      // Preparing returnable items from the original purchase list
      this.returnMaster.returnDetails = data.purchaseDetails.map((d: any) => ({
        productId: d.productId,
        productName: d.productName,
        lotId: d.lotId,
        originalQty: d.qty,
        quantity: 0, // Quantity being returned
        uomId: d.uomId,
        uomName: d.uomName,
        purchasePrice: d.purchasePrice,
        lineTotal: 0,
        fromDamagedPool: false
      }));
      this.isLoading = false;
    });
  }

  /**
   * Calculates row totals and updates the main summary
   */
  calculateLineTotal(item: any) {
    if (item.quantity > item.originalQty) {
      alert(`Return quantity cannot exceed purchased quantity (${item.originalQty})`);
      item.quantity = item.originalQty;
    }
    item.lineTotal = item.quantity * item.purchasePrice;
    this.calculateTotals();
  }

  /**
   * Updates grand totals for the entire return transaction
   */
  calculateTotals() {
    this.returnMaster.totalAmount = this.returnMaster.returnDetails.reduce((sum: number, item: any) => sum + item.lineTotal, 0);
    this.returnMaster.refundAmount = this.returnMaster.totalAmount;
  }

  /**
   * Submits the return data to the backend API
   */
  onSubmit() {
    const itemsToReturn = this.returnMaster.returnDetails.filter((d: any) => d.quantity > 0);
    if (itemsToReturn.length === 0) {
      alert('Please specify quantity for at least one item');
      return;
    }

    this.isLoading = true;
    const payload = { ...this.returnMaster, returnDetails: itemsToReturn };
    
    this.api.post('PurchaseReturn', payload).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.router.navigate(['/dashboard/purchase']);
      },
      error: (err) => {
        alert(err.error?.message || 'Purchase Return failed');
        this.isLoading = false;
      }
    });
  }
}
