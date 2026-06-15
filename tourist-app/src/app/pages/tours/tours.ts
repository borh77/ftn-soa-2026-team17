import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  HostListener,
  Inject,
  OnDestroy,
  OnInit,
  PLATFORM_ID
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../core/services/auth';
import { PositionSimulatorService } from '../../core/services/position-simulator';
import { KeyPoint, Tour, TourExecution, TourReview, TourService } from '../../core/services/tour';
import { PurchaseService } from '../../core/services/purchase';
import { finalize } from 'rxjs';
import { Router, RouterLink } from '@angular/router';

interface ReviewForm {
  rating: number;
  comment: string;
  visitedAt: string;
  imageUrlInput: string;
  images: string[];
}

interface ExistingKeyPointEditor {
  mode: 'add' | 'edit';
  originalOrdinalNo?: number;
  point: KeyPoint;
}

interface TourDetailsEditor {
  name: string;
  description: string;
  difficulty: string;
  tagsText: string;
  walkingMinutes: number;
  bicycleMinutes: number;
  carMinutes: number;
}

@Component({
  selector: 'app-tours',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './tours.html',
  styleUrl: './tours.css'
})
export class Tours implements OnInit, AfterViewInit, OnDestroy {
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
  activeExecutionsByTourId: Record<number, TourExecution> = {};
  executionMessages: Record<number, string> = {};
  executionErrors: Record<number, string> = {};
  executionChecking: Record<number, boolean> = {};

  cartMessages: Record<number, string> = {};
  cartErrors: Record<number, string> = {};
  cartLoading: Record<number, boolean> = {};
  cartTourIds = new Set<number>();
  purchasedTourIds = new Set<number>();

  selectedDraftPointIndex = 0;
  activeKeyPointEditors: Record<number, ExistingKeyPointEditor> = {};
  keyPointMessages: Record<number, string> = {};
  keyPointErrors: Record<number, string> = {};
  keyPointImageDropActive: Record<string, boolean> = {};
  publishPriceByTourId: Record<number, number> = {};
  tourDetailsEditors: Record<number, TourDetailsEditor> = {};
  tourActionLoadingById: Record<number, boolean> = {};
  lifecycleLoadingByTourId: Record<number, boolean> = {};
  selectedImage: string | null = null;
  selectedReviewTour: Tour | null = null;

  currentRole: string | null = null;
  isLoading = false;
  showCreateTourForm = false;
  message = '';
  errorMessage = '';

  private readonly defaultCenter: [number, number] = [45.2526, 19.8622];

  newTour = {
    name: '',
    description: '',
    difficulty: 'Easy',
    tagsText: '',
    walkingMinutes: 0,
    bicycleMinutes: 0,
    carMinutes: 0,
    keyPoints: [] as KeyPoint[]
  };

  private readonly isBrowser: boolean;
  private leaflet: any;
  private maps: Record<string, any> = {};
  private mapLayers: Record<string, any> = {};

  constructor(
    public authService: AuthService,
    private tourService: TourService,
    private positionSimulatorService: PositionSimulatorService,
    private purchaseService: PurchaseService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }
  
  ngOnInit(): void {
    this.currentRole = this.authService.getUserRole();
    this.isLoading = true;
    setTimeout(() => this.loadTours(), 0);
  }

  async ngAfterViewInit(): Promise<void> {
    if (!this.isBrowser) {
      return;
    }

    this.leaflet = await import('leaflet');
    delete (this.leaflet.Icon.Default.prototype as any)._getIconUrl;
    this.leaflet.Icon.Default.mergeOptions({
      iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
      iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
      shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png'
    });

    this.scheduleMapRender();
  }

  ngOnDestroy(): void {
    Object.values(this.maps).forEach(map => map.remove());
    Object.values(this.executionCheckIntervals).forEach(intervalId => clearInterval(intervalId));
  }

  get role(): string | null {
    return this.currentRole;
  }

  loadTours(clearNotices = true): void {
    if (clearNotices) {
      this.message = '';
      this.errorMessage = '';
    }
    this.currentRole = this.authService.getUserRole();

    if (!this.currentRole) {
      this.isLoading = false;
      this.errorMessage = 'Log in to view tours.';
      return;
    }

    if (this.currentRole === 'GUIDE') {
      this.loadGuideTours();
      return;
    }

    this.loadActiveTours();
  }

