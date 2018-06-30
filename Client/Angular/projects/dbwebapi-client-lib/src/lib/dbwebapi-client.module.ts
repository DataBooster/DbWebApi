/*!
* DbWebApi Client Angular Library
* https://github.com/databooster/dbwebapi
* Date: 2018-06-12
*/
import { NgModule, ModuleWithProviders } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { DbWebApiInterceptor } from './dbwebapi-client.interceptor';

@NgModule({
  imports: [HttpClientModule]
})
export class DbwebapiClientModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: DbwebapiClientModule,
      providers: [
        {
          provide: HTTP_INTERCEPTORS,
          useClass: DbWebApiInterceptor,
          multi: true
        }
      ]
    }
  }
}
