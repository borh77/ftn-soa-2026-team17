import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
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

export interface CreateBlogRequest {
  title: string;
  description: string;
  creationDate: string;
  images: string[];
}

export interface CreateCommentRequest {
  text: string;
}

export interface UpdateCommentRequest {
  text: string;
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
    return this.http.get<PagedResult<BlogEntry>>(this.apiUrl, {
      headers: this.getAuthHeaders(),
      params: this.paginationParams(page, pageSize)
    });
  }

  createBlog(request: CreateBlogRequest): Observable<BlogEntry> {
    return this.http.post<BlogEntry>(this.apiUrl, request, {
      headers: this.getAuthHeaders()
    });
  }

  addComment(blogId: number, request: CreateCommentRequest): Observable<CommentResponse> {
    return this.http.post<CommentResponse>(`${this.apiUrl}/${blogId}/comments`, request, {
      headers: this.getAuthHeaders()
    });
  }

  updateComment(
    blogId: number,
    commentId: number,
    request: UpdateCommentRequest
  ): Observable<CommentResponse> {
    return this.http.put<CommentResponse>(`${this.apiUrl}/${blogId}/comments/${commentId}`, request, {
      headers: this.getAuthHeaders()
    });
  }

  deleteComment(blogId: number, commentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${blogId}/comments/${commentId}`, {
      headers: this.getAuthHeaders()
    });
  }

  likeBlog(blogId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${blogId}/likes`, null, {
      headers: this.getAuthHeaders()
    });
  }

  unlikeBlog(blogId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${blogId}/likes`, {
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
