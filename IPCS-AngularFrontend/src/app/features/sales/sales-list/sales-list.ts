import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * SalesListComponent
 * Used to display a list of all sales invoices/transactions.
 */
@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './sales-list.html'
})
export class SalesListComponent implements OnInit {
  sales: any[] = []; // List of sales invoices
  filteredSales: any[] = [];
  searchTerm: string = '';
  isLoading = false;

  // Details state
  selectedSalesForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(sale: any) {
    this.api.get<any>(`Sales/${sale.salesId}`).subscribe({
      next: (fullData) => {
        this.selectedSalesForDetails = fullData;
      },
      error: (err) => {
        console.error('Error loading sales details', err);
        this.selectedSalesForDetails = sale;
      }
    });
  }

  closeDetails() {
    this.selectedSalesForDetails = null;
  }

  ngOnInit() {
    this.loadSales();
  }

  /**
   * Fetches all sales records from the API
   */
  loadSales() {
    this.isLoading = true;
    this.api.get<any[]>('Sales').subscribe({
      next: (data) => {
        this.sales = data;
        this.filteredSales = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading sales', err);
        this.isLoading = false;
      }
    });
  }

  /**
   * Filters sales by Invoice Number or Customer Name
   */
  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredSales = this.sales.filter(s => 
      s.invoiceNo?.toLowerCase().includes(term) || 
      s.customerName?.toLowerCase().includes(term)
    );
  }

  /**
   * Deletes a sale (Soft Delete)
   */
  deleteSale(id: number) {
    if (confirm('Are you sure you want to delete this sale? This will revert stock levels.')) {
      this.api.delete(`Sales/${id}`).subscribe(() => {
        this.loadSales();
      });
    }
  }
}
