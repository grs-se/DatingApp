import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { map, Observable } from 'rxjs';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef?: BsModalRef<ConfirmDialogComponent>;
  constructor(private modalService: BsModalService) { }

  confirm(
    // Default values can be overwritten
    title = 'Confirmation',
    message = "Are you sure you want to do this?",
    btnOkText = "Ok",
    btnCancelText = "Cancel"
  ): Observable<boolean> {
    const config = {
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }
    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);
    return this.bsModalRef.onHidden!.pipe(
      map(() => {
        // we know that inside bsModalRef.content.result, we initialised the value
        // so we should not have problems here.
        return this.bsModalRef!.content!.result
      }))
  }
}
