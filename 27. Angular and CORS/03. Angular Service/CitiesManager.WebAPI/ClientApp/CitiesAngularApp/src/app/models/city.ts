// حال این فایل ایجاد دشه را به این شکل کامل می کنیم
// به حروف اول  کوچک نا فیلد ها دقت شود
export class City {
  cityID: string | null;// نام فیلد های موجودیت city و چون تایپ اسکریپت GUID ندارد آن را string گذاشتیم
  cityName: string | null;

  // سازنده
  constructor(cityID: string | null = null, cityName: string | null = null) {
    this.cityID = cityID;
    this.cityName = cityName;
  }
}