  createTour(): void {
    this.message = '';
    this.errorMessage = '';

    const keyPoints = this.normalizedDraftKeyPoints();
    const travelTimes = this.newTourTravelTimes();

    if (!this.newTour.name.trim() || !this.newTour.description.trim()) {
      this.errorMessage = 'Tour name and description are required.';
      return;
    }

    if (keyPoints.some(point => !this.isCompleteKeyPoint(point))) {
      this.errorMessage = 'Every key point needs a name, description, secret text, image, and valid coordinates.';
      return;
    }

    this.tourService.createTour({
      name: this.newTour.name.trim(),
      description: this.newTour.description.trim(),
      difficulty: this.newTour.difficulty,
      tags: this.splitLinesOrCommas(this.newTour.tagsText),
      travelTimes,
      keyPoints
    }).subscribe({
      next: tour => {
        this.message = `Tour "${tour.name}" was saved as a draft.`;
        this.resetTourForm();
        this.showCreateTourForm = false;
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.backendDetail(error)
          || this.backendTitle(error)
          || 'The tour could not be created. Check its basic details and key points.';
      }
    });
  }

  startTourEdit(tour: Tour): void {
    this.tourDetailsEditors[tour.id] = {
      name: tour.name,
      description: tour.description,
      difficulty: tour.difficulty,
      tagsText: tour.tags.join(', '),
      walkingMinutes: this.travelMinutes(tour, 'Walking'),
      bicycleMinutes: this.travelMinutes(tour, 'Bicycle'),
      carMinutes: this.travelMinutes(tour, 'Car')
    };
  }

  cancelTourEdit(tourId: number): void {
    delete this.tourDetailsEditors[tourId];
  }

  saveTourDetails(tour: Tour): void {
    const editor = this.tourDetailsEditors[tour.id];

    if (!editor) {
      return;
    }

    if (!editor.name.trim() || !editor.description.trim()) {
      this.errorMessage = 'Tour name and description are required.';
      return;
    }

    this.tourActionLoadingById[tour.id] = true;
    this.message = '';
    this.errorMessage = '';

    this.tourService.updateTour(tour.id, {
      name: editor.name.trim(),
      description: editor.description.trim(),
      difficulty: editor.difficulty,
      tags: this.splitLinesOrCommas(editor.tagsText),
      price: 0,
      travelTimes: this.editorTravelTimes(editor)
    }).pipe(finalize(() => {
      this.tourActionLoadingById[tour.id] = false;
      this.cdr.detectChanges();
    })).subscribe({
      next: () => {
        delete this.tourDetailsEditors[tour.id];
        this.message = `Tour "${editor.name.trim()}" was updated.`;
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.backendDetail(error) || 'The tour details could not be saved.';
      }
    });
  }

