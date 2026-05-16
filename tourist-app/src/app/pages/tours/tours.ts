import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../core/services/auth';
import { KeyPoint, Tour, TourReview, TourService } from '../../core/services/tour';

interface ReviewForm {
  rating: number;
  comment: string;
  visitedAt: string;
  imageUrlInput: string;
  images: string[];
}

@Component({
  selector: 'app-tours',
  imports: [CommonModule, FormsModule],
  templateUrl: './tours.html',
  styleUrl: './tours.css'
})
export class Tours implements OnInit {
  guideTours: Tour[] = [];
  activeTours: Tour[] = [];
  reviewsByTourId: Record<number, TourReview[]> = {};
  reviewForms: Record<number, ReviewForm> = {};
  reviewMessages: Record<number, string> = {};
  reviewErrors: Record<number, string> = {};
  reviewSubmitting: Record<number, boolean> = {};
  reviewsLoaded: Record<number, boolean> = {};
  imageDropActive: Record<number, boolean> = {};
  pendingReviewIds: Record<number, number> = {};

  isLoading = false;
  message = '';
  errorMessage = '';

  newTour = {
    name: '',
    description: '',
    difficulty: 'Easy',
    tagsText: '',
    keyPoints: [
      this.emptyKeyPoint(1),
      this.emptyKeyPoint(2)
    ]
  };

  constructor(
    public authService: AuthService,
    private tourService: TourService
  ) {}

  ngOnInit(): void {
    this.loadTours();
  }

  get role(): string | null {
    return this.authService.getUserRole();
  }

  loadTours(): void {
    this.message = '';
    this.errorMessage = '';

    if (this.role === 'GUIDE') {
      this.loadGuideTours();
      return;
    }

    this.loadActiveTours();
  }

  createTour(): void {
    this.message = '';
    this.errorMessage = '';

    const keyPoints = this.newTour.keyPoints
      .filter(point => point.name.trim() || point.description.trim())
      .map((point, index) => ({
        ...point,
        ordinalNo: index + 1,
        name: point.name.trim(),
        description: point.description.trim(),
        secretText: point.secretText.trim(),
        imageUrl: point.imageUrl.trim(),
        latitude: Number(point.latitude),
        longitude: Number(point.longitude)
      }));

    this.tourService.createTour({
      name: this.newTour.name.trim(),
      description: this.newTour.description.trim(),
      difficulty: this.newTour.difficulty,
      tags: this.splitLinesOrCommas(this.newTour.tagsText),
      keyPoints
    }).subscribe({
      next: tour => {
        this.message = `Tura "${tour.name}" je sacuvana kao draft.`;
        this.resetTourForm();
        this.loadGuideTours();
      },
      error: () => {
        this.errorMessage = 'Tura nije kreirana. Proveri da li ima bar dve kljucne tacke i sva obavezna polja.';
      }
    });
  }

  publishTour(tour: Tour): void {
    this.message = '';
    this.errorMessage = '';

    this.tourService.publishTour(tour.id).subscribe({
      next: () => {
        this.message = `Tura "${tour.name}" je publishovana.`;
        this.loadGuideTours();
      },
      error: () => {
        this.errorMessage = 'Tura nije publishovana. Backend trazi validnu turu sa dovoljno kljucnih tacaka.';
      }
    });
  }

  addKeyPoint(): void {
    this.newTour.keyPoints.push(this.emptyKeyPoint(this.newTour.keyPoints.length + 1));
  }

  removeKeyPoint(index: number): void {
    if (this.newTour.keyPoints.length <= 2) {
      this.errorMessage = 'Za publish ture su potrebne bar dve kljucne tacke.';
      return;
    }

    this.newTour.keyPoints.splice(index, 1);
  }

  toggleReviews(tour: Tour): void {
    if (this.reviewsByTourId[tour.id]) {
      delete this.reviewsByTourId[tour.id];
      return;
    }

    this.loadReviews(tour.id);
  }

