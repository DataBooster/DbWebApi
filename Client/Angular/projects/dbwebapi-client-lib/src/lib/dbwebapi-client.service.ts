/*!
* DbWebApi Client Angular Library
* https://github.com/databooster/dbwebapi
* Date: 2018-06-12
*/
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IStoredProcedureResponse, OracleInputParams } from './dbwebapi-client.model';

export class DbWebApiClient {
  public httpOptions = {
    params: new HttpParams().set('NamingCase', 'CamelCase'),
    headers: new HttpHeaders().set('Content-Type', 'application/json').set('Accept', 'application/json'),
    withCredentials: true
  };

  constructor(public httpClient: HttpClient, public baseUrl: string = '') { }

  /**
   * Construct a GET request which interprets the body as JSON and returns it.
   *
   * @return an `Observable` of the body as type `IStoredProcedureResponse`.
   */
  public get(storedProcedure: string): Observable<IStoredProcedureResponse> {
    return this.httpClient.get<IStoredProcedureResponse>(this.baseUrl + storedProcedure, this.httpOptions);
  }

  /**
   * Construct a POST request which interprets the body as JSON and returns it.
   *
   * @return an `Observable` of the body as type `IStoredProcedureResponse`.
   */
  public post(storedProcedure: string, body: any | null, flattenAssociativeArrayParameters: boolean = false): Observable<IStoredProcedureResponse> {
    return this.httpClient.post<IStoredProcedureResponse>(this.baseUrl + storedProcedure,
      flattenAssociativeArrayParameters ? OracleInputParams.Flatten(body) : body,
      this.httpOptions);
  }
}
