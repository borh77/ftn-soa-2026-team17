import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { marked } from 'marked';
import { finalize } from 'rxjs';
import { BlogEntry, BlogService, CreateBlogRequest } from '../../core/services/blog';
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
  likeLoadingByBlogId: Record<number, boolean> = {};

  isLoading = false;
  showCreateForm = false;
  createLoading = false;
  message = '';
  errorMessage = '';

  newBlog = {
    title: '',
    description: '',
    creationDate: new Date().toISOString().slice(0, 16),
    imageUrlsText: ''
  };

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

  createBlog(): void {
    const creationDate = new Date(this.newBlog.creationDate);
    const request: CreateBlogRequest = {
      title: this.newBlog.title.trim(),
      description: this.newBlog.description.trim(),
      creationDate: '',
      images: this.splitLinesOrCommas(this.newBlog.imageUrlsText)
    };

    this.message = '';
    this.errorMessage = '';

    if (!request.title || !request.description) {
      this.errorMessage = 'Title and description are required.';
      return;
    }

    if (Number.isNaN(creationDate.getTime())) {
      this.errorMessage = 'Creation date is not valid.';
      return;
    }

    request.creationDate = creationDate.toISOString();

    this.createLoading = true;
    this.cdr.detectChanges();

    this.blogService.createBlog(request)
      .pipe(finalize(() => {
        this.createLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: blog => {
          this.message = `Blog "${blog.title}" was created.`;
          this.resetCreateForm();
          this.showCreateForm = false;
          this.loadBlogs();
        },
        error: () => {
          this.errorMessage = 'Blog could not be created.';
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

    this.blogService.addComment(blog.id, { text })
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

  toggleLike(blog: BlogEntry): void {
    this.message = '';
    this.errorMessage = '';
    this.likeLoadingByBlogId[blog.id] = true;
    this.cdr.detectChanges();

    const request = blog.isLikedByCurrentUser
      ? this.blogService.unlikeBlog(blog.id)
      : this.blogService.likeBlog(blog.id);

    request
      .pipe(finalize(() => {
        this.likeLoadingByBlogId[blog.id] = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: () => {
          blog.isLikedByCurrentUser = !blog.isLikedByCurrentUser;
          blog.likeCount += blog.isLikedByCurrentUser ? 1 : -1;
          this.message = blog.isLikedByCurrentUser ? 'Blog liked.' : 'Like removed.';
          this.cdr.detectChanges();
        },
        error: error => {
          const backendMessage = error?.error?.error || error?.error?.message;
          this.errorMessage = backendMessage || 'Like could not be changed.';
          this.cdr.detectChanges();
        }
      });
  }

  renderMarkdown(value: string): string {
    return value ? (marked.parse(value) as string) : '<p class="muted">No description.</p>';
  }

  commentCount(blog: BlogEntry): number {
    return blog.comments?.length ?? 0;
  }

  isMyOwnBlog(authorId: number): boolean {
    return this.authService.getPersonId() === authorId;
  }

  blogImageAlt(index: number, blogTitle: string): string {
    return `Image ${index + 1} for ${blogTitle}`;
  }

  private resetCreateForm(): void {
    this.newBlog = {
      title: '',
      description: '',
      creationDate: new Date().toISOString().slice(0, 16),
      imageUrlsText: ''
    };
  }

  private splitLinesOrCommas(value: string): string[] {
    return value
      .split(/[\n,]/)
      .map(item => item.trim())
      .filter(Boolean);
  }
}
