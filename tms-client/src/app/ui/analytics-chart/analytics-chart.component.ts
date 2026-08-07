
import { Component, computed, input } from '@angular/core';
import { Enrollment, EnrollmentStatus } from '../../models/enrollment.model';

@Component({
  selector: 'tms-analytics-chart',
  standalone: true,

  // Inline HTML template for the analytics chart.
  template: `
    <div class="chart-container">
      <h3>Enrollment Analytics</h3>

      <div class="chart-bars">

        <!-- Approved enrollments -->
        <div
          class="bar approved"
          [style.height.px]="approvedHeight()"
        >
          <span>Approved</span>
        </div>

        <!-- Pending enrollments -->
        <div
          class="bar pending"
          [style.height.px]="pendingHeight()"
        >
          <span>Pending</span>
        </div>

        <!-- Rejected enrollments -->
        <div
          class="bar rejected"
          [style.height.px]="rejectedHeight()"
        >
          <span>Rejected</span>
        </div>

      </div>

      <!-- Display the total number of enrollment records -->
      <p class="chart-summary">
        Total records: {{ data().length }}
      </p>
    </div>
  `,

  // External stylesheet for this component.
  // Make sure the filename matches the actual SCSS file.
  styleUrl: './analytics-chart.component.scss'
})
export class AnalyticsChartComponent {

  // Receives enrollment data from the parent component.
  data = input.required<Enrollment[]>();

  // Calculates the height of the Approved bar.
  // computed() recalculates only when data() changes.
  approvedHeight = computed(() => {
    const count = this.data()
      .filter(e => e.status === EnrollmentStatus.Approved)
      .length;

    return Math.max(20, count * 3);
  });

  // Calculates the height of the Pending bar.
  pendingHeight = computed(() => {
    const count = this.data()
      .filter(e => e.status === EnrollmentStatus.Pending)
      .length;

    return Math.max(20, count * 3);
  });

  // Calculates the height of the Rejected bar.
  rejectedHeight = computed(() => {
    const count = this.data()
      .filter(e => e.status ===EnrollmentStatus.Rejected)
      .length;

    return Math.max(20, count * 3);
  });
}

