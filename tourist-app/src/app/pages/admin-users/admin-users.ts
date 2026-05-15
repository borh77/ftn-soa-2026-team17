import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule } from '@angular/common';

import { isPlatformBrowser } from '@angular/common';

import {
  AdminUserService,
  UserAccount
} from '../../core/services/admin-user';

@Component({
  selector: 'app-admin-users',
  imports: [CommonModule],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css',
})
export class AdminUsers implements OnInit {

  users: UserAccount[] = [];

  constructor(
    private adminUserService: AdminUserService,
    @Inject(PLATFORM_ID) private platformId: object,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadUsers();
    }
  }

  loadUsers(): void {
    this.adminUserService.getUsers().subscribe({
      next: (response) => {
        this.users = response;
        this.changeDetectorRef.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  blockUser(id: number): void {
    this.adminUserService.blockUser(id).subscribe({
      next: () => {
        this.loadUsers();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }
}