import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * ReportsDashboardComponent
 * Provides high-level business analytics and summaries.
 * Includes monthly performance, inventory status, and alerts.
 */
@Component({
  selector: 'app-reports-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports-dashboard.html'
})
export class ReportsDashboardComponent implements OnInit {
  // Report data objects
  monthlyStats: any[] = [];
  inventoryStats: any = { totalItems: 0, lowStock: 0, outOfStock: 0 };
  recentActivities: any[] = [];
  
  selectedMonth: number = new Date().getMonth() + 1;
  selectedYear: number = new Date().getFullYear();
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadInventorySummary();
    this.loadMonthlyReport();
  }

  /**
   * Loads inventory status summary (low stock, out of stock counts)
   */
  loadInventorySummary() {
    this.api.get<any[]>('Product').subscribe(products => {
      this.inventoryStats.totalItems = products.length;
      this.inventoryStats.lowStock = products.filter(p => p.currentStock > 0 && p.currentStock <= p.reorderLevel).length;
      this.inventoryStats.outOfStock = products.filter(p => p.currentStock <= 0).length;
    });
  }

  /**
   * Fetches the monthly financial report for the selected period
   */
  loadMonthlyReport() {
    this.isLoading = true;
    const branchId = 1;
    this.api.get<any[]>(`DailySummary/monthly/${branchId}/${this.selectedYear}/${this.selectedMonth}`).subscribe({
      next: (data) => {
        this.monthlyStats = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Exports the current monthly data to a CSV file
   */
  exportToCSV() {
    if (this.monthlyStats.length === 0) return;

    const headers = ["Date", "Sales", "Purchase", "Expense", "Net Profit"];
    const rows = this.monthlyStats.map(d => [
      d.summaryDate,
      d.totalSales,
      d.totalPurchase,
      d.totalExpense,
      d.netProfit
    ]);

    let csvContent = "data:text/csv;charset=utf-8," 
      + headers.join(",") + "\n"
      + rows.map(e => e.join(",")).join("\n");

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `Report_${this.selectedMonth}_${this.selectedYear}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  /**
   * Utility to get Month Name for display
   */
  getMonthName(monthNum: number): string {
    const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    return months[monthNum - 1];
  }
}
