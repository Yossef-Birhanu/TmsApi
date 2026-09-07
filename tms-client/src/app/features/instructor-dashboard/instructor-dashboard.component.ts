import {
  Component,
  inject,
  OnInit
} from '@angular/core';

import {
  EnrollmentStore
} from '../../store/enrollment.store';

import {
  AnalyticsChartComponent
} from '../../ui/analytics-chart/analytics-chart.component';

@Component({
  selector: 'tms-instructor-dashboard',
  standalone: true,

  templateUrl: './instructor-dashboard.component.html',

  styleUrl: './instructor-dashboard.component.scss',

  imports: [
    AnalyticsChartComponent
  ]
})
export class InstructorDashboardComponent implements OnInit {

  // ========================================================
  // ENROLLMENT STORE
  // ========================================================

  store = inject(EnrollmentStore);


  // ========================================================
  // COMPONENT INITIALIZATION
  // ========================================================

  ngOnInit(): void {

    // Load existing enrollments from API
    this.store.loadEnrollments();

    // Start SignalR live synchronization
    this.store.listenForLiveUpdates();
  }
}