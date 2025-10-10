import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadData{
  private http = inject(HttpClient);
  private readonly getAllUrl = 'https://localhost:7002/api/products';
  products: any[] = [];

  async getAllProducts(): Promise<any[]> {
    try {
      const obs = this.http.get<any[]>(this.getAllUrl);
      this.products = await firstValueFrom(obs);
      return this.products;
    } catch (err) {
      console.error('Failed to load products', err);

      return this.products ?? [];
    }
  }

  async getProductById(id: any): Promise<any> {
    try {
      const obs = this.http.get<any>(`${this.getAllUrl}/${id}`);
      const product = await firstValueFrom(obs);
      return product;
    } catch (err) {
      console.error(`Failed to load product with id ${id}`, err);
      return null;
    }
  }
}
