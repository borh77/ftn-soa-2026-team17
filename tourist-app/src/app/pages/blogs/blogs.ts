import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { marked } from 'marked';
import { finalize } from 'rxjs';
import { BlogEntry, BlogService, CreateBlogRequest } from '../../core/services/blog';
import { FollowerService } from '../../core/services/follower';
import { AuthService } from '../../core/services/auth';
import { UserDirectoryService } from '../../core/services/user-directory';

@Component({
  selector: 'app-blogs',
  imports: [DatePipe, FormsModule],
  templateUrl: './blogs.html',
  styleUrl: './blogs.css'
})
export class Blogs implements OnInit {
  blogs: BlogEntry[] = [];
  recommendations: number[] = [];
  usernamesById: Record<number, string> = {};
  imageDropActive = false;
  selectedImage: string | null = null;

  canCommentByAuthorId: Record<number, boolean> = {};
  commentTextByBlogId: Record<number, string> = {};
  followLoadingByAuthorId: Record<number, boolean> = {};
  commentLoadingByBlogId: Record<number, boolean> = {};
  likeLoadingByBlogId: Record<number, boolean> = {};
  editingCommentId: number | null = null;
  editCommentText = '';
  commentActionLoadingById: Record<number, boolean> = {};

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
    private userDirectoryService: UserDirectoryService,
    public authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBlogs();
    this.loadRecommendations();
    this.loadUsernames();
  }

  loadBlogs(clearNotices = true): void {
    this.isLoading = true;

    if (clearNotices) {
      this.message = '';
      this.errorMessage = '';
    }

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
      images: this.blogImageValues()
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
          this.loadBlogs(false);
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

  loadUsernames(): void {
    this.userDirectoryService.getUsers().subscribe({
      next: users => {
        this.usernamesById = users.reduce<Record<number, string>>((lookup, user) => {
          lookup[Number(user.id)] = user.username;
          return lookup;
        }, {});
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
          this.message = `You are now following @${this.usernameFor(authorId)}.`;
          this.loadRecommendations();
          this.loadBlogs(false);
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
        next: () => {
          this.commentTextByBlogId[blog.id] = '';
          this.message = 'Comment added.';
          this.loadBlogs(false);
        },
        error: () => {
          this.errorMessage = 'Comment could not be added.';
          this.cdr.detectChanges();
        }
      });
  }

  startCommentEdit(comment: { id: number; text: string }): void {
    this.editingCommentId = comment.id;
    this.editCommentText = comment.text;
    this.message = '';
    this.errorMessage = '';
  }

  cancelCommentEdit(): void {
    this.editingCommentId = null;
    this.editCommentText = '';
  }

  updateComment(blog: BlogEntry, commentId: number): void {
    const text = this.editCommentText.trim();

    if (!text) {
      this.errorMessage = 'Comment text is required.';
      return;
    }

    this.commentActionLoadingById[commentId] = true;
    this.message = '';
    this.errorMessage = '';

    this.blogService.updateComment(blog.id, commentId, { text })
      .pipe(finalize(() => {
        this.commentActionLoadingById[commentId] = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: () => {
          this.cancelCommentEdit();
          this.message = 'Comment updated.';
          this.loadBlogs(false);
        },
        error: error => {
          const backendMessage = error?.error?.detail || error?.error?.message;
          this.errorMessage = backendMessage || 'Comment could not be updated.';
          this.cdr.detectChanges();
        }
      });
  }

  deleteComment(blog: BlogEntry, commentId: number): void {
    this.commentActionLoadingById[commentId] = true;
    this.message = '';
    this.errorMessage = '';

    this.blogService.deleteComment(blog.id, commentId)
      .pipe(finalize(() => {
        this.commentActionLoadingById[commentId] = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: () => {
          if (this.editingCommentId === commentId) {
            this.cancelCommentEdit();
          }

          this.message = 'Comment deleted.';
          this.loadBlogs(false);
        },
        error: error => {
          const backendMessage = error?.error?.detail || error?.error?.message;
          this.errorMessage = backendMessage || 'Comment could not be deleted.';
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
          this.message = blog.isLikedByCurrentUser ? 'Like removed.' : 'Blog liked.';
          this.loadBlogs(false);
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

  isMyComment(authorId: number): boolean {
    return this.authService.getPersonId() === authorId;
  }

  usernameFor(userId: number): string {
    if (this.authService.getPersonId() === userId) {
      return this.authService.getUsername() ?? this.usernamesById[userId] ?? `user-${userId}`;
    }

    return this.usernamesById[userId] ?? `user-${userId}`;
  }

  blogImageAlt(index: number, blogTitle: string): string {
    return `Image ${index + 1} for ${blogTitle}`;
  }

  openImage(image: string): void {
    this.selectedImage = image;
  }

  closeImage(): void {
    this.selectedImage = null;
  }

  @HostListener('document:keydown.escape')
  closeImageOnEscape(): void {
    this.closeImage();
  }

  onBlogImageDragOver(event: DragEvent): void {
    event.preventDefault();
    this.imageDropActive = true;
  }

  onBlogImageDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.imageDropActive = false;
  }

  onBlogImageDrop(event: DragEvent): void {
    event.preventDefault();
    this.imageDropActive = false;
    this.errorMessage = '';

    const files = Array.from(event.dataTransfer?.files ?? [])
      .filter(item => item.type.startsWith('image/'));
    const droppedText = event.dataTransfer?.getData('text/plain') ?? '';

    if (files.length === 0 && droppedText.trim()) {
      this.addBlogImages(this.parseBlogImages(droppedText));
      return;
    }

    if (files.length === 0) {
      this.errorMessage = 'Drop an image file or paste an image URL.';
      return;
    }

    files.forEach(file => {
      this.resizeImage(file)
        .then(image => {
          this.addBlogImages([image]);
          this.cdr.detectChanges();
        })
        .catch(() => {
          this.errorMessage = 'Could not process this image. Try another one or paste an image URL.';
          this.cdr.detectChanges();
        });
    });
  }

  onBlogImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? [])
      .filter(item => item.type.startsWith('image/'));

    this.errorMessage = '';

    if (files.length === 0) {
      return;
    }

    files.forEach(file => {
      this.resizeImage(file)
        .then(image => {
          this.addBlogImages([image]);
          this.cdr.detectChanges();
        })
        .catch(() => {
          this.errorMessage = 'Could not process this image. Try another one or paste an image URL.';
          this.cdr.detectChanges();
        });
    });

    input.value = '';
  }

  removeBlogImage(index: number): void {
    const images = this.blogImageValues();
    images.splice(index, 1);
    this.newBlog.imageUrlsText = images.join('\n');
  }

  clearBlogImages(): void {
    this.newBlog.imageUrlsText = '';
  }

  blogImageValues(): string[] {
    return this.parseBlogImages(this.newBlog.imageUrlsText);
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

  private addBlogImages(images: string[]): void {
    const nextImages = [...this.blogImageValues(), ...images.map(image => image.trim()).filter(Boolean)];
    this.newBlog.imageUrlsText = nextImages.join('\n');
  }

  private parseBlogImages(value: string): string[] {
    return value
      .split('\n')
      .flatMap(line => {
        const trimmed = line.trim();

        if (!trimmed) {
          return [];
        }

        return trimmed.startsWith('data:image/')
          ? [trimmed]
          : trimmed.split(',').map(item => item.trim()).filter(Boolean);
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
