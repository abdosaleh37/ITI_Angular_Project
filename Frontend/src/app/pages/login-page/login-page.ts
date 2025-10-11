import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Form, FormsModule } from '@angular/forms';
import { Router, RouterLink } from "@angular/router";
import { AuthService } from '../../sevices/auth-service';

@Component({
  selector: 'app-login-page',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css'
})
export class LoginPage {

  constructor(private router: Router, private auth: AuthService) {}

  user = {
    email: '',
    password: ''
  }

  errorMessage: string | null = null;
  loading = false;

  async onSubmit(form: any){
    if(!form.invalid) {
      this.loading = true;
      this.errorMessage = '';
      try {
        await this.auth.login(this.user.email, this.user.password);
        this.router.navigate(['/home']);
      } catch (err: any) {
        this.errorMessage = 'Wrong Email or Password';
      } finally {
        this.loading = false;
      }
    }
  }
}
