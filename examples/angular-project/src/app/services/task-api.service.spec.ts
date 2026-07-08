import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { NewTask, Priority, Task, TaskStatus } from '../models/task.model';
import { TaskApiService } from './task-api.service';

describe('TaskApiService', () => {
  let service: TaskApiService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiUrl}/tasks`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TaskApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('load() issues GET /api/tasks', () => {
    let result: Task[] | undefined;
    service.load().subscribe((tasks) => (result = tasks));

    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('GET');
    req.flush([{ id: '1', title: 'x' } as Task]);

    expect(result?.length).toBe(1);
  });

  it('create() POSTs the payload to /api/tasks', () => {
    const input: NewTask = { title: 'new', description: '', priority: Priority.High };
    service.create(input).subscribe();

    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(input);
    req.flush({ id: 'a', title: 'new' } as Task);
  });

  it('update() PUTs to /api/tasks/:id', () => {
    const task = { id: 'a', title: 'edit', status: TaskStatus.Done } as Task;
    service.update('a', task).subscribe();

    const req = httpMock.expectOne(`${base}/a`);
    expect(req.request.method).toBe('PUT');
    req.flush(task);
  });

  it('remove() DELETEs /api/tasks/:id', () => {
    service.remove('a').subscribe();

    const req = httpMock.expectOne(`${base}/a`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('stats() issues GET /api/tasks/stats', () => {
    service.stats().subscribe();

    const req = httpMock.expectOne(`${base}/stats`);
    expect(req.request.method).toBe('GET');
    req.flush({ total: 0, byStatus: {}, byPriority: {}, overdue: 0, completionRate: 0 });
  });
});
