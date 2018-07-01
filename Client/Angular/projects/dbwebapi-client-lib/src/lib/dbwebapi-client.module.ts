/*!
* DbWebApi Client Angular Library
* https://github.com/databooster/dbwebapi
* Date: 2018-06-12
*/
import { NgModule, ModuleWithProviders, Optional, SkipSelf } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { DbWebApiInterceptor } from './dbwebapi-client.interceptor';

@NgModule({
  imports: [HttpClientModule]
})
export class DbwebapiClientModule {
  constructor( @Optional() @SkipSelf() parentModule: DbwebapiClientModule) {
    if (parentModule) {
      throw new Error('DbwebapiClientModule is already loaded. Import it in the AppModule only');
    }
  }

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
