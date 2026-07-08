import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { NewTask, Task, TaskStats } from '../models/task.model';

/**
 * The HTTP client for the Task API backend (examples/aspnet-api-project). This is the seam the store
 * loads and persists through - the same `load()` contract the in-memory stand-in used to expose, now
 * backed by real REST calls. Kept as the single injection point, so a semantic caller lookup on
 * `TaskApiService` resolves the one binding the store holds.
 */
@Injectable({ providedIn: 'root' })
export class TaskApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tasks`;

  /** GET /api/tasks - the full task list. */
  load(): Observable<Task[]> {
    return this.http.get<Task[]>(this.base);
  }

  /** POST /api/tasks - create a task; the server assigns the id and timestamps. */
  create(task: NewTask): Observable<Task> {
    return this.http.post<Task>(this.base, task);
  }

  /** PUT /api/tasks/:id - replace a task's editable fields; returns the server copy. */
  update(id: string, task: Task): Observable<Task> {
    return this.http.put<Task>(`${this.base}/${id}`, task);
  }

  /** DELETE /api/tasks/:id. */
  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  /** GET /api/tasks/stats - the dashboard aggregate. */
  stats(): Observable<TaskStats> {
    return this.http.get<TaskStats>(`${this.base}/stats`);
  }
}
