import { TestBed, inject } from '@angular/core/testing';
import { DbWebApiClient } from './dbwebapi-client.service';

describe('DbWebApiClient', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DbWebApiClient]
    });
  });

  it('should be created', inject([DbWebApiClient], (service: DbWebApiClient) => {
    expect(service).toBeTruthy();
  }));
});
