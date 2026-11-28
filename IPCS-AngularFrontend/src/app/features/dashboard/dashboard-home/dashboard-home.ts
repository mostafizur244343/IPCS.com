import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../../core/services/api';

/**
 * DashboardHomeComponent
 * Landing page displaying real-time business performance metrics.
 * Fetches data from DailySummary and Product modules.
 */
@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './dashboard-home.html'
})
export class DashboardHomeComponent implements OnInit {
  userName: string = 'User';
  currentTime: Date = new Date();
  isLoading = false;
  
  // `name` holds an i18n key; the template renders it through the translate pipe.
  stats = [
    { name: 'dashboard.home.todaySales', value: '৳0.00', icon: 'trending-up', color: 'blue', delta: '+0%' },
    { name: 'dashboard.home.totalProducts', value: '0', icon: 'package', color: 'teal' },
    { name: 'dashboard.home.lowStock', value: '0', icon: 'alert-triangle', color: 'amber' },
    { name: 'dashboard.home.outOfStock', value: '0', icon: 'shopping-bag', color: 'red' }
  ];

  recentTransactions: any[] = [];

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadUserData();
    this.loadStats();
    this.startClock();
  }

  loadUserData() {
    const data = localStorage.getItem('user_data');
    if (data) {
      const user = JSON.parse(data);
      // Support both camelCase and PascalCase
      this.userName = user.name || user.Name || user.fullName || user.FullName || 'User';
    }
  }

  startClock() {
    setInterval(() => {
      this.currentTime = new Date();
    }, 1000);
  }

  loadStats() {
    this.isLoading = true;

    // The day, as a [from, to) UTC range for the browser's local day, so
    // "today's sales" matches what the old client-side filter computed.
    const now = new Date();
    const dayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0);
    const dayEnd = new Date(dayStart);
    dayEnd.setDate(dayEnd.getDate() + 1);
    const from = encodeURIComponent(dayStart.toISOString());
    const to = encodeURIComponent(dayEnd.toISOString());

    // Both calls fire in parallel; the sales call is the lightweight
    // Sales/recent endpoint (today's total + 5 recent sales) instead of
    // downloading the entire Sales table.
    forkJoin({
      stats: this.api.get<any>('DailySummary/inventory-stats'),
      sales: this.api.get<any>(`Sales/recent?from=${from}&to=${to}`)
    }).subscribe({
      next: ({ stats, sales }) => {
        this.stats[1].value = stats.totalItems.toString();
        this.stats[2].value = stats.lowStock.toString();
        this.stats[3].value = stats.outOfStock.toString();

        this.stats[0].value = `৳${Number(sales.todaySalesTotal || 0).toFixed(2)}`;

        this.recentTransactions = (sales.recentSales || []).map((s: any) => ({
          id: s.invoiceNo,
          // null customer falls back to the translated 'Walk-in' label in the template
          customer: s.customerName || null,
          date: new Date(s.salesDate).toLocaleDateString(),
          amount: `৳${s.netAmount.toFixed(2)}`,
          status: s.dueAmount > 0 ? 'Due' : 'Paid'
        }));
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }
}
