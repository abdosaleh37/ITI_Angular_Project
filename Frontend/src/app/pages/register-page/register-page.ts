import { Component } from '@angular/core';
import { Router, RouterLink } from "@angular/router";
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from "@angular/forms";
import { CommonModule } from '@angular/common';
import { AuthService } from '../../sevices/auth-service';

@Component({
  selector: 'app-register-page',
  imports: [RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './register-page.html',
  styleUrl: './register-page.css'
})
export class RegisterPage {
  constructor(private router: Router, private auth: AuthService) {}

  registerForm = new FormGroup({
  name: new FormControl('', [Validators.required]),
  email: new FormControl('', [Validators.required, Validators.email]),
  username: new FormControl('', [Validators.required, Validators.pattern(/^\S*$/)]),
  password: new FormControl('', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/)]),
  confirmPassword: new FormControl('', [Validators.required])
  }, 
  { validators: this.passwordMatchValidator });

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if(password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }

    return null;
  }

  errorMessage: string | null = null;
  loading = false;

  async onSubmit(){
    if(this.registerForm.valid) {
      this.loading = true;
      this.errorMessage = '';
      try {
        const value = this.registerForm.value as any;
        await this.auth.register(value.name, value.email, value.username, value.password, value.confirmPassword);
        this.router.navigate(['/login']);
      } catch (err: any) {
        this.errorMessage = 'Registration failed';
      } finally {
        this.loading = false;
      }
    }
  }
}
