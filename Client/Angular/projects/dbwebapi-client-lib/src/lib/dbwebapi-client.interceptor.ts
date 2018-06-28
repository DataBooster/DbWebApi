/*!
* DbWebApi Client Angular Library
* https://github.com/databooster/dbwebapi
* Date: 2018-06-12
*/
import { Injectable } from "@angular/core";
import { HttpRequest, HttpInterceptor, HttpHandler, HttpEvent, HttpErrorResponse } from "@angular/common/http";
import { Observable, throwError } from "rxjs";
import { catchError } from 'rxjs/operators';
import { DbError } from './dbwebapi-client.model';

@Injectable()
export class DbWebApiInterceptor implements HttpInterceptor {

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (request.params.has('NamingCase'))
      return next.handle(request).pipe(catchError((e: HttpErrorResponse) => throwError(new DbError(e))));
    else
      return next.handle(request);
  }
}