  createReview(tour: Tour): void {
    this.reviewMessages[tour.id] = '';
    this.reviewErrors[tour.id] = '';

    if (this.hasMyReview(tour.id)) {
      this.reviewErrors[tour.id] = 'Vec si ostavila recenziju za ovu turu.';
      return;
    }

    if (!this.reviewsLoaded[tour.id]) {
      this.reviewErrors[tour.id] = 'Sacekaj trenutak da ucitamo postojece recenzije za ovu turu.';
      this.loadReviews(tour.id);
      return;
    }

    const form = this.getReviewForm(tour.id);
    const visitedAt = new Date(form.visitedAt);

    if (!form.comment.trim()) {
      this.reviewErrors[tour.id] = 'Unesi komentar pre slanja recenzije.';
      return;
    }

    if (Number.isNaN(visitedAt.getTime())) {
      this.reviewErrors[tour.id] = 'Izaberi ispravan datum posete.';
      return;
    }

    this.reviewSubmitting[tour.id] = true;
    this.reviewMessages[tour.id] = 'Recenzija je dodata.';

    const optimisticId = -Date.now();
    const optimisticReview: TourReview = {
      id: optimisticId,
      tourId: tour.id,
      touristId: this.authService.getPersonId() ?? 0,
      touristUsername: this.authService.getUsername() ?? 'You',
      rating: Number(form.rating),
      comment: form.comment.trim(),
      visitedAt: visitedAt.toISOString(),
      createdAt: new Date().toISOString(),
      images: [...form.images]
    };

    this.pendingReviewIds[tour.id] = optimisticId;
    this.reviewsByTourId[tour.id] = [
      optimisticReview,
      ...(this.reviewsByTourId[tour.id] ?? [])
    ];
    this.reviewsLoaded[tour.id] = true;

    this.tourService.createReview(tour.id, {
      rating: Number(form.rating),
      comment: form.comment.trim(),
      visitedAt: visitedAt.toISOString(),
      images: form.images
    }).subscribe({
      next: review => {
        this.reviewSubmitting[tour.id] = false;
        this.reviewForms[tour.id] = this.defaultReviewForm();
        this.reviewsByTourId[tour.id] = (this.reviewsByTourId[tour.id] ?? [])
          .map(existing => existing.id === optimisticId ? review : existing);
        this.reviewsLoaded[tour.id] = true;
        delete this.pendingReviewIds[tour.id];
        this.reviewMessages[tour.id] = 'Recenzija je uspesno sacuvana.';
      },
      error: (error: HttpErrorResponse) => {
        this.reviewSubmitting[tour.id] = false;
        this.reviewsByTourId[tour.id] = (this.reviewsByTourId[tour.id] ?? [])
          .filter(existing => existing.id !== optimisticId);
        delete this.pendingReviewIds[tour.id];
        this.reviewErrors[tour.id] = this.reviewErrorMessage(error);
        if (this.isAlreadyReviewedError(error)) {
          this.loadReviews(tour.id);
        }
      }
    });
  }

  getReviewForm(tourId: number): ReviewForm {
    if (!this.reviewForms[tourId]) {
      this.reviewForms[tourId] = this.defaultReviewForm();
    }

    return this.reviewForms[tourId];
  }

  stars(rating: number): string {
    return `${rating}/5`;
  }

  addImageUrl(tourId: number): void {
    const form = this.getReviewForm(tourId);
    const urls = this.splitLinesOrCommas(form.imageUrlInput);

    if (urls.length === 0) {
      return;
    }

    form.images = [...form.images, ...urls];
    form.imageUrlInput = '';
  }

  removeReviewImage(tourId: number, index: number): void {
    this.getReviewForm(tourId).images.splice(index, 1);
  }

  onImageDragOver(event: DragEvent, tourId: number): void {
    event.preventDefault();
    this.imageDropActive[tourId] = true;
  }

  onImageDragLeave(event: DragEvent, tourId: number): void {
    event.preventDefault();
    this.imageDropActive[tourId] = false;
  }

  onImageDrop(event: DragEvent, tourId: number): void {
    event.preventDefault();
    this.imageDropActive[tourId] = false;
    this.reviewErrors[tourId] = '';

    const form = this.getReviewForm(tourId);
    const files = Array.from(event.dataTransfer?.files ?? []);
    const droppedText = event.dataTransfer?.getData('text/plain') ?? '';

    if (files.length === 0 && droppedText.trim()) {
      form.images = [...form.images, ...this.splitLinesOrCommas(droppedText)];
      return;
    }

    files
      .filter(file => file.type.startsWith('image/'))
      .forEach(file => {
        this.resizeImage(file)
          .then(image => {
            form.images = [...form.images, image];
          })
          .catch(() => {
            this.reviewErrors[tourId] = 'Ne mogu da obradim ovu sliku. Probaj drugu sliku ili dodaj URL.';
          });
      });
  }

  hasMyReview(tourId: number): boolean {
    const personId = this.authService.getPersonId();
    return !!personId && (this.reviewsByTourId[tourId] ?? []).some(review => review.touristId === personId);
  }

