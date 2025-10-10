import { TestBed } from '@angular/core/testing';

import { GetStars } from './get-stars';

describe('GetStars', () => {
  let service: GetStars;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GetStars);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
