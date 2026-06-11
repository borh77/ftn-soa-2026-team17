import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth';

export interface OrderItemResponse {
  id: number;
  tourId: number;
  tourName: string;
  price: number;
}

export interface ShoppingCartResponse {
  id: number;
  touristId: number;
  totalPrice: number;
  items: OrderItemResponse[];
}

export interface TourPurchaseTokenResponse {
  id: number;
  touristId: number;
  tourId: number;
  tourName: string;
  token: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseService {
  private readonly apiUrl = `${environment.apiHost}/purchases/api`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getCart(): Observable<ShoppingCartResponse> {
    return this.http.get<any>(`${this.apiUrl}/cart`, {
      headers: this.getAuthHeaders()
    }).pipe(map(cart => this.mapCart(cart)));
  }

  addToCart(tourId: number): Observable<ShoppingCartResponse> {
    return this.http.post<any>(`${this.apiUrl}/cart/items/${tourId}`, null, {
      headers: this.getAuthHeaders()
    }).pipe(map(cart => this.mapCart(cart)));
  }

  removeItem(itemId: number): Observable<ShoppingCartResponse> {
    return this.http.delete<any>(`${this.apiUrl}/cart/items/${itemId}`, {
      headers: this.getAuthHeaders()
    }).pipe(map(cart => this.mapCart(cart)));
  }

  checkout(): Observable<TourPurchaseTokenResponse[]> {
    return this.http.post<any>(`${this.apiUrl}/cart/checkout`, null, {
      headers: this.getAuthHeaders()
    }).pipe(map(response => this.mapTokens(response.tokens ?? response)));
  }

  getMyTokens(): Observable<TourPurchaseTokenResponse[]> {
    return this.http.get<any[]>(`${this.apiUrl}/purchases/tokens`, {
      headers: this.getAuthHeaders()
    }).pipe(map(tokens => this.mapTokens(tokens)));
  }

  hasPurchased(tourId: number): Observable<boolean> {
    return this.http.get<any>(`${this.apiUrl}/purchases/tours/${tourId}/purchased`, {
      headers: this.getAuthHeaders()
    }).pipe(map(response => response.purchased === true));
  }

  private mapCart(cart: any): ShoppingCartResponse {
    return {
      id: cart.id,
      touristId: cart.touristId,
      totalPrice: cart.totalPrice ?? 0,
      items: (cart.items ?? []).map((item: any) => ({
        id: item.id,
        tourId: item.tourId ?? item.tourID,
        tourName: item.tourName,
        price: item.price ?? 0
      }))
    };
  }

  private mapTokens(tokens: any[]): TourPurchaseTokenResponse[] {
    return (tokens ?? []).map(token => ({
      id: token.id,
      touristId: token.touristId,
      tourId: token.tourId ?? token.tourID,
      tourName: token.tourName,
      token: token.token,
      createdAt: token.createdAt
    }));
  }

  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.authService.getToken()}`
    });
  }
}