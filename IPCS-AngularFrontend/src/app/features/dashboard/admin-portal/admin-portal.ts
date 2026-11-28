import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-admin-portal',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  template: `
    <div class="portal-container animate-fade-in">
      <div class="portal-header">
        <h1>{{ 'dashboard.admin.title' | translate }}</h1>
        <p>{{ 'dashboard.admin.subtitle' | translate }}</p>
      </div>

      <div class="portal-grid">
        <!-- Security & Access Section -->
        <div class="portal-card glass">
          <div class="card-icon blue">🛡️</div>
          <div class="card-content">
            <h3>{{ 'dashboard.admin.securityTitle' | translate }}</h3>
            <p>{{ 'dashboard.admin.securityDesc' | translate }}</p>
            <div class="card-actions">
              <a routerLink="/dashboard/administration/users" class="portal-btn">{{ 'dashboard.admin.userManagement' | translate }}</a>
              <a routerLink="/dashboard/administration/roles" class="portal-btn">{{ 'dashboard.admin.roleDefinitions' | translate }}</a>
            </div>
          </div>
        </div>

        <!-- Global Configuration Section -->
        <div class="portal-card glass">
          <div class="card-icon teal">⚙️</div>
          <div class="card-content">
            <h3>{{ 'dashboard.admin.configTitle' | translate }}</h3>
            <p>{{ 'dashboard.admin.configDesc' | translate }}</p>
            <div class="card-actions">
              <a routerLink="/dashboard/setup/branches" class="portal-btn">{{ 'nav.branches' | translate }}</a>
              <a routerLink="/dashboard/setup/categories" class="portal-btn">{{ 'nav.categories' | translate }}</a>
              <a routerLink="/dashboard/setup/uoms" class="portal-btn">{{ 'dashboard.admin.unitsUom' | translate }}</a>
            </div>
          </div>
        </div>

        <!-- Masters & Partners Section -->
        <div class="portal-card glass">
          <div class="card-icon amber">🤝</div>
          <div class="card-content">
            <h3>{{ 'dashboard.admin.mastersTitle' | translate }}</h3>
            <p>{{ 'dashboard.admin.mastersDesc' | translate }}</p>
            <div class="card-actions">
              <a routerLink="/dashboard/setup/manufacturers" class="portal-btn">{{ 'nav.manufacturers' | translate }}</a>
              <a routerLink="/dashboard/setup/suppliers" class="portal-btn">{{ 'nav.suppliers' | translate }}</a>
              <a routerLink="/dashboard/setup/customers" class="portal-btn">{{ 'nav.customers' | translate }}</a>
            </div>
          </div>
        </div>

        <!-- Navigation Section -->
        <div class="portal-card glass highlight">
          <div class="card-icon primary">📊</div>
          <div class="card-content">
            <h3>{{ 'dashboard.admin.opsTitle' | translate }}</h3>
            <p>{{ 'dashboard.admin.opsDesc' | translate }}</p>
            <div class="card-actions">
              <a routerLink="/dashboard/home" class="portal-btn primary">{{ 'dashboard.admin.goToDashboard' | translate }}</a>
            </div>
          </div>
        </div>
      </div>
    </div>

    <style>
      .portal-container { padding: 40px; max-width: 1200px; margin: 0 auto; }
      .portal-header { margin-bottom: 48px; text-align: center; }
      .portal-header h1 { font-size: 2.5rem; margin-bottom: 12px; color: var(--primary); }
      
      .portal-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(350px, 1fr)); gap: 32px; }
      .portal-card { 
        display: flex; gap: 24px; padding: 32px; border-radius: 24px;
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        border: 1px solid rgba(226, 232, 240, 0.8);
      }
      .portal-card:hover { transform: translateY(-8px); box-shadow: 0 20px 40px -15px rgba(0,0,0,0.1); border-color: var(--primary-light); }
      
      .card-icon { 
        width: 64px; height: 64px; border-radius: 20px; font-size: 32px;
        display: flex; align-items: center; justify-content: center;
        flex-shrink: 0;
      }
      .card-icon.blue { background: #e0f2fe; }
      .card-icon.teal { background: #f0fdf4; }
      .card-icon.amber { background: #fffbeb; }
      .card-icon.primary { background: #f0fdfa; }

      .portal-card.highlight { background: linear-gradient(135deg, #f0fdfa 0%, #ffffff 100%); border: 2px solid var(--primary-light); }

      .card-content h3 { font-size: 1.25rem; margin-bottom: 8px; }
      .card-content p { color: var(--text-muted); font-size: 0.95rem; margin-bottom: 24px; }
      
      .card-actions { display: flex; flex-direction: column; gap: 10px; }
      .portal-btn {
        padding: 10px 16px; border-radius: 12px; background: #f1f5f9;
        color: var(--text-main); text-decoration: none; font-weight: 600;
        font-size: 0.9rem; transition: all 0.2s;
        border: 1px solid transparent;
      }
      .portal-btn:hover { background: white; border-color: var(--primary); color: var(--primary); }
      .portal-btn.primary { background: var(--primary); color: white; margin-top: 10px; text-align: center; }
      .portal-btn.primary:hover { background: var(--primary-dark); }
    </style>
  `
})
export class AdminPortalComponent implements OnInit {
  constructor() {}
  ngOnInit() {}
}
