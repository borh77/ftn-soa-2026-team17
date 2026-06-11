import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth';

export interface RecommendationsResponse {
  recommendations: number[];
}

@Injectable({
  providedIn: 'root'
})
export class FollowerService {
  private readonly apiUrl = `${environment.apiHost}/follower/api/followers`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  follow(followingId: number): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/follow`, { followingId }, {
      headers: this.getAuthHeaders()
    });
  }

  unfollow(followingId: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/unfollow`, {
      headers: this.getAuthHeaders(),
      body: { followingId }
    });
  }

  canComment(authorId: number): Observable<boolean> {
    return this.http.get<{ canComment: boolean }>(`${this.apiUrl}/can-comment?authorId=${authorId}`, {
      headers: this.getAuthHeaders()
    }).pipe(
      map(response => response.canComment === true)
    );
  }

  getRecommendations(): Observable<number[]> {
    return this.http.get<RecommendationsResponse>(`${this.apiUrl}/recommendations`, {
      headers: this.getAuthHeaders()
    }).pipe(
      map(response => response.recommendations ?? [])
    );
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.authService.getToken()}`
    });
  }
}