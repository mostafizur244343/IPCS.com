import { Component, Input, Output, EventEmitter, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageCropperComponent } from '../image-cropper/image-cropper';

@Component({
  selector: 'app-image-upload',
  standalone: true,
  imports: [CommonModule, ImageCropperComponent],
  template: `
    <div 
      class="image-upload-container"
      [class.has-image]="imageUrl"
      (drop)="onDrop($event)"
      (dragover)="onDragOver($event)"
      (dragleave)="onDragLeave($event)"
    >
      <input 
        type="file" 
        #fileInput
        accept="image/*"
        (change)="onFileSelected($event)"
        class="file-input"
      >
      
      <div *ngIf="!imageUrl" class="upload-placeholder" (click)="triggerFileInput()">
        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
          <circle cx="8.5" cy="8.5" r="1.5"></circle>
          <polyline points="21 15 16 10 5 21"></polyline>
        </svg>
        <p class="upload-text">Click to upload or drag and drop</p>
        <p class="upload-hint">PNG, JPG or GIF (Max 5MB)</p>
      </div>
      
      <div *ngIf="imageUrl" class="image-preview">
        <img [src]="getImageUrl()" alt="Preview" (click)="openCropper($event)">
        <div class="image-actions">
          <button class="action-btn crop-btn" (click)="openCropper($event)" title="Crop">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M6.13 1L6 16a2 2 0 0 0 2 2h15"></path>
              <path d="M1 6.13L16 6a2 2 0 0 1 2 2v15"></path>
            </svg>
          </button>
          <button class="action-btn remove-btn" (click)="removeImage($event)" title="Remove">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>
      </div>
    </div>
    
    <app-image-cropper
      [imageSrc]="tempImageSrc"
      [showFullImageBtn]="isNewFile"
      (cropped)="onCropped($event)"
      (fullImage)="onFullImage()"
      (closed)="onCropperClosed()"
      #imageCropper
    ></app-image-cropper>
  `,
  styles: [`
    .image-upload-container {
      position: relative;
      border: 2px dashed #e2e8f0;
      border-radius: 12px;
      padding: 24px;
      text-align: center;
      cursor: pointer;
      transition: all 0.2s;
      background: #f8fafc;
    }

    .image-upload-container:hover {
      border-color: var(--primary, #0d9488);
      background: #f0f9ff;
    }

    .image-upload-container.has-image {
      padding: 0;
      border: 2px solid #e2e8f0;
    }

    .file-input {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      opacity: 0;
      cursor: pointer;
    }

    .upload-placeholder {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
      color: var(--text-muted, #64748b);
    }

    .upload-placeholder svg {
      color: var(--primary, #0d9488);
    }

    .upload-text {
      font-size: 14px;
      font-weight: 500;
      color: var(--text, #1e293b);
    }

    .upload-hint {
      font-size: 12px;
      color: var(--text-muted, #64748b);
    }

    .image-preview {
      position: relative;
      width: 100%;
      min-height: 200px;
    }

    .image-preview img {
      width: 100%;
      height: 250px;
      object-fit: cover;
      border-radius: 10px;
      display: block;
      cursor: pointer;
    }

    .image-actions {
      position: absolute;
      top: 8px;
      right: 8px;
      display: flex;
      gap: 8px;
      opacity: 0;
      transition: opacity 0.2s;
    }

    .image-preview:hover .image-actions {
      opacity: 1;
    }

    .action-btn {
      width: 36px;
      height: 36px;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s;
    }

    .crop-btn {
      background: rgba(59, 130, 246, 0.9);
      color: white;
    }

    .crop-btn:hover {
      background: #3b82f6;
      transform: scale(1.05);
    }

    .remove-btn {
      background: rgba(239, 68, 68, 0.9);
      color: white;
    }

    .remove-btn:hover {
      background: #ef4444;
      transform: scale(1.05);
    }
  `]
})
export class ImageUploadComponent {
  @Input() imageUrl: string | null = null;
  @Output() imageSelected = new EventEmitter<File>();
  @Output() imageRemoved = new EventEmitter<void>();
  @Output() imageCropped = new EventEmitter<string>();

  @ViewChild('fileInput') fileInput!: ElementRef;
  @ViewChild('imageCropper') imageCropper!: ImageCropperComponent;

  tempImageSrc: string = '';
  tempFile: File | null = null;
  isNewFile: boolean = false;

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    const files = target.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  triggerFileInput() {
    this.fileInput.nativeElement.click();
  }

  handleFile(file: File) {
    if (!file.type.startsWith('image/')) {
      alert('Please select an image file');
      return;
    }
    
    if (file.size > 5 * 1024 * 1024) {
      alert('File size must be less than 5MB');
      return;
    }

    this.tempFile = file;
    this.isNewFile = true;
    this.tempImageSrc = URL.createObjectURL(file);
    this.imageCropper.open(this.tempImageSrc);
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  openCropper(event: Event) {
    event.stopPropagation();
    if (this.imageUrl) {
      this.isNewFile = false;
      this.tempImageSrc = this.getImageUrl();
      this.imageCropper.open(this.tempImageSrc);
    }
  }

  onFullImage() {
    if (this.tempFile) {
      this.imageSelected.emit(this.tempFile);
      this.tempFile = null;
    }
  }

  onCropped(dataUrl: string) {
    this.imageCropped.emit(dataUrl);
    this.dataUrlToFile(dataUrl).then(file => {
      this.imageSelected.emit(file);
    });
  }

  onCropperClosed() {
    if (this.tempImageSrc.startsWith('blob:')) {
      URL.revokeObjectURL(this.tempImageSrc);
    }
  }

  removeImage(event: Event) {
    event.stopPropagation();
    this.imageRemoved.emit();
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  getImageUrl(): string {
    if (!this.imageUrl) return '';
    if (this.imageUrl.startsWith('http') || this.imageUrl.startsWith('data:')) {
      return this.imageUrl;
    }
    return `https://localhost:7054${this.imageUrl}`;
  }

  async dataUrlToFile(dataUrl: string): Promise<File> {
    const response = await fetch(dataUrl);
    const blob = await response.blob();
    return new File([blob], 'cropped-image.png', { type: 'image/png' });
  }
}
