import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';

/**
 * DashboardHomeComponent
 * Landing page displaying real-time business performance metrics.
 * Fetches data from DailySummary and Product modules.
 */
@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-home.html'
})
export class DashboardHomeComponent implements OnInit {
  stats: any[] = [];
  recentTransactions: any[] = [];
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  /**
   * Fetches data for stats and recent sales from multiple API endpoints
   */
  loadDashboardData() {
    this.isLoading = true;
    const branchId = 1;
    const today = new Date().toISOString().split('T')[0];

    // 1. Fetch Today's Summary
    this.api.get<any>(`DailySummary/by-date/${branchId}/${today}`).subscribe(data => {
      this.stats = [
        { name: 'Today Sales', value: `৳ ${data?.totalSales || 0}`, icon: 'trending-up', color: 'blue' },
        { name: 'Today Purchase', value: `৳ ${data?.totalPurchase || 0}`, icon: 'shopping-bag', color: 'teal' },
      ];
    });

    // 2. Fetch Inventory Stats
    this.api.get<any[]>('Product').subscribe(products => {
      const lowStock = products.filter(p => p.currentStock <= p.reorderLevel).length;
      this.stats.push({ name: 'Active Products', value: products.length, icon: 'package', color: 'amber' });
      this.stats.push({ name: 'Low Stock', value: lowStock, icon: 'alert-triangle', color: 'red' });
    });

    // 3. Fetch Recent Sales
    this.api.get<any[]>('Sales').subscribe(sales => {
      this.recentTransactions = sales.slice(0, 5).map(s => ({
        id: s.invoiceNo,
        customer: s.customerName,
        date: s.salesDate,
        amount: `৳ ${s.netAmount}`,
        status: s.paymentStatus
      }));
      this.isLoading = false;
    });
  }
}
