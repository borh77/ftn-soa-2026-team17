import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { BlogEntry, BlogService } from '../../core/services/blog';
import { FollowerService } from '../../core/services/follower';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-blogs',
  imports: [DatePipe, FormsModule],
  templateUrl: './blogs.html',
  styleUrl: './blogs.css'
})
export class Blogs implements OnInit {
  blogs: BlogEntry[] = [];
  recommendations: number[] = [];

  canCommentByAuthorId: Record<number, boolean> = {};
  commentTextByBlogId: Record<number, string> = {};
  followLoadingByAuthorId: Record<number, boolean> = {};
  commentLoadingByBlogId: Record<number, boolean> = {};

  isLoading = false;
  message = '';
  errorMessage = '';

  constructor(
    private blogService: BlogService,
    private followerService: FollowerService,
    public authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBlogs();
    this.loadRecommendations();
  }

  loadBlogs(): void {
    this.isLoading = true;
    this.message = '';
    this.errorMessage = '';

    this.blogService.getBlogs(1, 50)
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          this.blogs = response.results ?? [];
          this.loadCanCommentState();
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Blogs could not be loaded.';
          this.cdr.detectChanges();
        }
      });
  }

  loadRecommendations(): void {
    this.followerService.getRecommendations().subscribe({
      next: recommendations => {
        this.recommendations = recommendations;
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  loadCanCommentState(): void {
    const authorIds = [...new Set(this.blogs.map(blog => blog.authorId))];

    for (const authorId of authorIds) {
      if (this.isMyOwnBlog(authorId)) {
        this.canCommentByAuthorId[authorId] = true;
        continue;
      }

      this.followerService.canComment(authorId).subscribe({
        next: canComment => {
          this.canCommentByAuthorId[authorId] = canComment;
          this.cdr.detectChanges();
        },
        error: () => {
          this.canCommentByAuthorId[authorId] = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  canComment(authorId: number): boolean {
    return this.canCommentByAuthorId[authorId] === true;
  }

  followAuthor(authorId: number): void {
    this.message = '';
    this.errorMessage = '';
    this.followLoadingByAuthorId[authorId] = true;
    this.cdr.detectChanges();

    this.followerService.follow(authorId)
      .pipe(finalize(() => {
        this.followLoadingByAuthorId[authorId] = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: () => {
          this.canCommentByAuthorId[authorId] = true;
          this.message = `You are now following user ${authorId}.`;
          this.loadRecommendations();
          this.cdr.detectChanges();
        },
        error: error => {
          const backendMessage = error?.error?.error || error?.error?.message;
          this.errorMessage = backendMessage || 'Could not follow this author.';
          this.cdr.detectChanges();
        }
      });
  }

  addComment(blog: BlogEntry): void {
    const text = (this.commentTextByBlogId[blog.id] ?? '').trim();

    if (!text) {
      this.errorMessage = 'Comment text is required.';
      return;
    }

    this.commentLoadingByBlogId[blog.id] = true;
    this.message = '';
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.blogService.addComment(blog.id, text)
      .pipe(finalize(() => {
        this.commentLoadingByBlogId[blog.id] = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: comment => {
          blog.comments = [comment, ...(blog.comments ?? [])];
          this.commentTextByBlogId[blog.id] = '';
          this.message = 'Comment added.';
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Comment could not be added.';
          this.cdr.detectChanges();
        }
      });
  }

  isMyOwnBlog(authorId: number): boolean {
    return this.authService.getPersonId() === authorId;
  }
}