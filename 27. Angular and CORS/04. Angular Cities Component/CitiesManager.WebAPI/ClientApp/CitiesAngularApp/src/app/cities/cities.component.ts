// ابتدا cities را اینجا تعریف می کنیم تا در cities.component.html از آن استفاده کنیم
import { Component } from '@angular/core';
import { City } from '../models/city'; // 1
import { CitiesService } from '../services/cities.service'; // 2

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.css']
})
export class CitiesComponent {  
  cities: City[] = [];// 3

  // 4
  constructor(private citiesService: CitiesService) {
  }
  // 5 هنگام بالا آمدن ng این ساخته میشود و کاپاننت innoke میشود
  ngOnInit() {
    this.cities = this.citiesService.getCities();
  }
}
