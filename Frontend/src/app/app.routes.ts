import { Routes , RouterLinkActive } from '@angular/router';
import { HomePage } from './pages/home-page/home-page';
import { NotFound } from './pages/not-found/not-found';
import { LoginPage } from './pages/login-page/login-page';
import { RegisterPage } from './pages/register-page/register-page';
import { CartPage } from './pages/cart-page/cart-page';
import { ProductDetails } from './pages/product-details/product-details';

export const routes: Routes = [
{path: '', component: HomePage, title:'home'},
{path: 'home', component: HomePage, title:'home'},
{path: 'login', component: LoginPage, title:'login'},
{path: 'register', component: RegisterPage, title:'register'},
{path: 'cart', component: CartPage, title:'cart'},
{path: 'details/:productId', component: ProductDetails, title:'product-details'},
{path: '**', component: NotFound, title:'not found'},
];
