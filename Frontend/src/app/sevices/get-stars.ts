import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GetStars {

  getStars(rating: number): { type: String }[] {
    const stars: {type: any} [] = [];
    const fullStars = Math.floor(rating);
    const hasQuartStar = (rating % 1) >= 0.1 && (rating % 1) < 0.4;
    const hasHalfStar = (rating % 1) >= 0.4 && (rating % 1) < 0.7;
    const hasThreeQuartStar = (rating % 1) >= 0.7 && (rating % 1) <= 0.9;

    for (let i = 0; i < fullStars; i++) {
      stars.push({ type: 'full'});
    }

    if (hasQuartStar && fullStars < 5) {
      stars.push({ type: 'quart'});
    }

    if (hasHalfStar && fullStars < 5) {
      stars.push({ type: 'half'});
    }

    if (hasThreeQuartStar && fullStars < 5) {
      stars.push({ type: 'threeQuart'});
    }

    while (stars.length < 5) {
      stars.push({ type: 'empty'});
    }

    return stars;
  }
}
