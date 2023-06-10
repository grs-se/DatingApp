import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { delay, finalize, identity, Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';
import { environment } from '../../environments/environment';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.busyService.busy();

    return next.handle(request).pipe(
      // can't use null so use identity which doesn't do anything
      // and effectively returns nothing.
      (environment.production ? identity : delay(1000)),
      finalize(() => {
        this.busyService.idle()
      })
    )
  }
}
