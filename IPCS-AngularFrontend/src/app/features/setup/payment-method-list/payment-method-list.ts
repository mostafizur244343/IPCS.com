import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * PaymentMethodListComponent
 * Configures available payment options (e.g., Cash, Card, Mobile Banking).
 */
@Component({
  selector: 'app-payment-method-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-method-list.html'
})
export class PaymentMethodListComponent implements OnInit {
  methods: any[] = [];
  newMethod = { methodName: '', description: '' };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadMethods();
  }

  loadMethods() {
    this.isLoading = true;
    this.api.get<any[]>('PaymentMethod').subscribe({
      next: (data) => {
        this.methods = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  addMethod() {
    if (!this.newMethod.methodName) return;
    this.isLoading = true;
    this.api.post('PaymentMethod', this.newMethod).subscribe({
      next: () => {
        this.newMethod = { methodName: '', description: '' };
        this.loadMethods();
      },
      error: () => this.isLoading = false
    });
  }

  deleteMethod(id: number) {
    if (confirm('Delete this payment method?')) {
      this.api.delete(`PaymentMethod/${id}`).subscribe(() => this.loadMethods());
    }
  }
}
