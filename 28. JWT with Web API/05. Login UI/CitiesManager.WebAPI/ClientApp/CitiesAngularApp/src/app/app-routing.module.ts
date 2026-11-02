import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CitiesComponent } from './cities/cities.component';
import { LoginComponent } from './login/login.component'; // 6
import { RegisterComponent } from './register/register.component';

const routes: Routes = [
  { path: "cities", component: CitiesComponent },
  { path: "register", component: RegisterComponent },
  { path: "login", component: LoginComponent } // 6
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
