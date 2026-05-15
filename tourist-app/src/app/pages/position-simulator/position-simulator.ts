import {
  AfterViewInit,
  Component,
  Inject,
  PLATFORM_ID
} from '@angular/core';

import { isPlatformBrowser } from '@angular/common';

import {
  PositionSimulatorService,
  TouristPosition
} from '../../core/services/position-simulator';

@Component({
  selector: 'app-position-simulator',
  imports: [],
  templateUrl: './position-simulator.html',
  styleUrl: './position-simulator.css',
})
export class PositionSimulator implements AfterViewInit {
  private map: any;
  private marker: any;
  private leaflet: any;

  currentPosition: TouristPosition | null = null;

  constructor(
    private positionService: PositionSimulatorService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.leaflet = await import('leaflet');

    delete (this.leaflet.Icon.Default.prototype as any)._getIconUrl;

  this.leaflet.Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  });

    this.initializeMap();
    this.loadCurrentPosition();
  }

  private initializeMap(): void {
    this.map = this.leaflet.map('map').setView([45.2671, 19.8335], 13);

    this.leaflet.tileLayer(
      'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
      {
        attribution: '&copy; OpenStreetMap contributors'
      }
    ).addTo(this.map);

    this.map.on('click', (e: any) => {
      this.savePosition(e.latlng.lat, e.latlng.lng);
    });
  }

  private loadCurrentPosition(): void {
    this.positionService.getMyPosition().subscribe({
      next: (position) => {
        if (!position) {
          return;
        }

        this.currentPosition = position;
        this.setMarker(position.latitude, position.longitude);

        this.map.setView(
          [position.latitude, position.longitude],
          13
        );
      },
      error: (err: unknown) => {
        console.error(err);
      }
    });
  }

  private savePosition(latitude: number, longitude: number): void {
    this.positionService.updateMyPosition({
      latitude,
      longitude
    }).subscribe({
      next: (position) => {
        this.currentPosition = position;
        this.setMarker(latitude, longitude);
      },
      error: (err: unknown) => {
        console.error(err);
      }
    });
  }

  private setMarker(latitude: number, longitude: number): void {
    if (this.marker) {
      this.map.removeLayer(this.marker);
    }

    this.marker = this.leaflet.marker([latitude, longitude])
      .addTo(this.map);
  }
}