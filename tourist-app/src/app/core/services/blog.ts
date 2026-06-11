import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth';

export interface CommentResponse {
  id: number;
  authorId: number;
  text: string;
  createdAt: string;
  lastModifiedAt?: string | null;
}

export interface BlogEntry {
  id: number;
  authorId: number;
  title: string;
  description: string;
  creationDate: string;
  images: string[];
  comments: CommentResponse[];
  likeCount: number;
  isLikedByCurrentUser: boolean;
}

export interface PagedResult<T> {
  results: T[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class BlogService {
  private readonly apiUrl = `${environment.apiHost}/blog/api/tourist/blog`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getBlogs(page = 1, pageSize = 20): Observable<PagedResult<BlogEntry>> {
    return this.http.get<PagedResult<BlogEntry>>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`, {
      headers: this.getAuthHeaders()
    });
  }

  addComment(blogId: number, text: string): Observable<CommentResponse> {
    return this.http.post<CommentResponse>(`${this.apiUrl}/${blogId}/comments`, { text }, {
      headers: this.getAuthHeaders()
    });
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.authService.getToken()}`
    });
  }
}