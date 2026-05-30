import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { marked } from 'marked';
import { AuthService } from '../../core/services/auth';
import {
  BlogComment,
  BlogEntry,
  BlogService,
  CreateCommentRequest,
  CreateBlogRequest
} from '../../core/services/blog';

@Component({
  selector: 'app-blogs',
  imports: [CommonModule, FormsModule],
  templateUrl: './blogs.html',
  styleUrl: './blogs.css'
})
export class Blogs implements OnInit {
  blogs: BlogEntry[] = [];
  currentRole: string | null = null;
  isLoading = false;
  message = '';
  errorMessage = '';
  showCreateForm = false;
  createSubmitting = false;
  commentSubmitting: Record<number, boolean> = {};
  likeSubmitting: Record<number, boolean> = {};
  commentDrafts: Record<number, string> = {};

  newBlog = {
    title: '',
    description: '',
    creationDate: new Date().toISOString().slice(0, 16),
    imageUrlsText: ''
  };

  constructor(
    public authService: AuthService,
    private blogService: BlogService
  ) {}

  ngOnInit(): void {
    this.currentRole = this.authService.getUserRole();
    this.loadBlogs();
  }

  get role(): string | null {
    return this.currentRole;
  }

  loadBlogs(): void {
    this.message = '';
    this.errorMessage = '';
    this.isLoading = true;

    this.blogService.getBlogs(1, 50).subscribe({
      next: response => {
        this.blogs = response.results;
        this.isLoading = false;
      },
      error: () => {
        this.blogs = [];
        this.errorMessage = 'Ne mogu da ucitam blogove. Proveri da li je backend podignut i da li si ulogovana kao turista.';
        this.isLoading = false;
      }
    });
  }

  createBlog(): void {
    this.message = '';
    this.errorMessage = '';
    this.createSubmitting = true;

    const request: CreateBlogRequest = {
      title: this.newBlog.title.trim(),
      description: this.newBlog.description.trim(),
      creationDate: new Date(this.newBlog.creationDate).toISOString(),
      images: this.splitLinesOrCommas(this.newBlog.imageUrlsText)
    };

    if (!request.title || !request.description) {
      this.errorMessage = 'Naslov i opis su obavezni.';
      this.createSubmitting = false;
      return;
    }

    if (Number.isNaN(new Date(request.creationDate).getTime())) {
      this.errorMessage = 'Izaberi validan datum kreiranja.';
      this.createSubmitting = false;
      return;
    }

    this.blogService.createBlog(request).subscribe({
      next: blog => {
        this.message = `Blog "${blog.title}" je sacuvan.`;
        this.resetCreateForm();
        this.showCreateForm = false;
        this.createSubmitting = false;
        this.loadBlogs();
      },
      error: () => {
        this.errorMessage = 'Blog nije kreiran. Proveri naslov, opis i datum.';
        this.createSubmitting = false;
      }
    });
  }

  toggleLike(blog: BlogEntry): void {
    this.message = '';
    this.errorMessage = '';
    this.likeSubmitting[blog.id] = true;

    const request = blog.isLikedByCurrentUser
      ? this.blogService.unlikeBlog(blog.id)
      : this.blogService.likeBlog(blog.id);

    request.subscribe({
      next: () => {
        this.likeSubmitting[blog.id] = false;
        this.message = blog.isLikedByCurrentUser
          ? `Lajk za "${blog.title}" je uklonjen.`
          : `Blog "${blog.title}" je lajkovan.`;
        this.loadBlogs();
      },
      error: () => {
        this.likeSubmitting[blog.id] = false;
        this.errorMessage = 'Ne mogu da promenim lajk. Proveri da li si vec lajkovala ovu objavu.';
      }
    });
  }

  submitComment(blog: BlogEntry): void {
    this.message = '';
    this.errorMessage = '';

    const text = (this.commentDrafts[blog.id] ?? '').trim();

    if (!text) {
      this.errorMessage = 'Komentar ne sme da bude prazan.';
      return;
    }

    this.commentSubmitting[blog.id] = true;

    const request: CreateCommentRequest = { text };

    this.blogService.addComment(blog.id, request).subscribe({
      next: () => {
        this.commentSubmitting[blog.id] = false;
        this.commentDrafts[blog.id] = '';
        this.message = 'Komentar je dodat.';
        this.loadBlogs();
      },
      error: () => {
        this.commentSubmitting[blog.id] = false;
        this.errorMessage = 'Komentar nije dodat. Proveri tekst komentara.';
      }
    });
  }

  getCommentDraft(blogId: number): string {
    return this.commentDrafts[blogId] ?? '';
  }

  renderMarkdown(value: string): string {
    return value ? (marked.parse(value) as string) : '<p class="muted">Nema opisa.</p>';
  }

  commentCount(blog: BlogEntry): number {
    return blog.comments?.length ?? 0;
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

  blogImageAlt(index: number, blogTitle: string): string {
    return `Image ${index + 1} for ${blogTitle}`;
  }
}
