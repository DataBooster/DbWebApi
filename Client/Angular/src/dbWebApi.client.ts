/*!
* DbWebApi Client Angular Library
* https://github.com/databooster/dbwebapi
* Date: 2018-06-12
*/
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IStoredProcedureResponse, OracleInputParams } from './dbWebApi.model';

export class DbWebApiClient {
  public httpOptions = {
    params: new HttpParams().set('NamingCase', 'CamelCase'),
    headers: new HttpHeaders().set('Content-Type', 'application/json').set('Accept', 'application/json'),
    withCredentials: true
  };

  constructor(public httpClient: HttpClient, public baseUrl: string = '') { }

  public getSp(sp: string): Observable<IStoredProcedureResponse> {
    return this.httpClient.get<IStoredProcedureResponse>(this.baseUrl + sp, this.httpOptions);
  }

  public postSp(sp: string, body: any | null): Observable<IStoredProcedureResponse> {
    return this.httpClient.post<IStoredProcedureResponse>(this.baseUrl + sp, body, this.httpOptions);
  }

  public postOraSp(sp: string, body: any | null): Observable<IStoredProcedureResponse> {
    return this.httpClient.post<IStoredProcedureResponse>(this.baseUrl + sp, OracleInputParams.Flatten(body), this.httpOptions);
  }
}
