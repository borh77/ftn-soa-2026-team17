import {
  ChangeDetectorRef,
  Component,
  Inject,
  OnInit,
  PLATFORM_ID
} from '@angular/core';

import { CommonModule, isPlatformBrowser } from '@angular/common';

import {
  ProfileService,
  UserProfile
} from '../../core/services/profile';

@Component({
  selector: 'app-profile',
  imports: [CommonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {

  profile: UserProfile | null = null;

  constructor(
    private profileService: ProfileService,
    @Inject(PLATFORM_ID) private platformId: object,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadProfile();
    }
  }

  loadProfile(): void {
    this.profileService.getMyProfile().subscribe({
      next: (response) => {
        this.profile = response;
        this.changeDetectorRef.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }
}