import { CanActivateFn } from '@angular/router';
import { AccountService } from '../core/services/account-service';
import { inject } from '@angular/core';
import { ToastService } from '../core/services/toast-service';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService)
  const toast = inject(ToastService)

  if(accountService.currentUser()){
return true
  }else{
    toast.error('You shall not pass')
    return false
  }
};
