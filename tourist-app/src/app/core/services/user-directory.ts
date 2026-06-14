import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DirectoryUser {
  id: number;
  username: string;
  email?: string;
  role?: string;
  blocked?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserDirectoryService {
  private readonly apiUrl = `${environment.apiHost}/grpc/stakeholders/users`;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<DirectoryUser[]> {
    return this.http.get<DirectoryUser[]>(this.apiUrl);
  }
}
