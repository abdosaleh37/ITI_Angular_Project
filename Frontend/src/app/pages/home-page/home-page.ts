import { Component, inject, OnInit } from '@angular/core';
import { Card } from "../../components/card/card";
import { RouterLink } from '@angular/router';
import { LoadData } from '../../sevices/load-data';

@Component({
  selector: 'app-home-page',
  imports: [Card, RouterLink],
  templateUrl: './home-page.html',
  styleUrl: './home-page.css'
})
export class HomePage implements OnInit {
  private loadDataService = inject(LoadData);
  products: any[] = [];

  async ngOnInit() {
    this.products = await this.loadDataService.getAllProducts();
  }
}
