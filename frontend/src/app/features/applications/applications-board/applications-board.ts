import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { Component, computed, inject } from '@angular/core';
import {
  APPLICATION_STATUSES,
  ApplicationStatus,
  JobApplication,
} from '../../../models/job-application';
import { ApplicationsStore } from '../../../services/applications-store';
import { humanize } from '../../../shared/humanize';

@Component({
  selector: 'app-applications-board',
  imports: [CdkDropListGroup, CdkDropList, CdkDrag],
  templateUrl: './applications-board.html',
  styleUrl: './applications-board.css',
})
export class ApplicationsBoard {
  private readonly store = inject(ApplicationsStore);
  protected readonly humanize = humanize;

  // One column per pipeline stage, in order, each holding its applications.
  protected readonly columns = computed(() =>
    APPLICATION_STATUSES.map((status) => ({
      status,
      apps: this.store.applications().filter((a) => a.status === status),
    })),
  );

  protected drop(event: CdkDragDrop<JobApplication[]>, status: ApplicationStatus): void {
    // Same column = a reorder, which carries no meaning here; only cross-column moves change stage.
    if (event.previousContainer === event.container) {
      return;
    }
    this.store.changeStatus(event.item.data as JobApplication, status);
  }

  // A genuine click (CDK suppresses the click that ends a real drag) opens the detail drawer.
  protected openEdit(app: JobApplication): void {
    this.store.openEdit(app);
  }
}
