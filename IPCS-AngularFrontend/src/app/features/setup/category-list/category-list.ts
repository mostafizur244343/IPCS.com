import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BaseFeatureComponent } from '../../../shared/components/base-feature';

/**
 * CategoryListComponent
 * Manages product categories (e.g., Tablets, Syrups, Injections).
 * Includes inline creation and deletion functionality.
 */
@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-list.html',
})
export class CategoryListComponent extends BaseFeatureComponent implements OnInit {
  categories: any[] = [];
  filteredCategories: any[] = [];
  newCategory = { categoryName: '', description: '' };
  showForm = false;
  searchTerm = '';

  // Editing state
  editingCategoryId: number | null = null;
  editCategory: any = {};

  // Details state
  selectedCategoryForDetails: any = null;

  override ngOnInit() {
    this.loadCategories();
  }

  showDetails(category: any) {
    this.selectedCategoryForDetails = category;
  }

  closeDetails() {
    this.selectedCategoryForDetails = null;
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredCategories = this.categories.filter(c => 
      (c.categoryName && c.categoryName.toLowerCase().includes(term)) ||
      (c.description && c.description.toLowerCase().includes(term))
    );
  }

  /**
   * Fetches the list of all active categories
   */
  loadCategories() {
    this.isLoading = true;
    this.api.get<any[]>('Category').subscribe({
      next: (data) => {
        this.categories = data;
        this.filteredCategories = data;
        this.isLoading = false;
      },
      error: (err) => this.handleError(err),
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
        this.notification.success('Category added successfully!');
        this.newCategory = { categoryName: '', description: '' };
        this.showForm = false;
        this.loadCategories();
      },
      error: (err) => this.handleError(err),
    });
  }

  /**
   * Starts editing a category
   */
  startEdit(category: any) {
    this.editingCategoryId = category.categoryId;
    this.editCategory = { ...category };
  }

  /**
   * Cancels editing
   */
  cancelEdit() {
    this.editingCategoryId = null;
    this.editCategory = {};
  }

  /**
   * Saves the updated category
   */
  updateCategory() {
    if (!this.editCategory.categoryName || !this.editingCategoryId) return;
    this.isLoading = true;
    this.api.put(`Category/${this.editingCategoryId}`, this.editCategory).subscribe({
      next: () => {
        this.notification.success('Category updated successfully!');
        this.cancelEdit();
        this.loadCategories();
      },
      error: (err) => this.handleError(err),
    });
  }

  /**
   * Removes a category from the system (soft delete)
   */
  async deleteCategory(id: number) {
    const confirmed = await this.confirmDelete('Delete Category?', 'This category will be removed.');
    if (confirmed) {
      this.isLoading = true;
      this.api.delete(`Category/${id}`).subscribe({
        next: () => {
          this.notification.success('Category deleted successfully!');
          this.loadCategories();
        },
        error: (err) => this.handleError(err)
      });
    }
  }
}

