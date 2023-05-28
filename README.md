# EralpAPI

---USER---
API de kullanıcı öncellikle /Auth/Login kısmında login işlemini gerçekleştirmesi gerekmektedir.
Daha sonra sayfanın sağ üst köşesinde bulunan Authorize kısmına login işleminin sonucunda dönen token alanını girerek authorize olması gerekmektedir.
Eğer kullanıcı kayıtlı değilse /Auth/Register alanından yeni bir kullanıcı oluşturulur. Daha sonra login işlemleri tekrar yapılır.
/Auth/UpdateUser/{id} alanında kullanıcı kendi bilgilerini güncelleyebilir
/Auth/GetMyDetails alanında ise kullanıcı kendine ait detay bilgilerini görebilir

--PRODUCT--
/Product/AddProductForUser bu alanda kullanıcı bazlı ürün eklenmesi yapılır.
/Product/GetProduct/{id} tek bir ürüne ait bilgileri getirir
/Product/UpdateProduct/{id} idye göre ürün güncellemesi yapılır
/Product/GetProductsByUser kullanıcıya ait ürünleri getiren method
/Product/DeleteProduct/{id} girilen ürün id ye göre silme işlemi yapılır.
