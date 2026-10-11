import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberDetailed } from './member-detailed';

describe('MemberDetailed', () => {
  let component: MemberDetailed;
  let fixture: ComponentFixture<MemberDetailed>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberDetailed],
    }).compileComponents();

    fixture = TestBed.createComponent(MemberDetailed);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
