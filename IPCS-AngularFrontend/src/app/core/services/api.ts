import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * ApiService
 * Specialized service for API communication.
 * Authentication headers are now handled globally via AuthInterceptor.
 */
@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private baseUrl = environment.apiUrl; 

  public getFileServerUrl(): string {
    return (environment as any).fileServerUrl || this.baseUrl.replace('/api', '');
  }

  constructor(private http: HttpClient) {}

  /**
   * Performs a GET request
   */
  get<T>(endpoint: string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${endpoint}`);
  }

  /**
   * Performs a POST request
   */
  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, data);
  }

  /**
   * Performs a PUT request
   */
  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${endpoint}`, data);
  }

  /**
   * Performs a DELETE request
   */
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${endpoint}`);
  }

  /**
   * Uploads a file and returns the file path
   */
  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/Upload`, formData);
  }

  /**
   * Uploads a data URL and returns the file path
   */
  async uploadDataUrl(dataUrl: string): Promise<string> {
    const file = await this.dataUrlToFile(dataUrl);
    return new Promise((resolve, reject) => {
      this.uploadFile(file).subscribe({
        next: (response: any) => resolve(response.filePath),
        error: reject
      });
    });
  }

  /**
   * Converts a data URL to a File object
   */
  private async dataUrlToFile(dataUrl: string): Promise<File> {
    const response = await fetch(dataUrl);
    const blob = await response.blob();
    return new File([blob], 'image.png', { type: 'image/png' });
  }
}
