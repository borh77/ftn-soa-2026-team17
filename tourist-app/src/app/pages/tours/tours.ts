import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AfterViewInit,
  Component,
  Inject,
  OnDestroy,
  OnInit,
  PLATFORM_ID
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../core/services/auth';
import { KeyPoint, Tour, TourReview, TourService, TourTravelTime } from '../../core/services/tour';

interface TourTravelTimeForm {
  walkingMinutes: number | null;
  bicycleMinutes: number | null;
  carMinutes: number | null;
}

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

@Component({
  selector: 'app-tours',
  imports: [CommonModule, FormsModule],
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

  selectedDraftPointIndex = 0;
  activeKeyPointEditors: Record<number, ExistingKeyPointEditor> = {};
  keyPointMessages: Record<number, string> = {};
  keyPointErrors: Record<number, string> = {};
  keyPointImageDropActive: Record<string, boolean> = {};

  currentRole: string | null = null;
  isLoading = false;
  message = '';
  errorMessage = '';

  private readonly defaultCenter: [number, number] = [45.2526, 19.8622];

  newTour = {
    name: '',
    description: '',
    difficulty: 'Easy',
    tagsText: '',
    travelTimes: {
      walkingMinutes: null,
      bicycleMinutes: null,
      carMinutes: null
    } as TourTravelTimeForm,
    keyPoints: [
      this.emptyKeyPoint(1),
      this.emptyKeyPoint(2)
    ]
  };
  showCreateModal = false;

  private readonly isBrowser: boolean;
  private leaflet: any;
  private maps: Record<string, any> = {};
  private mapLayers: Record<string, any> = {};

  constructor(
    public authService: AuthService,
    private tourService: TourService,
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
  }

  get role(): string | null {
    return this.currentRole;
  }

  loadTours(): void {
    this.message = '';
    this.errorMessage = '';
    this.currentRole = this.authService.getUserRole();

    if (!this.currentRole) {
      this.isLoading = false;
      this.errorMessage = 'Uloguj se da bi videla ture.';
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

    const travelTimes = this.buildTravelTimes();

    if (travelTimes.length === 0) {
      this.errorMessage = 'Unesi bar jedno vreme obilaska za peške, bicikl ili automobil.';
      return;
    }

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
      travelTimes,
      keyPoints
    }).subscribe({
      next: tour => {
        this.message = `Tura "${tour.name}" je sacuvana kao draft.`;
        this.resetTourForm();
        this.showCreateModal = false;
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

  archiveTour(tour: Tour): void {
    this.message = '';
    this.errorMessage = '';

    this.tourService.archiveTour(tour.id).subscribe({
      next: () => {
        this.message = `Tura "${tour.name}" je arhivirana.`;
        this.loadGuideTours();
      },
      error: () => {
        this.errorMessage = 'Tura nije arhivirana. Arhiviranje je dozvoljeno samo za objavljene ture.';
      }
    });
  }

  reactivateTour(tour: Tour): void {
    this.message = '';
    this.errorMessage = '';

    this.tourService.reactivateTour(tour.id).subscribe({
      next: () => {
        this.message = `Tura "${tour.name}" je ponovo aktivirana.`;
        this.loadGuideTours();
      },
      error: () => {
        this.errorMessage = 'Tura nije reaktivirana. Reaktivacija je dozvoljena samo za arhivirane ture.';
      }
    });
  }

  addKeyPoint(): void {
    this.newTour.keyPoints.push(this.emptyKeyPoint(this.newTour.keyPoints.length + 1));
    this.selectedDraftPointIndex = this.newTour.keyPoints.length - 1;
    this.scheduleMapRender();
  }

  removeKeyPoint(index: number): void {
    if (this.newTour.keyPoints.length <= 2) {
      this.errorMessage = 'Za publish ture su potrebne bar dve kljucne tacke.';
      return;
    }

    this.newTour.keyPoints.splice(index, 1);
    this.newTour.keyPoints.forEach((point, pointIndex) => point.ordinalNo = pointIndex + 1);
    this.selectedDraftPointIndex = Math.min(this.selectedDraftPointIndex, this.newTour.keyPoints.length - 1);
    this.scheduleMapRender();
  }

  selectDraftPoint(index: number): void {
    this.selectedDraftPointIndex = index;
    this.scheduleMapRender();
  }

  onDraftCoordinateChange(): void {
    this.scheduleMapRender();
  }

  toggleReviews(tour: Tour): void {
    if (this.reviewsByTourId[tour.id]) {
      delete this.reviewsByTourId[tour.id];
      return;
    }

    this.loadReviews(tour.id);
  }

  startAddKeyPoint(tour: Tour): void {
    const lastPoint = tour.keyPoints[tour.keyPoints.length - 1];
    this.keyPointMessages[tour.id] = 'Klikni na mapu da izaberes poziciju nove tacke.';
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
    this.keyPointMessages[tour.id] = 'Klikni na mapu da promenis koordinatu tacke.';
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
          ? 'Kljucna tacka je dodata.'
          : 'Kljucna tacka je izmenjena.';
        delete this.activeKeyPointEditors[tour.id];
        this.loadGuideTours();
      },
      error: (error: HttpErrorResponse) => {
        this.keyPointErrors[tour.id] = this.backendDetail(error) || 'Kljucna tacka nije sacuvana. Proveri naziv, opis, sliku i koordinate.';
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
        this.keyPointMessages[tour.id] = 'Kljucna tacka je obrisana.';
        this.loadGuideTours();
      },
      error: (error: HttpErrorResponse) => {
        this.keyPointErrors[tour.id] = this.backendDetail(error) || 'Kljucna tacka nije obrisana.';
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
    this.readImageDrop(event, image => {
      form.images = [...form.images, image];
    }, text => {
      form.images = [...form.images, ...this.splitLinesOrCommas(text)];
    }, () => {
      this.reviewErrors[tourId] = 'Ne mogu da obradim ovu sliku. Probaj drugu sliku ili dodaj URL.';
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

  private loadGuideTours(): void {
    this.isLoading = true;

    this.tourService.getMyTours().subscribe({
      next: response => {
        this.guideTours = response.results;
        this.isLoading = false;
        this.scheduleMapRender();
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

  private renderAllMaps(): void {
    if (!this.leaflet || this.role !== 'GUIDE') {
      return;
    }

    this.renderRouteMap(
      'draft',
      'draft-keypoint-map',
      this.newTour.keyPoints,
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
        (latitude, longitude) => {
          if (tour.status !== 'Draft') {
            this.keyPointMessages[tour.id] = 'Objavljena tura je zakljucana za izmene.';
            return;
          }

          if (!this.activeKeyPointEditors[tour.id]) {
            this.startAddKeyPoint(tour);
          }

          const activeEditor = this.activeKeyPointEditors[tour.id];
          activeEditor.point.latitude = Number(latitude.toFixed(6));
          activeEditor.point.longitude = Number(longitude.toFixed(6));
          this.keyPointMessages[tour.id] = 'Pozicija je izabrana. Popuni podatke i sacuvaj tacku.';
          this.renderAllMaps();
        },
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
        .bindPopup(`<strong>${point.ordinalNo ?? index + 1}. ${this.escapeHtml(point.name || 'Nova tacka')}</strong>`);

      if (isActivePoint) {
        marker.bindTooltip('Drag or click map to move', { permanent: true, direction: 'top' });
        marker.on('dragend', (event: any) => {
          const position = event.target.getLatLng();
          onMapClick(position.lat, position.lng);
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
      return 'Vec si ostavila recenziju za ovu turu. Ucitavam je sada.';
    }

    if (detail) {
      return detail;
    }

    return 'Recenzija nije dodata. Proveri komentar, ocenu i datum posete.';
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
      travelTimes: {
        walkingMinutes: null,
        bicycleMinutes: null,
        carMinutes: null
      },
      keyPoints: [
        this.emptyKeyPoint(1),
        this.emptyKeyPoint(2)
      ]
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

  private hasValidCoordinates(point: KeyPoint): boolean {
    return Number.isFinite(Number(point.latitude)) && Number.isFinite(Number(point.longitude));
  }

  private buildTravelTimes(): TourTravelTime[] {
    return [
      { transportType: 'Walking', minutes: this.newTour.travelTimes.walkingMinutes },
      { transportType: 'Bicycle', minutes: this.newTour.travelTimes.bicycleMinutes },
      { transportType: 'Car', minutes: this.newTour.travelTimes.carMinutes }
    ]
      .filter((item): item is TourTravelTime => typeof item.minutes === 'number' && item.minutes > 0);
  }

  visiblePublicKeyPoints(tour: Tour): KeyPoint[] {
    return tour.keyPoints.slice(0, 1);
  }

  private backendDetail(error: HttpErrorResponse): string {
    return typeof error.error?.detail === 'string' ? error.error.detail : '';
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
}
