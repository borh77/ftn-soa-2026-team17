import {
  ChangeDetectorRef,
  Component,
  Inject,
  OnInit,
  PLATFORM_ID
} from '@angular/core';

import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  ProfileService,
  UpdateProfileRequest,
  UserProfile
} from '../../core/services/profile';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {

  profile: UserProfile | null = null;
  editMode = false;
  editForm: UpdateProfileRequest = this.emptyForm();
  successMessage = '';
  errorMessage = '';
  imageDropActive = false;

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
        this.editForm = this.toForm(response);
        this.changeDetectorRef.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  startEdit(): void {
    if (!this.profile) {
      return;
    }

    this.editForm = this.toForm(this.profile);
    this.successMessage = '';
    this.errorMessage = '';
    this.editMode = true;
  }

  cancelEdit(): void {
    this.editMode = false;
    this.successMessage = '';
    this.errorMessage = '';
  }

  saveProfile(): void {
    this.successMessage = '';
    this.errorMessage = '';

    this.profileService.updateMyProfile(this.editForm).subscribe({
      next: response => {
        this.profile = response;
        this.editForm = this.toForm(response);
        this.editMode = false;
        this.successMessage = 'Profile updated successfully.';
        this.changeDetectorRef.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Profile update failed.';
      }
    });
  }

  onImageDragOver(event: DragEvent): void {
    event.preventDefault();
    this.imageDropActive = true;
  }

  onImageDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.imageDropActive = false;
  }

  onImageDrop(event: DragEvent): void {
    event.preventDefault();
    this.imageDropActive = false;
    this.errorMessage = '';

    const file = Array.from(event.dataTransfer?.files ?? [])
      .find(item => item.type.startsWith('image/'));

    if (!file) {
      const droppedText = event.dataTransfer?.getData('text/plain') ?? '';
      this.editForm.profileImage = droppedText.trim();
      return;
    }

    this.resizeImage(file)
      .then(image => {
        this.editForm.profileImage = image;
        this.changeDetectorRef.detectChanges();
      })
      .catch(() => {
        this.errorMessage = 'Could not process this image. Try another one or paste an image URL.';
      });
  }

  clearProfileImage(): void {
    this.editForm.profileImage = '';
  }

  initials(): string {
    if (!this.profile) {
      return '?';
    }

    const first = this.profile.firstName?.[0] ?? this.profile.username?.[0] ?? '';
    const last = this.profile.lastName?.[0] ?? '';

    return `${first}${last}`.toUpperCase() || '?';
  }

  private toForm(profile: UserProfile): UpdateProfileRequest {
    return {
      firstName: profile.firstName ?? '',
      lastName: profile.lastName ?? '',
      profileImage: profile.profileImage ?? '',
      biography: profile.biography ?? '',
      motto: profile.motto ?? ''
    };
  }

  private emptyForm(): UpdateProfileRequest {
    return {
      firstName: '',
      lastName: '',
      profileImage: '',
      biography: '',
      motto: ''
    };
  }

  private resizeImage(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onerror = () => reject();
      reader.onload = () => {
        const image = new Image();

        image.onerror = () => reject();
        image.onload = () => {
          const maxSide = 720;
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
          resolve(canvas.toDataURL('image/jpeg', 0.76));
        };

        image.src = String(reader.result);
      };

      reader.readAsDataURL(file);
    });
  }
}
