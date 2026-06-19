import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * DashboardHomeComponent
 * Landing page displaying real-time business performance metrics.
 * Fetches data from DailySummary and Product modules.
 */
@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-home.html'
})
export class DashboardHomeComponent implements OnInit {
  userName: string = 'User';
  currentTime: Date = new Date();
  isLoading = false;
  
  stats = [
    { name: 'Today\'s Sales', value: '৳0.00', icon: 'trending-up', color: 'blue', delta: '+0%' },
    { name: 'Total Products', value: '0', icon: 'package', color: 'teal' },
    { name: 'Low Stock', value: '0', icon: 'alert-triangle', color: 'amber' },
    { name: 'Out of Stock', value: '0', icon: 'shopping-bag', color: 'red' }
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
    // Load Inventory Stats
    this.api.get<any>('DailySummary/inventory-stats').subscribe({
      next: (data) => {
        this.stats[1].value = data.totalItems.toString();
        this.stats[2].value = data.lowStock.toString();
        this.stats[3].value = data.outOfStock.toString();
      }
    });

    // Load Sales for Today and Recent Transactions
    this.api.get<any[]>('Sales').subscribe({
      next: (sales) => {
        const todayStr = new Date().toDateString();
        const todaySales = sales.filter(s => new Date(s.salesDate).toDateString() === todayStr);
        const total = todaySales.reduce((acc, curr) => acc + curr.netAmount, 0);
        this.stats[0].value = `৳${total.toFixed(2)}`;
        
        this.recentTransactions = sales.slice(0, 5).map(s => ({
          id: s.invoiceNo,
          customer: s.customerName || 'Walk-in',
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
