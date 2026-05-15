import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth';

export interface UserAccount {
  id: number;
  username: string;
  email: string;
  role: string;
  blocked: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AdminUserService {
  private readonly apiUrl = `${environment.apiHost}/stakeholders/api/admin/users`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getUsers(): Observable<UserAccount[]> {
    return this.http.get<UserAccount[]>(this.apiUrl, {
      headers: this.getAuthHeaders()
    });
  }

  blockUser(id: number): Observable<UserAccount> {
    return this.http.patch<UserAccount>(`${this.apiUrl}/${id}/block`, {}, {
      headers: this.getAuthHeaders()
    });
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.authService.getToken()}`
    });
  }
}