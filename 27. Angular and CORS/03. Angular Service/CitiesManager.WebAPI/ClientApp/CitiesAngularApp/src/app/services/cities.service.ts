// فایل ساخته شده را به این شکل ویرایش میکنیم
import { Injectable } from '@angular/core';
import { City } from "../models/city"; // مدا سیتی را وارد برنامه کنید

@Injectable({
  providedIn: 'root'
})
export class CitiesService {
  cities: City[] = [];// متغییر cities از نوع آرایه ای از City است

  // و سازنده را میسازیم
  constructor() {
    this.cities = [
      new City("101", "New York"),
      new City("102", "New Delhi"),
      new City("103", "Sydney"),
      new City("104", "Berlin"),
    ];
  }

  // و متد مورد نظر برای باز گرداندن آن
  public getCities(): City[] {
    return this.cities;
  }
}
