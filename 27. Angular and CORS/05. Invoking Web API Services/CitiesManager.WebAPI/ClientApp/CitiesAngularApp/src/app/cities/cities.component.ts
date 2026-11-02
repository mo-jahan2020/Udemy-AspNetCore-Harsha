import { Component } from '@angular/core';
import { City } from '../models/city';
import { CitiesService } from '../services/cities.service';

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.css']
})
export class CitiesComponent {
  cities: City[] = [];

  constructor(private citiesService: CitiesService) {
  }
  // 3 در حال اجرای برنامه این اجرا خواهد شد
  // subscribe سه آیتم میتوند داشته باشد
  //next : اولی جریان دیتا
  // error دومی خطایی اگر رخ داد
  // complete سوم پس از کامل شدن کار چکار کند
  ngOnInit() {
    this.citiesService.getCities()
      .subscribe({
        // کارای که باید انجام شود
        next: (response: City[]) => {
          this.cities = response;
        },
        // خطایی اگر رخ داد
        error: (error: any) => {
          console.log(error)
        },
        // بعد از پایان چه بکند
        complete: () => { }
      });

  }
}
