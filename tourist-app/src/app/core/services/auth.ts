import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

import { Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  role: 'TOURIST' | 'GUIDE';
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiHost}/stakeholders/api/auth`;

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  login(request: LoginRequest): Observable<string> {
    return this.http.post(`${this.apiUrl}/login`, request, {
      responseType: 'text'
    }).pipe(
      tap(token => {
        if (this.isBrowser()) {
          localStorage.setItem('jwt', token);

        }
      })
    );
  }

  register(request: RegisterRequest): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/register`, request);
  }

  logout(): void {
    if (this.isBrowser()) {
      localStorage.removeItem('jwt');
    }
  }

  getToken(): string | null {
    if (!this.isBrowser()) {
      return null;
    }

    return localStorage.getItem('jwt');
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  getUserRole(): string | null {
  const token = this.getToken();

  if (!token) {
    return null;
  }

  const payload = JSON.parse(atob(token.split('.')[1]));

  return payload.role;
}

getUsername(): string | null {
  const token = this.getToken();

  if (!token) {
    return null;
  }

  const payload = JSON.parse(atob(token.split('.')[1]));

  return payload.sub;
}

}