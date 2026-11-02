import { HttpClientModule } from '@angular/common/http'; // 3
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CitiesComponent } from './cities/cities.component'; // 4

@NgModule({
  declarations: [
    AppComponent,
    CitiesComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule // 5 مهم
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
// با وجود این کار ها اگر برنامه مقداری باز نگرداند در مرورگر آنگولار :
// F12-> Network-> Refresh -> click on cities-> header: strict-origin-when-cross-origin را دیدد
// این برای این است که شما میخواهید در دات نت کر از آدرس دیگری غیر از آدرس و پورت برنامه سرویس بگیرید که دات نت جلو آن را میگیرد
// این مشکل را در برتانع درس بعد رفع میکنیم
