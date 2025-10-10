import { TestBed } from '@angular/core/testing';

import { LoadData } from './load-data';

describe('LoadData', () => {
  let service: LoadData;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoadData);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
