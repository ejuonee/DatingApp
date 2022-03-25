import { REFERENCE_PREFIX } from '@angular/compiler/src/render3/view/util';
import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanDeactivate,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { Observable } from 'rxjs';
import { ConfirmService } from '../_services/confirm.service';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

@Injectable({
  providedIn: 'root',
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {
  constructor(private confirmService: ConfirmService) {}

  // canDeactivate(component: MemberEditComponent): boolean {
  //   if (component.editForm.dirty) {
  //     return confirm(
  //       'Are you sure you want to continue? Any unsaved changes will be lost'
  //     );
  //   }
  //   return true;
  // }

  canDeactivate(component: MemberEditComponent): Observable<boolean> | boolean {
    if (component.editForm.dirty) {
      return this.confirmService.confirm();
    }
    return true;
  }
}
