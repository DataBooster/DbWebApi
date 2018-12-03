#### DbWebApi Client for Angular 6.0+

If your project is Angular 6.0 or higher, the npm package [**dbwebapi-client**](https://www.npmjs.com/package/dbwebapi-client) can be used to simplify your http client coding.
```
> npm i dbwebapi-client
```
app.module.ts
``` typescript
import { DbwebapiClientModule } from 'dbwebapi-client';

@NgModule({
  declarations: [ // ...
  ],
  imports: [ // ...
    DbwebapiClientModule.forRoot()
  ],
  providers: [ // ...
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```
Then your Angular service can inherit from DbWebApiClient class, as shown in the following example:
``` typescript
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from 'rxjs/operators';

import { DbWebApiClient, JsonTimeZoneKind } from 'dbwebapi-client';

@Injectable({ providedIn: 'root' })
export class MyDbWebApiService extends DbWebApiClient {
    constructor(_http: HttpClient) {
        super(_http, 'http://my-base-url-path.');
        Date.jsonTimeZoneKind = JsonTimeZoneKind.Unspecified;
    }

    invokeMyStoredProcedure(inParams: object): Observable<MyTypescriptModel> {
        return super.post('my_stored_procedure', inParams).pipe(map(data => new MyTypescriptModel(data.ResultSets)));
    }
}
```
_The example method will return an Observable MyTypescriptModel instance if the **constructor** of MyTypescriptModel class transforms flat result sets to local hierarchical data model._

If you need to control the details of http options (such as: credentials, headers), you can use the property `httpOptions` to set it up.


https://github.com/DataBooster/DbWebApi/tree/master/Client/Angular/projects/dbwebapi-client-lib#readme
