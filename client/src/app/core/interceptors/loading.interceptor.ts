import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, delay, finalize, identity } from 'rxjs';
import { BusyService } from '../services/busy.service';
import { environment } from '../../../environments/environment';

@Injectable()
export class loadingInterceptor implements HttpInterceptor {
  constructor(private busyService: BusyService) {}

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    if (
      req.url.includes('emailExists') ||
      (req.method === 'POST' && req.url.includes('orders')) ||
      req.method === 'DELETE'
    ) {
      return next.handle(req);
    }

    this.busyService.busy();
    return next.handle(req).pipe(
      environment.production ? identity : delay(1000),
      finalize(() => this.busyService.idle())
    );
  }
}
// export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
//   return next(req);
// };
