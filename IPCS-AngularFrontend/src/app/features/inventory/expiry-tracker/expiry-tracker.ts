import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

/**
 * ExpiryTrackerComponent
 * Identifies medicines approaching their expiration date.
 * Helps pharmacists clear stock and prevent selling expired drugs.
 */
@Component({
  selector: 'app-expiry-tracker',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './expiry-tracker.html'
})
export class ExpiryTrackerComponent implements OnInit {
  expiredItems: any[] = [];
  monthsThreshold: number = 6; // Default to showing items expiring in next 6 months
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadExpiringItems();
  }

  /**
   * Fetches lots from the API and filters them by expiry date.
   */
  loadExpiringItems() {
    this.isLoading = true;
    this.api.get<any[]>('LotInfo').subscribe({
      next: (data) => {
        const today = new Date();
        const thresholdDate = new Date();
        thresholdDate.setMonth(today.getMonth() + this.monthsThreshold);

        // Filter lots that expire before the threshold
        this.expiredItems = data.filter(lot => {
          const expiry = new Date(lot.expiryDate);
          return expiry <= thresholdDate && lot.isActive;
        }).sort((a, b) => new Date(a.expiryDate).getTime() - new Date(b.expiryDate).getTime());
        
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Utility to check if an item is already expired
   */
  isExpired(date: string): boolean {
    return new Date(date) < new Date();
  }
}
