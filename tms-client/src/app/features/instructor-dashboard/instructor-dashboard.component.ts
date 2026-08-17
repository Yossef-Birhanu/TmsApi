
import { Component, inject, OnInit } from '@angular/core';
import { EnrollmentStore } from '../../store/enrollment.store';
import { AnalyticsChartComponent } from "../../ui/analytics-chart/analytics-chart.component";

@Component({
  selector: 'tms-instructor-dashboard',
  standalone: true,

  // The dashboard template contains the @defer block
  // that loads the analytics chart when needed.
  templateUrl: './instructor-dashboard.component.html',

  // External stylesheet for the dashboard.
  styleUrl: './instructor-dashboard.component.scss',
  imports: [AnalyticsChartComponent]
})
export class InstructorDashboardComponent implements OnInit {

  // Inject the EnrollmentStore to manage enrollment data.
  store = inject(EnrollmentStore);
  

  // Load enrollments when the dashboard is initialized.
  ngOnInit(): void {
    this.store.loadEnrollments();
    this.store.listenForLiveUpdates();
    
  }


}


