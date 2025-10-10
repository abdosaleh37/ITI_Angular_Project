import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { LoadData } from '../../sevices/load-data';
import { GetStars } from '../../sevices/get-stars';

@Component({
  selector: 'app-product-details',
  imports: [DatePipe],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css'
})
export class ProductDetails implements OnInit {
  private loadDataService = inject(LoadData);
  private getStarsService = inject(GetStars);
  product: any;
  image: any;
  tags = '';

  constructor(private route: ActivatedRoute, private router: Router) {}

  async ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('productId');
    if (idParam === null) {
      this.router.navigate(['/not-found']);
      return;
    }

    this.product = await this.loadDataService.getProductById(idParam);

    if(!this.product) {
      this.router.navigate(['/not-found']);
      return;
    }

    this.image = this.product?.images?.[0].url || this.product?.thumbnail;

    if (Array.isArray(this.product.tags) && this.product.tags.length) {
      this.tags = this.product.tags.join(', ');
    } else if (Array.isArray(this.product.productTags) && this.product.productTags.length) {
      this.tags = this.product.productTags
        .map((pt: any) => pt?.tag?.name ?? pt?.tagName ?? pt?.name ?? null)
        .filter(Boolean)
        .join(', ');
    } else {
      this.tags = '';
    }
  }

  getStars(rating: number): {type: String}[] {
    return this.getStarsService.getStars(rating);
  }

  changeImage(index: number){
    this.image = this.product?.images?.[index].url || this.product?.thumbnail;
  }
}