  myReview(tourId: number): TourReview | undefined {
    const personId = this.authService.getPersonId();
    return (this.reviewsByTourId[tourId] ?? []).find(review => review.touristId === personId);
  }

  isReviewPending(tourId: number): boolean {
    return !!this.pendingReviewIds[tourId];
  }

  private loadGuideTours(): void {
    this.isLoading = true;

    this.tourService.getMyTours().subscribe({
      next: response => {
        this.guideTours = response.results;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Ne mogu da ucitam tvoje ture. Proveri da li je backend podignut.';
        this.isLoading = false;
      }
    });
  }

  private loadActiveTours(): void {
    this.isLoading = true;

    this.tourService.getActiveTours(1, 50).subscribe({
      next: response => {
        this.activeTours = response.results;
        this.reviewsByTourId = {};
        this.reviewsLoaded = {};
        this.isLoading = false;

        this.activeTours.forEach(tour => {
          this.reviewsByTourId[tour.id] = [];
          this.reviewsLoaded[tour.id] = false;
        });

        this.loadVisibleReviews();
      },
      error: () => {
        this.errorMessage = 'Ne mogu da ucitam objavljene ture. Proveri login i backend.';
        this.isLoading = false;
      }
    });
  }

  private loadVisibleReviews(): void {
    if (this.activeTours.length === 0) {
      return;
    }

    const reviewRequests = this.activeTours.map(tour =>
      this.tourService.getReviews(tour.id, 1, 100).pipe(
        catchError(() => of({ results: [], totalCount: 0 }))
      )
    );

    forkJoin(reviewRequests).subscribe(reviewResults => {
      this.activeTours.forEach((tour, index) => {
        this.reviewsByTourId[tour.id] = reviewResults[index]?.results ?? [];
        this.reviewsLoaded[tour.id] = true;
      });
    });
  }

  private loadReviews(tourId: number): void {
    this.reviewsLoaded[tourId] = false;

    this.tourService.getReviews(tourId).subscribe({
      next: response => {
        this.reviewsByTourId[tourId] = response.results;
        this.reviewsLoaded[tourId] = true;
      },
      error: () => {
        this.reviewErrors[tourId] = 'Ne mogu da ucitam recenzije za ovu turu.';
        this.reviewsLoaded[tourId] = true;
      }
    });
  }

  private reviewErrorMessage(error: HttpErrorResponse): string {
    const detail = typeof error.error?.detail === 'string' ? error.error.detail : '';

    if (detail.includes('already reviewed')) {
      return 'Vec si ostavila recenziju za ovu turu. Ucitavam je sada.';
    }

    if (detail) {
      return detail;
    }

    return 'Recenzija nije dodata. Proveri komentar, ocenu i datum posete.';
  }

  private isAlreadyReviewedError(error: HttpErrorResponse): boolean {
    const detail = typeof error.error?.detail === 'string' ? error.error.detail : '';
    return detail.includes('already reviewed');
  }

  private resetTourForm(): void {
    this.newTour = {
      name: '',
      description: '',
      difficulty: 'Easy',
      tagsText: '',
      keyPoints: [
        this.emptyKeyPoint(1),
        this.emptyKeyPoint(2)
      ]
    };
  }

  private defaultReviewForm(): ReviewForm {
    return {
      rating: 5,
      comment: '',
      visitedAt: new Date().toISOString().slice(0, 16),
      imageUrlInput: '',
      images: []
    };
  }

  private emptyKeyPoint(ordinalNo: number): KeyPoint {
    return {
      ordinalNo,
      name: '',
      description: '',
      secretText: '',
      imageUrl: '',
      latitude: 45.2526,
      longitude: 19.8622
    };
  }

  private splitLinesOrCommas(value: string): string[] {
    return value
      .split(/[\n,]/)
      .map(item => item.trim())
      .filter(Boolean);
  }

  private resizeImage(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onerror = () => reject();
      reader.onload = () => {
        const image = new Image();

        image.onerror = () => reject();
        image.onload = () => {
          const maxSide = 900;
          const scale = Math.min(1, maxSide / Math.max(image.width, image.height));
          const canvas = document.createElement('canvas');
          canvas.width = Math.max(1, Math.round(image.width * scale));
          canvas.height = Math.max(1, Math.round(image.height * scale));

          const context = canvas.getContext('2d');

          if (!context) {
            reject();
            return;
          }

          context.drawImage(image, 0, 0, canvas.width, canvas.height);
          resolve(canvas.toDataURL('image/jpeg', 0.72));
        };

        image.src = String(reader.result);
      };

      reader.readAsDataURL(file);
    });
  }
}
