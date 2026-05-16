import { Routes } from '@angular/router';

import { Home } from './pages/home/home';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Profile } from './pages/profile/profile';
import { AdminUsers } from './pages/admin-users/admin-users';
import { PositionSimulator } from './pages/position-simulator/position-simulator';
import { Tours } from './pages/tours/tours';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'profile', component: Profile },
  { path: 'tours', component: Tours },
  { path: 'admin/users', component: AdminUsers },
  { path: 'position-simulator', component: PositionSimulator },
  { path: '**', redirectTo: '' }
];
