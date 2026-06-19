import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * PurchaseListComponent
 * Displays a list of all purchase transactions made with suppliers.
 */
@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './purchase-list.html'
})
export class PurchaseListComponent implements OnInit {
  purchases: any[] = []; // Full list of purchase records
  filteredPurchases: any[] = [];
  searchTerm: string = '';
  isLoading = false;

  // Details state
  selectedPurchaseForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(purchase: any) {
    this.api.get<any>(`Purchase/${purchase.purchaseId}`).subscribe({
      next: (fullData) => {
        this.selectedPurchaseForDetails = fullData;
      },
      error: (err) => {
        console.error('Error loading purchase details', err);
        this.selectedPurchaseForDetails = purchase;
      }
    });
  }

  closeDetails() {
    this.selectedPurchaseForDetails = null;
  }

  ngOnInit() {
    this.loadPurchases();
  }

  /**
   * Fetches all active purchase records from the API
   */
  loadPurchases() {
    this.isLoading = true;
    this.api.get<any[]>('Purchase').subscribe({
      next: (data) => {
        this.purchases = data;
        this.filteredPurchases = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading purchases', err);
        this.isLoading = false;
      }
    });
  }

  /**
   * Filters the list by Supplier Invoice or Supplier Name
   */
  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredPurchases = this.purchases.filter(p => 
      p.supplierInvoiceNo?.toLowerCase().includes(term) || 
      p.supplierName?.toLowerCase().includes(term)
    );
  }

  /**
   * Deletes a purchase (Soft Delete)
   */
  deletePurchase(id: number) {
    if (confirm('Are you sure you want to delete this purchase? This will reduce stock levels.')) {
      this.api.delete(`Purchase/${id}`).subscribe(() => {
        this.loadPurchases();
      });
    }
  }

  /**
   * Receives a shipment for a pending purchase
   */
  receiveShipment(id: number) {
    if (confirm('Mark this shipment as received? This will update the inventory.')) {
      this.api.post(`Purchase/receive-shipment/${id}`, {}).subscribe(() => {
        this.loadPurchases();
      });
    }
  }
}
