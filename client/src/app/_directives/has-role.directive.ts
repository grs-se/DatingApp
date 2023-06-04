import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  // '*' (i.e. *ngIf) = structural directive, meaning it will remove elements from DOM
  // *appHasRole='["Admin", "Thing"]'
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[] = [];
  user: User = {} as User;

  constructor(private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) this.user = user
      }
    })
  }
    ngOnInit(): void {
      if (this.user.roles.some(r => this.appHasRole.includes(r))) {
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      } else {
        // this operates in the same way as *ngIf
        // if we clear() the viewContainerRef then we remove that element from the DOM
        // and in this case that means the admin link will be removed from the DOM
        this.viewContainerRef.clear()
      }
    }

}
