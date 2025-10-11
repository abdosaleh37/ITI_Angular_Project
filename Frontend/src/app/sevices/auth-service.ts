import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly loginUrl = 'https://localhost:7002/api/users/login';
  private readonly registerUrl = 'https://localhost:7002/api/users/register';

  // Reactive auth signals
  readonly token = signal<string | null>(localStorage.getItem('auth_token'));
  readonly expiration = signal<string | null>(localStorage.getItem('auth_exp'));
  readonly user = signal<any | null>(JSON.parse(localStorage.getItem('auth_user') || 'null'));

  async login(email: string, password: string): Promise<any> {
    try {
      const response = await this.http.post<any>(this.loginUrl, { email, password }).toPromise();
      const token = response?.token || null;
      const expiration = response?.expiration || null;
      const user = response?.user || null;

      if (token) {
        this.setToken(token);
      }

      if (expiration) {
        localStorage.setItem('auth_exp', expiration);
        this.expiration.set(expiration);
      }

      if (user) {
        localStorage.setItem('auth_user', JSON.stringify(user));
        this.user.set(user);
      }

      return response;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  async register(name: string, email: string, username: string, password: string, confirmPassword: string): Promise<any> {
    try {
      const response = await this.http.post<any>(this.registerUrl, { name, email, username, password, confirmPassword }).toPromise();
      return response;
    } catch (error) {
      console.error('Registration error:', error);
      throw error;
    }
  }

  // Save token in memory (signal) and localStorage
  setToken(token: string | null) {
    if (token) {
      localStorage.setItem('auth_token', token);
      this.token.set(token);
    } else {
      localStorage.removeItem('auth_token');
      this.token.set(null);
    }
  }

  // Convenience getter
  getToken(): string | null {
    return this.token();
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout() {
    this.setToken(null);
    localStorage.removeItem('auth_exp');
    localStorage.removeItem('auth_user');
    this.expiration.set(null);
    this.user.set(null);
  }
}