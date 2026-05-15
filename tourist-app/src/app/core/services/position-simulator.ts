import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth';

export interface TouristPosition {
  latitude: number;
  longitude: number;
  updatedAt: string;
}

export interface UpdateTouristPositionRequest {
  latitude: number;
  longitude: number;
}

@Injectable({
  providedIn: 'root'
})
export class PositionSimulatorService {
  private readonly apiUrl =
    `${environment.apiHost}/tours/api/position-simulator/me`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getMyPosition(): Observable<TouristPosition | null> {
    return this.http.get<TouristPosition | null>(this.apiUrl, {
      headers: this.getAuthHeaders()
    });
  }

  updateMyPosition(
    request: UpdateTouristPositionRequest
  ): Observable<TouristPosition> {
    return this.http.put<TouristPosition>(
      this.apiUrl,
      request,
      {
        headers: this.getAuthHeaders()
      }
    );
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.authService.getToken()}`
    });
  }
}