  deleteTour(tour: Tour): void {
    this.tourActionLoadingById[tour.id] = true;
    this.message = '';
    this.errorMessage = '';

    this.tourService.deleteTour(tour.id)
      .pipe(finalize(() => {
        this.tourActionLoadingById[tour.id] = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: () => {
          delete this.tourDetailsEditors[tour.id];
          this.message = `Tour "${tour.name}" was deleted.`;
          this.loadTours(false);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.backendDetail(error) || 'The draft tour could not be deleted.';
        }
      });
  }

  publishTour(tour: Tour): void {
    this.message = '';
    this.errorMessage = '';
    const price = Number(this.publishPriceByTourId[tour.id] ?? tour.price ?? 0);

    if (!Number.isFinite(price) || price <= 0) {
      this.errorMessage = 'Enter a price greater than 0 before publishing.';
      return;
    }

    this.lifecycleLoadingByTourId[tour.id] = true;
    this.tourService.publishTour(tour.id, { price }).pipe(finalize(() => {
      this.lifecycleLoadingByTourId[tour.id] = false;
      this.cdr.detectChanges();
    })).subscribe({
      next: () => {
        this.message = `Tour "${tour.name}" was published.`;
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.backendDetail(error)
          || 'The tour could not be published. It needs basic details, tags, at least two key points, and a travel time.';
      }
    });
  }

  archiveTour(tour: Tour): void {
    this.message = '';
    this.errorMessage = '';

    this.lifecycleLoadingByTourId[tour.id] = true;
    this.tourService.archiveTour(tour.id).pipe(finalize(() => {
      this.lifecycleLoadingByTourId[tour.id] = false;
      this.cdr.detectChanges();
    })).subscribe({
      next: () => {
        this.message = `Tour "${tour.name}" was archived.`;
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.backendDetail(error) || 'The tour could not be archived.';
      }
    });
  }

  reactivateTour(tour: Tour): void {
    this.message = '';
    this.errorMessage = '';

    this.lifecycleLoadingByTourId[tour.id] = true;
    this.tourService.reactivateTour(tour.id).pipe(finalize(() => {
      this.lifecycleLoadingByTourId[tour.id] = false;
      this.cdr.detectChanges();
    })).subscribe({
      next: () => {
        this.message = `Tour "${tour.name}" was reactivated.`;
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.backendDetail(error) || 'The tour could not be reactivated.';
      }
    });
  }

  addToCart(tour: Tour): void {
    this.cartMessages[tour.id] = '';
    this.cartErrors[tour.id] = '';
    this.cartLoading[tour.id] = true;
    this.cdr.detectChanges();

    this.purchaseService.addToCart(tour.id)
      .pipe(finalize(() => {
        this.cartLoading[tour.id] = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: cart => {
          this.cartTourIds = new Set(cart.items.map(item => item.tourId));
          this.cartMessages[tour.id] = `Tour "${tour.name}" has been added to cart.`;
          this.loadTours(false);
        },
        error: error => {
          const backendMessage = error?.error?.message;
          this.cartErrors[tour.id] = backendMessage || 'Tour was not added to cart.';
          this.cdr.detectChanges();
        }
      });
  }

  addKeyPoint(): void {
    this.errorMessage = 'Click the map to add the next key point.';
    this.scheduleMapRender();
  }

  addDraftPointAt(latitude: number, longitude: number): void {
    const ordinalNo = this.newTour.keyPoints.length + 1;
    this.newTour.keyPoints.push(this.defaultDraftKeyPoint(ordinalNo, latitude, longitude));
    this.selectedDraftPointIndex = this.newTour.keyPoints.length - 1;
    this.errorMessage = '';
    this.scheduleMapRender();
  }

  clearDraftKeyPoints(): void {
    this.newTour.keyPoints = [];
    this.selectedDraftPointIndex = 0;
    this.errorMessage = '';
    this.scheduleMapRender();
  }

  removeKeyPoint(index: number): void {
    this.newTour.keyPoints.splice(index, 1);
    this.newTour.keyPoints.forEach((point, pointIndex) => point.ordinalNo = pointIndex + 1);
    this.selectedDraftPointIndex = Math.max(0, Math.min(this.selectedDraftPointIndex, this.newTour.keyPoints.length - 1));
    this.errorMessage = '';
    this.scheduleMapRender();
  }

  selectDraftPoint(index: number): void {
    this.selectedDraftPointIndex = index;
    this.scheduleMapRender();
  }

  onDraftCoordinateChange(): void {
    this.scheduleMapRender();
  }

  startAddKeyPoint(tour: Tour): void {
    const lastPoint = tour.keyPoints[tour.keyPoints.length - 1];
    this.keyPointMessages[tour.id] = 'Click the map to choose the new point position.';
    this.keyPointErrors[tour.id] = '';
    this.activeKeyPointEditors[tour.id] = {
      mode: 'add',
      point: {
        ...this.emptyKeyPoint(tour.keyPoints.length + 1),
        latitude: lastPoint?.latitude ?? this.defaultCenter[0],
        longitude: lastPoint?.longitude ?? this.defaultCenter[1]
      }
    };
    this.scheduleMapRender();
  }

  startEditKeyPoint(tour: Tour, point: KeyPoint): void {
    this.keyPointMessages[tour.id] = 'Click the map to change the point coordinate.';
    this.keyPointErrors[tour.id] = '';
    this.activeKeyPointEditors[tour.id] = {
      mode: 'edit',
      originalOrdinalNo: point.ordinalNo ?? 1,
      point: { ...point }
    };
    this.scheduleMapRender();
  }

  cancelKeyPointEdit(tourId: number): void {
    delete this.activeKeyPointEditors[tourId];
    this.keyPointMessages[tourId] = '';
    this.keyPointErrors[tourId] = '';
    this.scheduleMapRender();
  }

  saveKeyPoint(tour: Tour): void {
    const editor = this.activeKeyPointEditors[tour.id];

    if (!editor) {
      return;
    }

    this.keyPointErrors[tour.id] = '';
    const point = this.normalizeKeyPoint(editor.point);

    const request = editor.mode === 'add'
      ? this.tourService.addKeyPoint(tour.id, point)
      : this.tourService.updateKeyPoint(tour.id, editor.originalOrdinalNo ?? point.ordinalNo ?? 1, point);

    request.subscribe({
      next: () => {
        this.keyPointMessages[tour.id] = editor.mode === 'add'
          ? 'Key point added.'
          : 'Key point updated.';
        delete this.activeKeyPointEditors[tour.id];
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.keyPointErrors[tour.id] = this.backendDetail(error) || 'The key point could not be saved. Check its name, description, image, and coordinates.';
      }
    });
  }

  deleteKeyPoint(tour: Tour, point: KeyPoint): void {
    if (!point.ordinalNo) {
      return;
    }

    this.keyPointErrors[tour.id] = '';

    this.tourService.deleteKeyPoint(tour.id, point.ordinalNo).subscribe({
      next: () => {
        this.keyPointMessages[tour.id] = 'Key point deleted.';
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.keyPointErrors[tour.id] = this.backendDetail(error) || 'The key point could not be deleted.';
      }
    });
  }

  keyPointEditor(tourId: number): ExistingKeyPointEditor | undefined {
    return this.activeKeyPointEditors[tourId];
  }

  createReview(tour: Tour): void {
    this.reviewMessages[tour.id] = '';
    this.reviewErrors[tour.id] = '';

    if (this.hasMyReview(tour.id)) {
      this.reviewErrors[tour.id] = 'You have already reviewed this tour.';
      return;
    }

    if (!this.reviewsLoaded[tour.id]) {
      this.reviewErrors[tour.id] = 'Wait while existing reviews are loaded.';
      this.loadReviews(tour.id);
      return;
    }

    const form = this.getReviewForm(tour.id);
    const visitedAt = new Date(form.visitedAt);

    if (!form.comment.trim()) {
      this.reviewErrors[tour.id] = 'Enter a comment before submitting the review.';
      return;
    }

    if (Number.isNaN(visitedAt.getTime())) {
      this.reviewErrors[tour.id] = 'Choose a valid visit date.';
      return;
    }

    if (visitedAt.getTime() > Date.now()) {
      this.reviewErrors[tour.id] = 'Visit date cannot be in the future.';
      return;
    }

    this.reviewSubmitting[tour.id] = true;
    this.reviewMessages[tour.id] = 'Review added.';

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
        this.reviewMessages[tour.id] = 'Review saved successfully.';
        this.loadTours(false);
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

  publishPrice(tour: Tour): number {
    if (this.publishPriceByTourId[tour.id] == null) {
      this.publishPriceByTourId[tour.id] = Math.max(1, Number(tour.price) || 100);
    }

    return this.publishPriceByTourId[tour.id];
  }

  draftRouteLengthKm(): number {
    return this.calculateRouteLengthKm(this.newTour.keyPoints);
  }

  routeLength(tour: Tour): number {
    return Number(tour.routeLengthKm ?? this.calculateRouteLengthKm(tour.keyPoints ?? []));
  }

  canPublish(tour: Tour): boolean {
    return !!tour.name?.trim()
      && !!tour.description?.trim()
      && (tour.tags?.length ?? 0) > 0
      && (tour.keyPoints?.length ?? 0) >= 2
      && (tour.travelTimes?.length ?? 0) > 0;
  }

  travelMinutes(tour: Tour, transportType: 'Walking' | 'Bicycle' | 'Car'): number {
    return tour.travelTimes?.find(time => time.transportType === transportType)?.minutes ?? 0;
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
    this.readImageDrop(event, image => {
      form.images = [...form.images, image];
    }, text => {
      form.images = [...form.images, ...this.splitLinesOrCommas(text)];
    }, () => {
      this.reviewErrors[tourId] = 'The image could not be processed. Try another image or add a URL.';
    });
  }

  onKeyPointImageDragOver(event: DragEvent, key: string): void {
    event.preventDefault();
    this.keyPointImageDropActive[key] = true;
  }

  onKeyPointImageDragLeave(event: DragEvent, key: string): void {
    event.preventDefault();
    this.keyPointImageDropActive[key] = false;
  }

  onDraftKeyPointImageDrop(event: DragEvent, point: KeyPoint, key: string): void {
    event.preventDefault();
    this.keyPointImageDropActive[key] = false;
    this.readImageDrop(event, image => point.imageUrl = image, text => point.imageUrl = text.trim());
  }

  onExistingKeyPointImageDrop(event: DragEvent, editor: ExistingKeyPointEditor, key: string): void {
    event.preventDefault();
    this.keyPointImageDropActive[key] = false;
    this.readImageDrop(event, image => editor.point.imageUrl = image, text => editor.point.imageUrl = text.trim());
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

  startTour(tour: Tour): void {
    this.executionMessages[tour.id] = '';
    this.executionErrors[tour.id] = '';

    this.positionSimulatorService.getMyPosition().subscribe({
      next: position => {
        if (!position) {
          this.executionErrors[tour.id] = 'Set your location in the position simulator first.';
          return;
        }

        this.tourService.startTourExecution(tour.id, {
          latitude: position.latitude,
          longitude: position.longitude
        }).subscribe({
          next: execution => {
            this.activeExecutionsByTourId[tour.id] = execution;
            this.executionMessages[tour.id] = 'Tour started.';
            this.startExecutionPolling(tour.id);
            this.checkKeyPoints(tour.id);
            this.cdr.detectChanges();
          },
          error: (error: HttpErrorResponse) => {
            this.executionErrors[tour.id] = this.backendDetail(error) || 'The tour could not be started.';
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.executionErrors[tour.id] = 'The location could not be read from the position simulator.';
        this.cdr.detectChanges();
      }
    });
  }

  checkKeyPoints(tourId: number): void {
    const execution = this.activeExecutionsByTourId[tourId];

    if (!execution || execution.status !== 'Active' || this.executionChecking[tourId]) {
      return;
    }

    this.executionChecking[tourId] = true;

    this.positionSimulatorService.getMyPosition().subscribe({
      next: position => {
        if (!position) {
          this.executionChecking[tourId] = false;
          this.executionErrors[tourId] = 'Set your location in the position simulator first.';
          return;
        }

        this.tourService.checkKeyPointProximity(execution.id, {
          latitude: position.latitude,
          longitude: position.longitude
        }).subscribe({
          next: result => {
            this.executionChecking[tourId] = false;
            this.activeExecutionsByTourId[tourId] = result.execution;
            this.executionMessages[tourId] = result.reached
              ? `Key point ${result.keyPointOrdinalNo} reached.`
              : 'Location checked. There are no new key points nearby.';

            if (result.execution.status !== 'Active') {
              this.stopExecutionPolling(tourId);
            }
            this.cdr.detectChanges();
          },
          error: (error: HttpErrorResponse) => {
            this.executionChecking[tourId] = false;
            this.executionErrors[tourId] = this.backendDetail(error) || 'Key point checking failed.';
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.executionChecking[tourId] = false;
        this.executionErrors[tourId] = 'The location could not be read from the position simulator.';
        this.cdr.detectChanges();
      }
    });
  }

  completeTour(tourId: number): void {
    const execution = this.activeExecutionsByTourId[tourId];

    if (!execution) {
      return;
    }

    this.tourService.completeTourExecution(execution.id).subscribe({
      next: updated => {
        this.activeExecutionsByTourId[tourId] = updated;
        this.executionMessages[tourId] = 'Tour completed.';
        this.stopExecutionPolling(tourId);
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.executionErrors[tourId] = this.backendDetail(error) || 'The tour could not be completed.';
      }
    });
  }

  abandonTour(tourId: number): void {
    const execution = this.activeExecutionsByTourId[tourId];

    if (!execution) {
      return;
    }

    this.tourService.abandonTourExecution(execution.id).subscribe({
      next: updated => {
        this.activeExecutionsByTourId[tourId] = updated;
        this.executionMessages[tourId] = 'Tour abandoned.';
        this.stopExecutionPolling(tourId);
        this.loadTours(false);
      },
      error: (error: HttpErrorResponse) => {
        this.executionErrors[tourId] = this.backendDetail(error) || 'The tour could not be abandoned.';
      }
    });
  }

  activeExecution(tourId: number): TourExecution | undefined {
    return this.activeExecutionsByTourId[tourId];
  }

  openImage(image: string): void {
    this.selectedImage = image;
  }

  closeImage(): void {
    this.selectedImage = null;
  }

  openReviews(tour: Tour): void {
    this.selectedReviewTour = tour;
    this.loadReviews(tour.id);
  }

  closeReviews(): void {
    this.selectedReviewTour = null;
  }

  @HostListener('document:keydown.escape')
  closeOverlaysOnEscape(): void {
    this.closeImage();
    this.closeReviews();
  }

  private loadGuideTours(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.tourService.getMyTours().subscribe({
      next: response => {
        this.guideTours = response.results;
        this.isLoading = false;
        this.loadGuideReviews();
        this.scheduleMapRender();
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Your tours could not be loaded. Check that the backend is running.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private loadGuideReviews(): void {
    if (this.guideTours.length === 0) {
      return;
    }

    this.guideTours.forEach(tour => {
      this.reviewsByTourId[tour.id] = [];
      this.reviewsLoaded[tour.id] = false;
    });

    const reviewRequests = this.guideTours.map(tour =>
      this.tourService.getReviews(tour.id, 1, 100).pipe(
        catchError(() => of({ results: [], totalCount: 0 }))
      )
    );

    forkJoin(reviewRequests).subscribe(reviewResults => {
      this.guideTours.forEach((tour, index) => {
        this.reviewsByTourId[tour.id] = reviewResults[index]?.results ?? [];
        this.reviewsLoaded[tour.id] = true;
      });
      this.cdr.detectChanges();
    });
  }

  private loadActiveTours(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

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

        this.loadCartState();
        this.loadPurchasedState();
        this.loadVisibleReviews();

        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Cannot load published tours. Check login and backend.';
        this.isLoading = false;
        this.cdr.detectChanges();
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
      this.cdr.detectChanges();
    });
  }

  private loadReviews(tourId: number): void {
    this.reviewsLoaded[tourId] = false;

    this.tourService.getReviews(tourId).subscribe({
      next: response => {
        this.reviewsByTourId[tourId] = response.results;
        this.reviewsLoaded[tourId] = true;
        this.cdr.detectChanges();
      },
      error: () => {
        this.reviewErrors[tourId] = 'Reviews for this tour could not be loaded.';
        this.reviewsLoaded[tourId] = true;
        this.cdr.detectChanges();
      }
    });
  }

  private renderAllMaps(): void {
    if (!this.leaflet || this.role !== 'GUIDE') {
      return;
    }

    this.renderRouteMap(
      'draft',
      'draft-keypoint-map',
      this.newTour.keyPoints,
      (latitude, longitude) => {
        this.addDraftPointAt(latitude, longitude);
      },
      (latitude, longitude) => {
        const point = this.newTour.keyPoints[this.selectedDraftPointIndex];
        if (!point) {
          return;
        }

        point.latitude = Number(latitude.toFixed(6));
        point.longitude = Number(longitude.toFixed(6));
        this.renderAllMaps();
      },
      this.newTour.keyPoints[this.selectedDraftPointIndex]?.ordinalNo
    );

    this.guideTours.forEach(tour => {
      const editor = this.activeKeyPointEditors[tour.id];
      const visiblePoints = this.visibleRoutePoints(tour);
      const activeOrdinalNo = editor?.mode === 'edit'
        ? editor.originalOrdinalNo
        : editor?.point.ordinalNo;

      this.renderRouteMap(
        `tour-${tour.id}`,
        `tour-map-${tour.id}`,
        visiblePoints,
        () => {
          if (tour.status === 'Draft') {
            this.router.navigate(['/tours', tour.id, 'edit']);
          }
        },
        undefined,
        activeOrdinalNo
      );
    });
  }

  private visibleRoutePoints(tour: Tour): KeyPoint[] {
    const editor = this.activeKeyPointEditors[tour.id];

    if (!editor) {
      return tour.keyPoints;
    }

    if (editor.mode === 'add') {
      return [
        ...tour.keyPoints,
        {
          ...editor.point,
          ordinalNo: editor.point.ordinalNo ?? tour.keyPoints.length + 1
        }
      ];
    }

    return tour.keyPoints.map(point =>
      point.ordinalNo === editor.originalOrdinalNo
        ? { ...editor.point, ordinalNo: editor.originalOrdinalNo }
        : point
    );
  }

  private renderRouteMap(
    key: string,
    elementId: string,
    points: KeyPoint[],
    onMapClick: (latitude: number, longitude: number) => void,
    onActivePointMove?: (latitude: number, longitude: number) => void,
    activeOrdinalNo?: number | null
  ): void {
    const element = document.getElementById(elementId);

    if (!element) {
      return;
    }

    const validPoints = points
      .filter(point => this.hasValidCoordinates(point))
      .sort((a, b) => (a.ordinalNo ?? 0) - (b.ordinalNo ?? 0));

    if (!this.maps[key]) {
      this.maps[key] = this.leaflet.map(element).setView(this.defaultCenter, 13);
      this.leaflet.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
      }).addTo(this.maps[key]);
      this.mapLayers[key] = this.leaflet.layerGroup().addTo(this.maps[key]);
    }

    const map = this.maps[key];
    const layer = this.mapLayers[key];
    layer.clearLayers();
    map.off('click');
    map.on('click', (event: any) => onMapClick(event.latlng.lat, event.latlng.lng));

    const latLngs = validPoints.map(point => [point.latitude, point.longitude]);

    validPoints.forEach((point, index) => {
      const isActivePoint = activeOrdinalNo != null && point.ordinalNo === activeOrdinalNo;
      const marker = this.leaflet.marker([point.latitude, point.longitude], {
        draggable: isActivePoint
      })
        .bindPopup(`<strong>${point.ordinalNo ?? index + 1}. ${this.escapeHtml(point.name || 'New point')}</strong>`);

      if (isActivePoint) {
        marker.bindTooltip('Drag or click map to move', { permanent: true, direction: 'top' });
        marker.on('dragend', (event: any) => {
          const position = event.target.getLatLng();
          (onActivePointMove ?? onMapClick)(position.lat, position.lng);
        });
      }

      marker.addTo(layer);
    });

    if (latLngs.length >= 2) {
      this.leaflet.polyline(latLngs, {
        color: '#e76f51',
        weight: 4,
        opacity: 0.92
      }).addTo(layer);
    }

    if (latLngs.length > 0) {
      map.fitBounds(this.leaflet.latLngBounds(latLngs), {
        padding: [24, 24],
        maxZoom: 15
      });
    } else {
      map.setView(this.defaultCenter, 13);
    }

    setTimeout(() => map.invalidateSize(), 0);
  }

  private scheduleMapRender(): void {
    if (!this.isBrowser) {
      return;
    }

    setTimeout(() => this.renderAllMaps(), 0);
  }

  private reviewErrorMessage(error: HttpErrorResponse): string {
    const detail = this.backendDetail(error);

    if (detail.includes('already reviewed')) {
      return 'You have already reviewed this tour. Loading your review now.';
    }

    if (detail) {
      return detail;
    }

    return 'The review could not be added. Check the comment, rating, and visit date.';
  }

  private isAlreadyReviewedError(error: HttpErrorResponse): boolean {
    return this.backendDetail(error).includes('already reviewed');
  }

  private resetTourForm(): void {
    this.newTour = {
      name: '',
      description: '',
      difficulty: 'Easy',
      tagsText: '',
      walkingMinutes: 0,
      bicycleMinutes: 0,
      carMinutes: 0,
      keyPoints: []
    };
    this.selectedDraftPointIndex = 0;
    this.scheduleMapRender();
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
      latitude: this.defaultCenter[0],
      longitude: this.defaultCenter[1]
    };
  }

  private defaultDraftKeyPoint(ordinalNo: number, latitude: number, longitude: number): KeyPoint {
    return {
      ordinalNo,
      name: `Key point ${ordinalNo}`,
      description: `Key point ${ordinalNo} description`,
      secretText: `Secret ${ordinalNo}`,
      imageUrl: 'https://placehold.co/800x500?text=Key+point',
      latitude: Number(latitude.toFixed(6)),
      longitude: Number(longitude.toFixed(6))
    };
  }

  private normalizeKeyPoint(point: KeyPoint): KeyPoint {
    return {
      ordinalNo: point.ordinalNo,
      name: point.name.trim(),
      description: point.description.trim(),
      secretText: point.secretText.trim(),
      imageUrl: point.imageUrl.trim(),
      latitude: Number(point.latitude),
      longitude: Number(point.longitude)
    };
  }

  private splitLinesOrCommas(value: string): string[] {
    return value
      .split(/[\n,]/)
      .map(item => item.trim())
      .filter(Boolean);
  }

  private readImageDrop(
    event: DragEvent,
    onImage: (image: string) => void,
    onText: (text: string) => void,
    onError?: () => void
  ): void {
    const files = Array.from(event.dataTransfer?.files ?? []);
    const droppedText = event.dataTransfer?.getData('text/plain') ?? '';

    if (files.length === 0 && droppedText.trim()) {
      onText(droppedText);
      return;
    }

    files
      .filter(file => file.type.startsWith('image/'))
      .forEach(file => {
        this.resizeImage(file)
          .then(onImage)
          .catch(() => onError?.());
      });
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

  loadCartState(): void {
    this.purchaseService.getCart().subscribe({
      next: cart => {
        this.cartTourIds = new Set(cart.items.map(item => item.tourId));
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  loadPurchasedState(): void {
    if (this.role !== 'TOURIST') {
      return;
    }

    this.purchasedTourIds = new Set<number>();

    for (const tour of this.activeTours) {
      this.purchaseService.hasPurchased(tour.id).subscribe({
        next: purchased => {
          if (purchased) {
            this.purchasedTourIds.add(tour.id);
            this.loadPurchasedTourDetails(tour.id);
            this.cdr.detectChanges();
          }
        },
        error: () => {}
      });
    }
  }

  loadPurchasedTourDetails(tourId: number): void {
    this.tourService.getPurchasedTourDetails(tourId).subscribe({
      next: fullTour => {
        this.activeTours = this.activeTours.map(tour =>
          tour.id === tourId ? fullTour : tour
        );

        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  visibleKeyPoints(tour: Tour): KeyPoint[] {
    if (this.isPurchased(tour.id)) {
      return tour.keyPoints ?? [];
    }

    return (tour.keyPoints ?? []).slice(0, 1);
  }

  isInCart(tourId: number): boolean {
    return this.cartTourIds.has(tourId);
  }

  isPurchased(tourId: number): boolean {
    return this.purchasedTourIds.has(tourId);
  }

  private hasValidCoordinates(point: KeyPoint): boolean {
    return Number.isFinite(Number(point.latitude)) && Number.isFinite(Number(point.longitude));
  }

  private normalizedDraftKeyPoints(): KeyPoint[] {
    return this.newTour.keyPoints.map((point, index) => ({
      ...point,
      ordinalNo: index + 1,
      name: point.name.trim(),
      description: point.description.trim(),
      secretText: point.secretText.trim(),
      imageUrl: point.imageUrl.trim(),
      latitude: Number(point.latitude),
      longitude: Number(point.longitude)
    }));
  }

  private isCompleteKeyPoint(point: KeyPoint): boolean {
    return !!point.name.trim()
      && !!point.description.trim()
      && !!point.secretText.trim()
      && !!point.imageUrl.trim()
      && this.hasValidCoordinates(point)
      && Number(point.latitude) >= -90
      && Number(point.latitude) <= 90
      && Number(point.longitude) >= -180
      && Number(point.longitude) <= 180;
  }

  private backendDetail(error: HttpErrorResponse): string {
    return '';
  }

  private backendTitle(error: HttpErrorResponse): string {
    return '';
  }

  private newTourTravelTimes() {
    return [
      { transportType: 'Walking' as const, minutes: Number(this.newTour.walkingMinutes) },
      { transportType: 'Bicycle' as const, minutes: Number(this.newTour.bicycleMinutes) },
      { transportType: 'Car' as const, minutes: Number(this.newTour.carMinutes) }
    ].filter(time => Number.isFinite(time.minutes) && time.minutes > 0);
  }

  private editorTravelTimes(editor: TourDetailsEditor) {
    return [
      { transportType: 'Walking' as const, minutes: Number(editor.walkingMinutes) },
      { transportType: 'Bicycle' as const, minutes: Number(editor.bicycleMinutes) },
      { transportType: 'Car' as const, minutes: Number(editor.carMinutes) }
    ].filter(time => Number.isFinite(time.minutes) && time.minutes > 0);
  }

  private calculateRouteLengthKm(points: KeyPoint[]): number {
    const ordered = points
      .filter(point => this.hasValidCoordinates(point))
      .sort((a, b) => (a.ordinalNo ?? 0) - (b.ordinalNo ?? 0));

    if (ordered.length < 2) {
      return 0;
    }

    let total = 0;

    for (let index = 1; index < ordered.length; index++) {
      total += this.haversineDistanceKm(ordered[index - 1], ordered[index]);
    }

    return Math.round(total * 100) / 100;
  }

  private haversineDistanceKm(start: KeyPoint, end: KeyPoint): number {
    const earthRadiusKm = 6371;
    const startLat = this.degreesToRadians(start.latitude);
    const endLat = this.degreesToRadians(end.latitude);
    const deltaLat = this.degreesToRadians(end.latitude - start.latitude);
    const deltaLon = this.degreesToRadians(end.longitude - start.longitude);
    const halfChord = Math.sin(deltaLat / 2) ** 2
      + Math.cos(startLat) * Math.cos(endLat) * Math.sin(deltaLon / 2) ** 2;
    const angularDistance = 2 * Math.atan2(Math.sqrt(halfChord), Math.sqrt(1 - halfChord));

    return earthRadiusKm * angularDistance;
  }

  private degreesToRadians(value: number): number {
    return value * Math.PI / 180;
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  private readonly executionCheckIntervals: Record<number, ReturnType<typeof setInterval>> = {};

  private startExecutionPolling(tourId: number): void {
    this.stopExecutionPolling(tourId);
    this.executionCheckIntervals[tourId] = setInterval(() => this.checkKeyPoints(tourId), 10000);
  }

  private stopExecutionPolling(tourId: number): void {
    const intervalId = this.executionCheckIntervals[tourId];

    if (!intervalId) {
      return;
    }

    clearInterval(intervalId);
    delete this.executionCheckIntervals[tourId];
  }
}
