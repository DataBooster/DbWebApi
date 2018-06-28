/*!
* DbWebApi Client Angular Library
* https://github.com/databooster/dbwebapi
* Date: 2018-06-12
*/
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { DbWebApiInterceptor } from './dbwebapi-client.interceptor';

@NgModule({
	imports: [HttpClientModule],
	declarations: [],
	providers: [
		{
			provide: HTTP_INTERCEPTORS,
			useClass: DbWebApiInterceptor,
			multi: true
		}
	]
})
export class DbwebapiClientModule { }
