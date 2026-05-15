import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PositionSimulator } from './position-simulator';

describe('PositionSimulator', () => {
  let component: PositionSimulator;
  let fixture: ComponentFixture<PositionSimulator>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PositionSimulator]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PositionSimulator);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
