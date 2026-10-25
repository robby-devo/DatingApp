import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { memberResolverResolver } from './member-resolver-resolver';

describe('memberResolverResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) =>
    TestBed.runInInjectionContext(() => memberResolverResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
