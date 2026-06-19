import { Component, Input, Output, EventEmitter, OnInit, ElementRef, ViewChild, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-image-cropper',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="cropper-overlay" *ngIf="isOpen" (click)="onOverlayClick($event)">
      <div class="cropper-container" (click)="$event.stopPropagation()">
        <div class="cropper-header">
          <h3>Crop & Edit Image</h3>
          <button type="button" class="close-btn" (click)="close()">&times;</button>
        </div>
        
        <div class="cropper-content" #cropperContent>
          <div 
            class="image-wrapper" 
            #imageWrapper
            [class.pan-mode]="mode === 'pan'"
            [class.draw-mode]="mode === 'draw'"
            [style.transform]="wrapperTransform"
            (mousedown)="onMouseDown($event)"
          >
            <img 
              #originalImage
              [src]="imageSrc" 
              crossorigin="anonymous"
              (load)="onImageLoad()"
              alt="Image to crop"
              draggable="false"
            />
            <canvas 
              #drawCanvas 
              class="draw-canvas"
            ></canvas>
            <div 
              class="crop-box" 
              *ngIf="showCropBox" 
              [style.left.px]="cropBox.x" 
              [style.top.px]="cropBox.y" 
              [style.width.px]="cropBox.w" 
              [style.height.px]="cropBox.h"
            ></div>
          </div>
        </div>
        
        <div class="cropper-controls">
          <div class="control-group mode-toggle">
            <button type="button" class="control-btn" [class.active]="mode === 'crop'" (click)="setMode('crop')" title="Draw Crop Box">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M6.13 1L6 16a2 2 0 0 0 2 2h15"></path><path d="M1 6.13L16 6a2 2 0 0 1 2 2v15"></path></svg>
              Crop
            </button>
            <button type="button" class="control-btn" [class.active]="mode === 'pan'" (click)="setMode('pan')" title="Move Image">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="5 9 2 12 5 15"></polyline><polyline points="9 5 12 2 15 5"></polyline><polyline points="19 9 22 12 19 15"></polyline><polyline points="9 19 12 22 15 19"></polyline><line x1="2" y1="12" x2="22" y2="12"></line><line x1="12" y1="2" x2="12" y2="22"></line></svg>
              Move
            </button>
            <button type="button" class="control-btn" [class.active]="mode === 'draw'" (click)="setMode('draw')" title="Draw on Image">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 19l7-7 3 3-7 7-3-3z"></path><path d="M18 13l-1.5-7.5L2 2l3.5 14.5L13 18l5-5z"></path><path d="M2 2l7.586 7.586"></path><circle cx="11" cy="11" r="2"></circle></svg>
              Draw
            </button>
          </div>

          <div class="control-group" *ngIf="mode === 'draw'">
            <input type="color" [(ngModel)]="drawColor" class="color-picker" title="Pen Color"/>
            <input type="range" min="1" max="20" step="1" [(ngModel)]="drawSize" style="width: 80px;" title="Pen Size"/>
            <button type="button" class="control-btn border-btn" (click)="clearDrawings()">Clear</button>
          </div>

          <div class="control-group" *ngIf="mode !== 'draw'">
            <label>Zoom</label>
            <input type="range" min="0.1" max="3" step="0.1" [(ngModel)]="zoom" />
            <span>{{ (zoom * 100).toFixed(0) }}%</span>
          </div>

          <div class="control-group" *ngIf="mode !== 'draw'">
            <label>Rotate</label>
            <button type="button" class="control-btn border-btn" (click)="rotate(-90)">-90&deg;</button>
            <button type="button" class="control-btn border-btn" (click)="rotate(90)">+90&deg;</button>
          </div>
          
          <div class="control-group">
            <button type="button" class="btn btn-secondary btn-sm" (click)="reset()">Reset All</button>
          </div>
        </div>
        
        <div class="cropper-actions">
          <div>
            <button type="button" *ngIf="showCropBox" class="btn btn-warning" (click)="undoCrop()">Clear Crop Box</button>
          </div>
          <div style="display: flex; gap: 12px;">
            <button type="button" class="btn btn-secondary" (click)="close()">Cancel</button>
            <button type="button" *ngIf="showFullImageBtn" class="btn btn-secondary" (click)="uploadFull()">Upload Full</button>
            <button type="button" class="btn btn-primary" (click)="crop()" [disabled]="!showCropBox && mode === 'crop'">Save</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .cropper-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.8);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
      padding: 20px;
    }

    .cropper-container {
      background: white;
      border-radius: 12px;
      max-width: 950px;
      width: 100%;
      max-height: 90vh;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
    }

    .cropper-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px;
      border-bottom: 1px solid #e2e8f0;
      background: #f8fafc;
    }

    .cropper-header h3 {
      margin: 0;
      font-size: 18px;
      font-weight: 600;
      color: #1e293b;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #64748b;
      line-height: 1;
      padding: 0;
    }
    .close-btn:hover { color: #0f172a; }

    .cropper-content {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #e2e8f0;
      min-height: 300px;
      overflow: hidden;
      position: relative;
    }

    .image-wrapper {
      position: relative;
      display: inline-block;
      transform-origin: center center;
      cursor: crosshair;
      user-select: none;
    }
    .image-wrapper.pan-mode {
      cursor: grab;
    }
    .image-wrapper.pan-mode:active {
      cursor: grabbing;
    }
    .image-wrapper.draw-mode {
      cursor: crosshair;
    }

    .image-wrapper img {
      max-width: 800px;
      max-height: 60vh;
      display: block;
      pointer-events: none;
    }

    .draw-canvas {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      pointer-events: none;
      z-index: 5;
    }

    .crop-box {
      position: absolute;
      border: 2px dashed #ffffff;
      box-shadow: 0 0 0 9999px rgba(0, 0, 0, 0.5);
      background: rgba(255, 255, 255, 0.1);
      pointer-events: none; 
      z-index: 10;
    }

    .cropper-controls {
      display: flex;
      flex-wrap: wrap;
      gap: 20px;
      padding: 16px 24px;
      border-top: 1px solid #e2e8f0;
      background: #f8fafc;
      align-items: center;
    }

    .control-group {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .mode-toggle {
      background: #e2e8f0;
      padding: 4px;
      border-radius: 8px;
      gap: 4px;
    }

    .control-btn {
      padding: 6px 12px;
      border: none;
      background: transparent;
      border-radius: 6px;
      cursor: pointer;
      font-size: 13px;
      font-weight: 600;
      color: #475569;
      display: flex;
      align-items: center;
      gap: 6px;
      transition: all 0.2s;
    }
    .control-btn.border-btn {
      border: 1px solid #cbd5e1;
      background: white;
    }
    .control-btn.active {
      background: white;
      color: #0d9488;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    .control-btn:hover:not(.active) {
      background: rgba(255,255,255,0.5);
    }

    .color-picker {
      width: 32px;
      height: 32px;
      padding: 0;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }

    .control-group label {
      font-size: 13px;
      font-weight: 600;
      color: #475569;
    }

    .control-group span {
      font-size: 13px;
      font-weight: 600;
      color: #1e293b;
      min-width: 40px;
    }

    .cropper-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px;
      border-top: 1px solid #e2e8f0;
      background: white;
    }

    .btn {
      padding: 8px 20px;
      border: none;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }
    .btn-sm {
      padding: 6px 12px;
      font-size: 13px;
    }

    .btn-primary {
      background: var(--primary, #0d9488);
      color: white;
    }
    .btn-primary:hover:not(:disabled) { background: #0f766e; }
    .btn-primary:disabled { background: #94a3b8; cursor: not-allowed; opacity: 0.7; }

    .btn-secondary {
      background: #e2e8f0;
      color: #475569;
    }
    .btn-secondary:hover { background: #cbd5e1; }
    
    .btn-warning {
      background: #fef08a;
      color: #854d0e;
    }
    .btn-warning:hover { background: #fde047; }
  `]
})
export class ImageCropperComponent implements OnInit {
  @Input() imageSrc: string = '';
  @Input() showFullImageBtn: boolean = false;
  @Output() cropped = new EventEmitter<string>();
  @Output() fullImage = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  @ViewChild('originalImage') originalImage!: ElementRef<HTMLImageElement>;
  @ViewChild('imageWrapper') imageWrapper!: ElementRef<HTMLDivElement>;
  @ViewChild('drawCanvas') drawCanvas!: ElementRef<HTMLCanvasElement>;

  isOpen = false;
  
  // Transform State
  zoom = 1;
  panX = 0;
  panY = 0;
  rotation = 0;
  
  // Interaction State
  mode: 'crop' | 'pan' | 'draw' = 'crop';
  isDragging = false;
  
  // Drawing state
  drawColor = '#ef4444'; // default red
  drawSize = 4;
  hasDrawings = false;
  private drawCtx!: CanvasRenderingContext2D;
  
  // To handle dragging outside the container reliably with rotation support
  startXLocal = 0;
  startYLocal = 0;
  startClientX = 0;
  startClientY = 0;

  // Crop Box State (relative to image wrapper)
  showCropBox = false;
  cropBox = { x: 0, y: 0, w: 0, h: 0 };

  originalImageElement!: HTMLImageElement;

  get wrapperTransform(): string {
    return `translate(${this.panX}px, ${this.panY}px) scale(${this.zoom}) rotate(${this.rotation}deg)`;
  }

  ngOnInit() {}

  open(imageSrc: string) {
    this.imageSrc = imageSrc;
    this.isOpen = true;
    this.reset();
  }

  onOverlayClick(event: MouseEvent) {
    this.close();
  }

  close() {
    this.isOpen = false;
    this.closed.emit();
  }

  uploadFull() {
    if (this.originalImageElement && (this.rotation !== 0 || this.hasDrawings)) {
      // If there are modifications, perform a full-screen crop to bake them in
      this.cropBox = { 
        x: 0, 
        y: 0, 
        w: this.originalImageElement.width, 
        h: this.originalImageElement.height 
      };
      this.showCropBox = true;
      this.crop();
    } else {
      this.fullImage.emit();
      this.close();
    }
  }

  setMode(newMode: 'crop' | 'pan' | 'draw') {
    this.mode = newMode;
  }

  rotate(degrees: number) {
    this.rotation = (this.rotation + degrees) % 360;
  }

  undoCrop() {
    this.showCropBox = false;
    this.cropBox = { x: 0, y: 0, w: 0, h: 0 };
  }

  clearDrawings() {
    if (this.drawCtx) {
      this.drawCtx.clearRect(0, 0, this.drawCanvas.nativeElement.width, this.drawCanvas.nativeElement.height);
    }
    this.hasDrawings = false;
  }

  reset() {
    this.zoom = 1;
    this.panX = 0;
    this.panY = 0;
    this.rotation = 0;
    this.undoCrop();
    this.clearDrawings();
    this.mode = 'crop';
  }

  onImageLoad() {
    const img = this.originalImage.nativeElement;
    this.originalImageElement = img;
    
    // Initialize the drawing canvas to precisely match the rendered image
    const canvas = this.drawCanvas.nativeElement;
    canvas.width = img.width;
    canvas.height = img.height;
    
    this.drawCtx = canvas.getContext('2d')!;
    this.hasDrawings = false;
  }

  // Maps screen movement (delta X/Y) to local coordinate movement based on rotation
  getLocalDeltas(screenDx: number, screenDy: number): { x: number, y: number } {
    const rot = ((this.rotation % 360) + 360) % 360;
    if (rot === 90) return { x: screenDy, y: -screenDx };
    if (rot === 180) return { x: -screenDx, y: -screenDy };
    if (rot === 270) return { x: -screenDy, y: screenDx };
    return { x: screenDx, y: screenDy };
  }

  // --- Mouse Events for Drawing, Cropping & Panning ---

  onMouseDown(e: MouseEvent) {
    if (e.button !== 0) return; // Only process left click
    e.preventDefault();
    this.isDragging = true;
    
    // Record starting local coords (relative to the unscaled wrapper)
    this.startXLocal = e.offsetX;
    this.startYLocal = e.offsetY;
    
    // Record starting screen coords to track movement robustly outside bounds
    this.startClientX = e.clientX;
    this.startClientY = e.clientY;
    
    if (this.mode === 'crop') {
      this.cropBox = { x: this.startXLocal, y: this.startYLocal, w: 0, h: 0 };
      this.showCropBox = true;
    } else if (this.mode === 'draw') {
      this.hasDrawings = true;
      this.drawCtx.lineCap = 'round';
      this.drawCtx.lineJoin = 'round';
      this.drawCtx.strokeStyle = this.drawColor;
      this.drawCtx.lineWidth = this.drawSize;
      
      this.drawCtx.beginPath();
      this.drawCtx.moveTo(this.startXLocal, this.startYLocal);
      this.drawCtx.lineTo(this.startXLocal, this.startYLocal);
      this.drawCtx.stroke();
    }
  }

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(e: MouseEvent) {
    if (!this.isDragging) return;

    // Raw screen deltas scaled by zoom
    const dx = (e.clientX - this.startClientX) / this.zoom;
    const dy = (e.clientY - this.startClientY) / this.zoom;

    if (this.mode === 'crop') {
      const deltas = this.getLocalDeltas(dx, dy);
      let currentX = this.startXLocal + deltas.x;
      let currentY = this.startYLocal + deltas.y;
      
      this.cropBox.x = Math.min(this.startXLocal, currentX);
      this.cropBox.y = Math.min(this.startYLocal, currentY);
      this.cropBox.w = Math.abs(currentX - this.startXLocal);
      this.cropBox.h = Math.abs(currentY - this.startYLocal);
      
    } else if (this.mode === 'draw') {
      const deltas = this.getLocalDeltas(dx, dy);
      let currentX = this.startXLocal + deltas.x;
      let currentY = this.startYLocal + deltas.y;

      this.drawCtx.lineTo(currentX, currentY);
      this.drawCtx.stroke();
      
      // Update local start to current for the next continuous stroke segment
      this.startXLocal = currentX;
      this.startYLocal = currentY;
      this.startClientX = e.clientX;
      this.startClientY = e.clientY;
      
    } else if (this.mode === 'pan') {
      this.panX += dx;
      this.panY += dy;
      this.startClientX = e.clientX;
      this.startClientY = e.clientY;
    }
  }

  @HostListener('document:mouseup', ['$event'])
  onMouseUp(e: MouseEvent) {
    if (!this.isDragging) return;
    this.isDragging = false;
    
    if (this.mode === 'crop') {
      if (this.cropBox.w < 10 || this.cropBox.h < 10) {
        this.showCropBox = false;
      }
    }
  }

  crop() {
    if (!this.originalImageElement) {
      this.close();
      return;
    }
    
    // If no crop box drawn, default to full image
    if (!this.showCropBox) {
      this.cropBox = { x: 0, y: 0, w: this.originalImageElement.width, h: this.originalImageElement.height };
    }

    const img = this.originalImageElement;
    
    // Create a temporary canvas that merges the original image and our drawings
    // at the original image's maximum resolution
    const mergedCanvas = document.createElement('canvas');
    mergedCanvas.width = img.naturalWidth;
    mergedCanvas.height = img.naturalHeight;
    const mergedCtx = mergedCanvas.getContext('2d');
    if (!mergedCtx) return;

    // Draw the underlying original image
    mergedCtx.drawImage(img, 0, 0);
    
    // Draw the annotations if any exist, scaling them up to match natural resolution
    if (this.hasDrawings) {
      mergedCtx.drawImage(this.drawCanvas.nativeElement, 0, 0, img.naturalWidth, img.naturalHeight);
    }
    
    // Scale factor to map CSS pixels (crop box) back to natural pixels (merged canvas)
    const scaleX = img.naturalWidth / img.width;
    const scaleY = img.naturalHeight / img.height;

    let sourceX = this.cropBox.x * scaleX;
    let sourceY = this.cropBox.y * scaleY;
    let sourceW = this.cropBox.w * scaleX;
    let sourceH = this.cropBox.h * scaleY;

    // Create the final output canvas and apply rotation if needed
    const finalCanvas = document.createElement('canvas');
    const finalCtx = finalCanvas.getContext('2d');
    if (!finalCtx) return;

    if (this.rotation === 90 || this.rotation === -270) {
      finalCanvas.width = sourceH;
      finalCanvas.height = sourceW;
      finalCtx.translate(finalCanvas.width / 2, finalCanvas.height / 2);
      finalCtx.rotate(this.rotation * Math.PI / 180);
      finalCtx.drawImage(mergedCanvas, sourceX, sourceY, sourceW, sourceH, -sourceW / 2, -sourceH / 2, sourceW, sourceH);
    } else if (this.rotation === -90 || this.rotation === 270) {
      finalCanvas.width = sourceH;
      finalCanvas.height = sourceW;
      finalCtx.translate(finalCanvas.width / 2, finalCanvas.height / 2);
      finalCtx.rotate(this.rotation * Math.PI / 180);
      finalCtx.drawImage(mergedCanvas, sourceX, sourceY, sourceW, sourceH, -sourceW / 2, -sourceH / 2, sourceW, sourceH);
    } else if (this.rotation === 180 || this.rotation === -180) {
      finalCanvas.width = sourceW;
      finalCanvas.height = sourceH;
      finalCtx.translate(finalCanvas.width / 2, finalCanvas.height / 2);
      finalCtx.rotate(this.rotation * Math.PI / 180);
      finalCtx.drawImage(mergedCanvas, sourceX, sourceY, sourceW, sourceH, -sourceW / 2, -sourceH / 2, sourceW, sourceH);
    } else {
      finalCanvas.width = sourceW;
      finalCanvas.height = sourceH;
      finalCtx.drawImage(mergedCanvas, sourceX, sourceY, sourceW, sourceH, 0, 0, sourceW, sourceH);
    }

    const croppedDataUrl = finalCanvas.toDataURL('image/png');
    this.cropped.emit(croppedDataUrl);
    this.close();
  }
}
