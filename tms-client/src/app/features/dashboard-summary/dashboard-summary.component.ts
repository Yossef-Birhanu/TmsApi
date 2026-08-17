import { Component,inject } from '@angular/core';
import { EnrollmentStore } from '../../store/enrollment.store';

@Component({
  selector: 'app-dashboard-summary',
  standalone: true,
  imports: [],
  templateUrl: './dashboard-summary.component.html',
  styleUrl: './dashboard-summary.component.scss',
})
export class DashboardSummaryComponent {
  // Uses exactly the same singleton store
  // as EnrollmentListComponent.
  store = inject(EnrollmentStore);
}