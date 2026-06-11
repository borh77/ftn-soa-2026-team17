import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { PurchaseService, TourPurchaseTokenResponse } from '../../core/services/purchase';

@Component({
  selector: 'app-my-tokens',
  imports: [DatePipe, RouterLink],
  templateUrl: './my-tokens.html',
  styleUrl: './my-tokens.css'
})
export class MyTokens implements OnInit {
  tokens: TourPurchaseTokenResponse[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(
    private purchaseService: PurchaseService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadTokens();
  }

  loadTokens(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.purchaseService.getMyTokens()
      .pipe(finalize(() => {this.isLoading = false; this.cdr.detectChanges();}))
      .subscribe({
        next: tokens => {
          this.tokens = tokens;
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Tokens could not be loaded.';
        }
      });
  }
}