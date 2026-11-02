import { Injectable } from '@angular/core';
import { City } from "../models/city";
import { HttpClient } from "@angular/common/http";// این را وارد کردیم
import { Observable } from "rxjs"; // و این ها را وارد کردیم

@Injectable({
  providedIn: 'root'
})
export class CitiesService {
  cities: City[] = [];

  // 1 از اینجا مقادیر اولیه را حذف کردیم و پارمتر وروردی را HttpClient قرار دادیم
  constructor(private httpClient: HttpClient) {
  }

  // 2 ایم مقدار تابع را به این شکل تغییر میدهیم تا دیتا از Web API دریافت کند
  // Observable یعنی داده ای که Stream می باشد و جریان دارد
  public getCities(): Observable<City[]> {
    return this.httpClient.get<City[]>("http://localhost:5063/api/v1/cities")
  }
}
