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

export const enum JsonTimeZoneKind {
  Unspecified = 0,
  UTC = 1,
  TimeOffset = 2
}

declare global {
  interface Date {
    toTzString(): string;
    toNonTzString(): string;
  }

  interface DateConstructor {
    jsonTimeZoneKind: JsonTimeZoneKind;
  }
}

Date.jsonTimeZoneKind = JsonTimeZoneKind.TimeOffset;

Date.prototype.toNonTzString = function (this: Date): string {
  let lt = new Date(this.getTime() - this.getTimezoneOffset() * 60000);
  let uz = lt.toISOString();
  return uz.substr(0, uz.length - 1);
}

Date.prototype.toTzString = function (this: Date): string {
  let tzOffset = this.getTimezoneOffset();
  let sign = tzOffset > 0 ? '-' : '+';
  let absOffset = Math.abs(tzOffset);
  let zh = Math.floor(absOffset / 60 + 100).toString().substr(1);
  let zm = (absOffset % 60 + 100).toString().substr(1);
  return this.toNonTzString() + sign + zh + ':' + zm;
}

Date.prototype.toJSON = function (this: Date): string {
  switch (Date.jsonTimeZoneKind) {
    case JsonTimeZoneKind.UTC:
      return this.toISOString();
    case JsonTimeZoneKind.TimeOffset:
      return this.toTzString();
    default:
      return this.toNonTzString();
  }
}
