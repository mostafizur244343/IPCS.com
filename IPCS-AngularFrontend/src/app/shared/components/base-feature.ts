import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { NotificationService } from '../../core/services/notification';
import { ApiService } from '../../core/services/api';
import Swal from 'sweetalert2';

@Component({
    template: ''
})
export abstract class BaseFeatureComponent {
    protected router = inject(Router);
    protected notification = inject(NotificationService);
    protected api = inject(ApiService);
    protected translate = inject(TranslateService);

    isLoading = false;

    ngOnInit(): void {}

    /**
     * Common alert for confirmation before deletion
     */
    async confirmDelete(title?: string, text?: string): Promise<boolean> {
        const result = await Swal.fire({
            title: title ?? this.translate.instant('notify.confirmTitle'),
            text: text ?? this.translate.instant('notify.confirmDeleteText'),
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: this.translate.instant('notify.yesDelete'),
            cancelButtonText: this.translate.instant('notify.cancel')
        });
        return result.isConfirmed;
    }

    /**
     * Standard error handler for components (optional if using Interceptor)
     */
    handleError(error: any) {
        this.isLoading = false;
        console.error('An error occurred:', error);
        // ErrorInterceptor already shows SweetAlert, but we can add extra logic here
    }
}
