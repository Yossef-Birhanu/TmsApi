
import { Component, inject } from '@angular/core';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-admin-course-list',
  imports: [],
  templateUrl: './admin-course-list.component.html',
  styleUrl: './admin-course-list.component.scss',
})
export class AdminCourseListComponent {

  // Inject AuthService and make it available to the template as "auth"
  auth = inject(AuthService);

  course: any;

  deleteCourse(arg0: any) {
    throw new Error('Method not implemented.');
  }
}

