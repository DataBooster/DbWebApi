/*!
* DbWebApi Client Angular Library
* https://github.com/databooster/dbwebapi
* Date: 2018-06-12
*/
import { HttpErrorResponse } from "@angular/common/http";

export interface IStoredProcedureResponse {
  ResultSets: object[][];
  OutputParameters: object;
  ReturnValue: number | string | boolean | null;
}

export interface IDbError {
  Message: string;
  ExceptionMessage: string;
  ExceptionType: string;
  StackTrace: string;
}

export class DbError implements IDbError {
  Message: string;
  ExceptionMessage: string;
  ExceptionType: string;
  StackTrace: string;

  constructor(public httpError: HttpErrorResponse) {
    const err = httpError.error;

    for (let e in err) {
      if (!this.hasOwnProperty(e)) {
        this[e] = err[e];
      }
    }
  }
}

export abstract class OracleInputParams {
  // Flatten object-array to array-properties (Properties of PL/SQL Associative Array)
  public static Flatten(source: object): object {
    if (!source)
      return source;

    let inputOracle = {};

    if (Array.isArray(source))
      return OracleInputParams.DivideArrayToProperties(inputOracle, source);

    for (let p in source) {
      let v = source[p];
      if (Array.isArray(v) && v.length > 0 && v[0] instanceof Object)
        OracleInputParams.DivideArrayToProperties(inputOracle, v);
      else
        inputOracle[p] = v;
    }

    return inputOracle;
  }

  private static DivideArrayToProperties(container: object, sourceArray: object[]): object {
    if (!sourceArray)
      return sourceArray;
    if (!container)
      container = {};

    let len = sourceArray.length;

    if (len > 0) {
      for (let p in sourceArray[0]) {
        if (container[p] === undefined) {
          container[p] = sourceArray.map(o => o[p]);
        }
      }
    }

    return container;
  }
}
