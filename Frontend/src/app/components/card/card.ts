import { Component, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GetStars } from '../../sevices/get-stars';

@Component({
  selector: 'app-card',
  imports: [CommonModule],
  templateUrl: './card.html',
  styleUrl: './card.css'
})
export class Card {
  @Input() product: any = null;
  private getStarsService = inject(GetStars);

  getStars(rating: number){
    return this.getStarsService.getStars(rating);
  }

}
