import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  Inject,
  OnDestroy,
  OnInit,
  PLATFORM_ID
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { KeyPoint, Tour, TourService, TourTravelTime } from '../../core/services/tour';

@Component({
  selector: 'app-tour-editor',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './tour-editor.html',
  styleUrl: './tour-editor.css'
})
export class TourEditor implements OnInit, AfterViewInit, OnDestroy {
  tourId: number | null = null;
  sourceTour: Tour | null = null;
  selectedPointIndex = 0;
  imageDropActive: Record<number, boolean> = {};
  isLoading = false;
  isSaving = false;
  errorMessage = '';

  form = {
    name: '',
    description: '',
    difficulty: 'Easy',
    tagsText: '',
    walkingMinutes: 0,
    bicycleMinutes: 0,
    carMinutes: 0,
    keyPoints: [] as KeyPoint[]
  };

  private readonly defaultCenter: [number, number] = [45.2526, 19.8622];
  private readonly isBrowser: boolean;
  private leaflet: any;
  private map: any;
  private mapLayer: any;

  constructor(
    private tourService: TourService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  get isEditMode(): boolean {
    return this.tourId !== null;
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (Number.isFinite(id) && id > 0) {
      this.tourId = id;
      this.loadTour(id);
    }
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
    this.map?.remove();
  }

  async save(): Promise<void> {
    this.errorMessage = '';
    const keyPoints = this.normalizedKeyPoints();

    if (!this.form.name.trim() || !this.form.description.trim()) {
      this.errorMessage = 'Name and description are required.';
      return;
    }

    if (keyPoints.some(point => !this.isCompleteKeyPoint(point))) {
      this.errorMessage = 'Every key point needs a name, description, secret, image, and valid coordinates.';
      return;
    }

    this.isSaving = true;

    try {
      if (this.isEditMode && this.sourceTour) {
        await this.updateExistingTour(keyPoints);
      } else {
        await firstValueFrom(this.tourService.createTour({
          name: this.form.name.trim(),
          description: this.form.description.trim(),
          difficulty: this.form.difficulty,
          tags: this.splitValues(this.form.tagsText),
          travelTimes: this.travelTimes(),
          keyPoints
        }));
      }

      await this.router.navigate(['/tours']);
    } catch (error) {
      this.errorMessage = this.backendMessage(error as HttpErrorResponse)
        || 'The tour could not be saved.';
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }

  addPointAt(latitude: number, longitude: number): void {
    const ordinalNo = this.form.keyPoints.length + 1;
    this.form.keyPoints.push({
      ordinalNo,
      name: '',
      description: '',
      secretText: '',
      imageUrl: '',
      latitude: Number(latitude.toFixed(6)),
      longitude: Number(longitude.toFixed(6))
    });
    this.selectedPointIndex = this.form.keyPoints.length - 1;
    this.scheduleMapRender();
  }

  selectPoint(index: number): void {
    this.selectedPointIndex = index;
    this.scheduleMapRender();
  }

  removePoint(index: number): void {
    this.form.keyPoints.splice(index, 1);
    this.form.keyPoints.forEach((point, pointIndex) => point.ordinalNo = pointIndex + 1);
    this.selectedPointIndex = Math.max(0, Math.min(this.selectedPointIndex, this.form.keyPoints.length - 1));
    this.scheduleMapRender();
  }

  clearPoints(): void {
    this.form.keyPoints = [];
    this.selectedPointIndex = 0;
    this.scheduleMapRender();
  }

  onCoordinateChange(): void {
    this.scheduleMapRender();
  }

  routeLengthKm(): number {
    const points = this.normalizedKeyPoints();
    let total = 0;

    for (let index = 1; index < points.length; index++) {
      total += this.distanceKm(points[index - 1], points[index]);
    }

    return Math.round(total * 100) / 100;
  }

  onImageDragOver(event: DragEvent, index: number): void {
    event.preventDefault();
    this.imageDropActive[index] = true;
  }

  onImageDragLeave(event: DragEvent, index: number): void {
    event.preventDefault();
    this.imageDropActive[index] = false;
  }

  onImageDrop(event: DragEvent, point: KeyPoint, index: number): void {
    event.preventDefault();
    this.imageDropActive[index] = false;
    const file = Array.from(event.dataTransfer?.files ?? [])
      .find(item => item.type.startsWith('image/'));

    if (!file) {
      point.imageUrl = (event.dataTransfer?.getData('text/plain') ?? '').trim();
      return;
    }

    this.resizeImage(file).then(image => {
      point.imageUrl = image;
      this.cdr.detectChanges();
    }).catch(() => {
      this.errorMessage = 'The image could not be processed.';
    });
  }

  onImageSelected(event: Event, point: KeyPoint): void {
    const input = event.target as HTMLInputElement;
    const file = Array.from(input.files ?? []).find(item => item.type.startsWith('image/'));

    if (!file) {
      return;
    }

    this.resizeImage(file).then(image => {
      point.imageUrl = image;
      input.value = '';
      this.cdr.detectChanges();
    }).catch(() => {
      this.errorMessage = 'The image could not be processed.';
    });
  }

  private loadTour(id: number): void {
    this.isLoading = true;
    this.tourService.getMyTours(1, 100).subscribe({
      next: response => {
        const tour = response.results.find(item => item.id === id);

        if (!tour) {
          this.errorMessage = 'Tour not found.';
        } else if (tour.status !== 'Draft') {
          this.errorMessage = 'Only draft tours can be edited.';
        } else {
          this.sourceTour = tour;
          this.form = {
            name: tour.name,
            description: tour.description,
            difficulty: tour.difficulty,
            tagsText: tour.tags.join(', '),
            walkingMinutes: this.minutesFor(tour, 'Walking'),
            bicycleMinutes: this.minutesFor(tour, 'Bicycle'),
            carMinutes: this.minutesFor(tour, 'Car'),
            keyPoints: tour.keyPoints.map(point => ({ ...point }))
          };
        }

        this.isLoading = false;
        this.scheduleMapRender();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'The tour could not be loaded.';
        this.cdr.detectChanges();
      }
    });
  }

  private async updateExistingTour(points: KeyPoint[]): Promise<void> {
    if (!this.tourId || !this.sourceTour) {
      return;
    }

    await firstValueFrom(this.tourService.updateTour(this.tourId, {
      name: this.form.name.trim(),
      description: this.form.description.trim(),
      difficulty: this.form.difficulty,
      tags: this.splitValues(this.form.tagsText),
      price: 0,
      travelTimes: this.travelTimes()
    }));

    const commonCount = Math.min(this.sourceTour.keyPoints.length, points.length);

    for (let index = 0; index < commonCount; index++) {
      await firstValueFrom(this.tourService.updateKeyPoint(this.tourId, index + 1, points[index]));
    }

    for (let ordinal = this.sourceTour.keyPoints.length; ordinal > points.length; ordinal--) {
      await firstValueFrom(this.tourService.deleteKeyPoint(this.tourId, ordinal));
    }

    for (let index = commonCount; index < points.length; index++) {
      await firstValueFrom(this.tourService.addKeyPoint(this.tourId, points[index]));
    }
  }

  private renderMap(): void {
    const element = document.getElementById('tour-editor-map');

    if (!element || !this.leaflet) {
      return;
    }

    if (!this.map) {
      this.map = this.leaflet.map(element).setView(this.defaultCenter, 13);
      this.leaflet.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
      }).addTo(this.map);
      this.mapLayer = this.leaflet.layerGroup().addTo(this.map);
    }

    this.mapLayer.clearLayers();
    this.map.off('click');
    this.map.on('click', (event: any) => this.addPointAt(event.latlng.lat, event.latlng.lng));

    const points = this.normalizedKeyPoints();
    const coordinates = points.map(point => [point.latitude, point.longitude]);

    points.forEach((point, index) => {
      const marker = this.leaflet.marker([point.latitude, point.longitude], {
        draggable: index === this.selectedPointIndex
      }).bindPopup(`<strong>${index + 1}. ${this.escapeHtml(point.name || 'New point')}</strong>`);

      if (index === this.selectedPointIndex) {
        marker.on('dragend', (event: any) => {
          const position = event.target.getLatLng();
          this.form.keyPoints[index].latitude = Number(position.lat.toFixed(6));
          this.form.keyPoints[index].longitude = Number(position.lng.toFixed(6));
          this.renderMap();
        });
      }

      marker.addTo(this.mapLayer);
    });

    if (coordinates.length >= 2) {
      this.leaflet.polyline(coordinates, { color: '#e76f51', weight: 4 }).addTo(this.mapLayer);
    }

    if (coordinates.length) {
      this.map.fitBounds(this.leaflet.latLngBounds(coordinates), { padding: [24, 24], maxZoom: 15 });
    }

    setTimeout(() => this.map.invalidateSize(), 0);
  }

