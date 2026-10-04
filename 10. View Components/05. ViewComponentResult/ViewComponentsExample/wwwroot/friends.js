// Write your JavaScript code.
// کاربر که روی دکمه کلیک کرده است، این کار را انجام دهید
document.querySelector("#load-friends-button").addEventListener("click", async function () {
    //مسیر /friends-list را درخواست میکند
    var response = await fetch("friends-list", { method: "GET" });
    var responseBody = await response.text();
    // پاسخ را در صفحه فراخوانی کننده با دیو با آی دی listجایگزین میکند

    // مهم اینجاست که مقادیر را چایگزین میکند نه اینکه مسیر زیر را فراخوانی کند
    ///friends-list
    // زیرا اگر مستقیم این مسیر فراخوانی شود در صفحه خالی مقادیر چاپ میشود
    // پس چنانچه که دقت میشود صفحه برای بازخوانی اطلاعات رفرش نمیشود و این بسیار مهم است
    document.querySelector("#list").innerHTML = responseBody;
});
