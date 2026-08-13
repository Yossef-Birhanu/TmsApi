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

  // Dashboard template
  templateUrl: './instructor-dashboard.component.html',

  // Dashboard stylesheet
  styleUrl: './instructor-dashboard.component.scss',

  imports: [
    AnalyticsChartComponent
  ]
})
export class InstructorDashboardComponent
  implements OnInit {


  // ========================================================
  // ENROLLMENT STORE
  // ========================================================

  // Inject EnrollmentStore.
  store = inject(EnrollmentStore);


  // ========================================================
  // COMPONENT INITIALIZATION
  // ========================================================

  ngOnInit(): void {

    // Load the existing enrollments from the API.
    this.store.loadEnrollments();


    // Start SignalR live synchronization.
    //
    // This listens for:
    //
    // ReceiveEnrollmentStatusUpdated
    //
    // from the .NET backend.
    this.store.listenForLiveUpdates();

  }

}

