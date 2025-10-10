import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Form, FormsModule } from '@angular/forms';
import { Router, RouterLink } from "@angular/router";

@Component({
  selector: 'app-login-page',
  imports: [RouterLink, FormsModule, CommonModule],
templateUrl: './login-page.html',
  styleUrl: './login-page.css'
})
export class LoginPage {

  constructor(private router: Router) {}

  user = {
    email: '',
    password: ''
  }

  onSubmit(form: any){
    if(!form.invalid) {
      this.router.navigate(['/home']);
    }
  }
}
