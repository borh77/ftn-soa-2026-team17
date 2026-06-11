import { CurrencyPipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { PurchaseService, ShoppingCartResponse } from '../../core/services/purchase';

@Component({
  selector: 'app-cart',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.css'
})
export class Cart implements OnInit {
  cart: ShoppingCartResponse | null = null;
  isLoading = false;
  isCheckingOut = false;
  message = '';
  errorMessage = '';

  constructor(
    private purchaseService: PurchaseService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.isLoading = true;
    this.message = '';
    this.errorMessage = '';

    this.purchaseService.getCart()
      .pipe(finalize(() => {this.isLoading = false;
                           this.cdr.detectChanges();}))
      .subscribe({
        next: cart => {
          this.cart = cart;
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Cart could not be loaded.';
        }
      });
  }

  removeItem(itemId: number): void {
    this.message = '';
    this.errorMessage = '';

    this.purchaseService.removeItem(itemId).subscribe({
        next: cart => {
        this.cart = { ...cart };
        this.message = 'Item removed from cart.';
        this.cdr.detectChanges();
        },
        error: () => {
        this.errorMessage = 'Item was not removed.';
        this.cdr.detectChanges();
        }
    });
  }

  checkout(): void {
    if (!this.cart || this.cart.items.length === 0) {
      this.errorMessage = 'Cart is empty.';
      return;
    }

    this.isCheckingOut = true;
    this.message = '';
    this.errorMessage = '';

    this.purchaseService.checkout()
      .pipe(finalize(() => this.isCheckingOut = false))
      .subscribe({
        next: () => {
          this.router.navigate(['/my-tokens']);
        },
        error: () => {
          this.errorMessage = 'Checkout failed.';
        }
      });
  }
}