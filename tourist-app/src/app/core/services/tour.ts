import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth';

export interface PagedResult<T> {
  results: T[];
  totalCount: number;
}

export interface KeyPoint {
  ordinalNo?: number | null;
  name: string;
  description: string;
  secretText: string;
  imageUrl: string;
  latitude: number;
  longitude: number;
}

export interface TourTravelTime {
  transportType: 'Walking' | 'Bicycle' | 'Car';
  minutes: number;
}

export interface Tour {
  id: number;
  authorId: number;
  name: string;
  description: string;
  difficulty: string;
  tags: string[];
  status: string;
  price: number;
  routeLengthKm: number;
  publishedAt: string | null;
  archivedAt: string | null;
  travelTimes: TourTravelTime[];
  keyPoints: KeyPoint[];
}

export interface CreateTourRequest {
  name: string;
  description: string;
  difficulty: string;
  tags: string[];
  travelTimes: TourTravelTime[];
  keyPoints: KeyPoint[];
}

export interface TourReview {
  id: number;
  tourId: number;
  touristId: number;
  touristUsername: string;
  rating: number;
  comment: string;
  visitedAt: string;
  createdAt: string;
  images: string[];
}

export interface CreateTourReviewRequest {
  rating: number;
  comment: string;
  visitedAt: string;
  images: string[];
}

@Injectable({
  providedIn: 'root'
})
export class TourService {
  private readonly apiUrl = `${environment.apiHost}/tours/api/Tours`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getMyTours(page = 1, pageSize = 20): Observable<PagedResult<Tour>> {
    return this.http.get<PagedResult<Tour>>(this.apiUrl, {
      headers: this.getAuthHeaders(),
      params: this.paginationParams(page, pageSize)
    });
  }

  getActiveTours(page = 1, pageSize = 20): Observable<PagedResult<Tour>> {
    return this.http.get<PagedResult<Tour>>(`${this.apiUrl}/active`, {
      headers: this.getAuthHeaders(),
      params: this.paginationParams(page, pageSize)
    });
  }

  createTour(request: CreateTourRequest): Observable<Tour> {
    return this.http.post<Tour>(this.apiUrl, request, {
      headers: this.getAuthHeaders()
    });
  }

  addKeyPoint(tourId: number, keyPoint: KeyPoint): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${tourId}/keypoints`, keyPoint, {
      headers: this.getAuthHeaders()
    });
  }

  updateKeyPoint(tourId: number, ordinalNo: number, keyPoint: KeyPoint): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${tourId}/keypoints/${ordinalNo}`, keyPoint, {
      headers: this.getAuthHeaders()
    });
  }

  deleteKeyPoint(tourId: number, ordinalNo: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${tourId}/keypoints/${ordinalNo}`, {
      headers: this.getAuthHeaders()
    });
  }

  publishTour(tourId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${tourId}/publish`, null, {
      headers: this.getAuthHeaders()
    });
  }

  archiveTour(tourId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${tourId}/archive`, null, {
      headers: this.getAuthHeaders()
    });
  }

  reactivateTour(tourId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${tourId}/reactivate`, null, {
      headers: this.getAuthHeaders()
    });
  }

  getReviews(tourId: number, page = 1, pageSize = 100): Observable<PagedResult<TourReview>> {
    return this.http.get<PagedResult<TourReview>>(`${this.apiUrl}/${tourId}/reviews`, {
      params: this.paginationParams(page, pageSize)
    });
  }

  createReview(tourId: number, request: CreateTourReviewRequest): Observable<TourReview> {
    return this.http.post<TourReview>(`${this.apiUrl}/${tourId}/reviews`, request, {
      headers: this.getAuthHeaders()
    });
  }

  private paginationParams(page: number, pageSize: number): HttpParams {
    return new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.authService.getToken()}`
    });
  }
}
