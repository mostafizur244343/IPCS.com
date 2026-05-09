import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * CategoryListComponent
 * Manages product categories (e.g., Tablets, Syrups, Injections).
 * Includes inline creation and deletion functionality.
 */
@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-list.html'
})
export class CategoryListComponent implements OnInit {
  categories: any[] = [];
  newCategory = { categoryName: '', description: '' };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadCategories();
  }

  /**
   * Fetches the list of all active categories
   */
  loadCategories() {
    this.isLoading = true;
    this.api.get<any[]>('Category').subscribe({
      next: (data) => {
        this.categories = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Adds a new category to the system
   */
  addCategory() {
    if (!this.newCategory.categoryName) return;
    this.isLoading = true;
    this.api.post('Category', this.newCategory).subscribe({
      next: () => {
        this.newCategory = { categoryName: '', description: '' };
        this.loadCategories();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Removes a category from the system (soft delete)
   */
  deleteCategory(id: number) {
    if (confirm('Delete this category?')) {
      this.api.delete(`Category/${id}`).subscribe(() => this.loadCategories());
    }
  }
}
