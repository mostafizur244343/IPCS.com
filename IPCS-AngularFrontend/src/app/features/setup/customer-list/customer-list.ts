import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

/**
 * CustomerListComponent
 * Manages pharmacy customers and their credit profiles.
 */
@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './customer-list.html'
})
export class CustomerListComponent implements OnInit {
  customers: any[] = [];
  newCustomer = { name: '', phone: '', email: '', address: '', initialBalance: 0 };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.isLoading = true;
    this.api.get<any[]>('Customer').subscribe({
      next: (data) => {
        this.customers = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  addCustomer() {
    if (!this.newCustomer.name) return;
    this.isLoading = true;
    this.api.post('Customer', this.newCustomer).subscribe({
      next: () => {
        this.newCustomer = { name: '', phone: '', email: '', address: '', initialBalance: 0 };
        this.loadCustomers();
      },
      error: () => this.isLoading = false
    });
  }

  deleteCustomer(id: number) {
    if (confirm('Delete this customer?')) {
      this.api.delete(`Customer/${id}`).subscribe(() => this.loadCustomers());
    }
  }
}
