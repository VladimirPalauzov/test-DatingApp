import { Injectable } from '@angular/core';
declare let alertify: any; // no need to import it because it is already imported via angular.json

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

  constructor() { }

  confirm(messsage: string, okCallback: () => any) { // okCallback is a function of type any
    alertify.confirm(messsage, function(e) { // handling click event. The 'e' represents user clicking OK button
      if (e) {
        okCallback();
      } else {} // do nothing if user clicks 'cancel'
    });
  }

  success(messsage: string) {
    alertify.success(messsage);
  }

  error(messsage: string) {
    alertify.error(messsage);
  }

  warning(messsage: string) {
    alertify.warning(messsage);
  }

  message(message: string) {
    alertify.message(message);
  }

}