  private scheduleMapRender(): void {
    if (this.isBrowser) {
      setTimeout(() => this.renderMap(), 0);
    }
  }

  private normalizedKeyPoints(): KeyPoint[] {
    return this.form.keyPoints.map((point, index) => ({
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

  private travelTimes(): TourTravelTime[] {
    return [
      { transportType: 'Walking' as const, minutes: Number(this.form.walkingMinutes) },
      { transportType: 'Bicycle' as const, minutes: Number(this.form.bicycleMinutes) },
      { transportType: 'Car' as const, minutes: Number(this.form.carMinutes) }
    ].filter(time => Number.isFinite(time.minutes) && time.minutes > 0);
  }

  private minutesFor(tour: Tour, type: TourTravelTime['transportType']): number {
    return tour.travelTimes.find(time => time.transportType === type)?.minutes ?? 0;
  }

  private splitValues(value: string): string[] {
    return value.split(/[\n,]/).map(item => item.trim()).filter(Boolean);
  }

  private isCompleteKeyPoint(point: KeyPoint): boolean {
    return !!point.name && !!point.description && !!point.secretText && !!point.imageUrl
      && Number.isFinite(point.latitude) && Number.isFinite(point.longitude)
      && point.latitude >= -90 && point.latitude <= 90
      && point.longitude >= -180 && point.longitude <= 180;
  }

  private distanceKm(start: KeyPoint, end: KeyPoint): number {
    const toRadians = (value: number) => value * Math.PI / 180;
    const deltaLat = toRadians(end.latitude - start.latitude);
    const deltaLon = toRadians(end.longitude - start.longitude);
    const startLat = toRadians(start.latitude);
    const endLat = toRadians(end.latitude);
    const a = Math.sin(deltaLat / 2) ** 2
      + Math.cos(startLat) * Math.cos(endLat) * Math.sin(deltaLon / 2) ** 2;
    return 6371 * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  }

  private resizeImage(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onerror = () => reject();
      reader.onload = () => {
        const image = new Image();
        image.onerror = () => reject();
        image.onload = () => {
          const scale = Math.min(1, 900 / Math.max(image.width, image.height));
          const canvas = document.createElement('canvas');
          canvas.width = Math.max(1, Math.round(image.width * scale));
          canvas.height = Math.max(1, Math.round(image.height * scale));
          const context = canvas.getContext('2d');

          if (!context) {
            reject();
            return;
          }

          context.drawImage(image, 0, 0, canvas.width, canvas.height);
          resolve(canvas.toDataURL('image/jpeg', 0.76));
        };
        image.src = String(reader.result);
      };
      reader.readAsDataURL(file);
    });
  }

  private backendMessage(error: HttpErrorResponse): string {
    return error.error?.detail || error.error?.title || error.error?.message || '';
  }

  private escapeHtml(value: string): string {
    return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
  }
}
