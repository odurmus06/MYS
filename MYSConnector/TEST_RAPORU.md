# MYSClient Test Raporu

Tarih: 10 Eylül 2026  
Hedef: WPF / .NET Framework 4.5 / C# 5

## Sonuç

- Debug derlemesi: Başarılı
- Release derlemesi: Başarılı
- Son filtre revizyonu için otomatik arayüz ve işlev testi: 20 başarılı, 0 başarısız
- Tarih aralığı sınır testi: 12 başarılı, 0 başarısız
- Küçük ayrıntı/validasyon testi: 16 başarılı, 0 başarısız
- Menü ikonları testi: 3 başarılı, 0 başarısız
- Aktif ikon/seçili menü testi: 5 başarılı, 0 başarısız
- Logout filtre temizleme testi: 6 başarılı, 0 başarısız
- Servis durum göstergesi: Kod/derleme doğrulaması başarılı
- Servis timeout/yeniden deneme politikası: Retry/iptal smoke testi başarılı (3 denemede sonuç)
- Oturum zaman aşımı: Kod/derleme doğrulaması başarılı
- Klavye odak/Enter erişilebilirliği: Kod/derleme doğrulaması başarılı
- Servis politikası Settings kaydetme/doğrulaması: Kod/derleme doğrulaması başarılı
- Uygulama açılış testi: Başarılı
- Bottom status paneli derleme/açılış testi: Başarılı
- Tarih seçilmeden arama doğrulaması: ATO/ACO alan altında satır içi hata gösteriliyor
- Boş sonuç ekranı testi: ATO/ACO için sonuç kartı ve filtre özeti doğrulandı

## Doğrulanan düzeltmeler

- Yan menü ve giriş ekranında yalnızca `HvBS` yazısı görünüyor; yazının arkasında daire, nokta veya renkli Border bulunmuyor.
- ATO ve ACO tarih alanlarında sabit `15` yerine takvim simgesi görünüyor.
- ATO ve ACO takvim açılır pencereleri açılıyor.
- Logout butonunda hover durumunda yazı beyaz renge geçiyor.
- Pencere `1024 x 650` değerinin altına küçültülemiyor.
- ATO ve ACO parse işlemi başarıyla bittiğinde buton metni `Done` oluyor, yeşil vurgu/pulse animasyonu oynuyor ve 2 saniye sonra tekrar `Parse` durumuna dönüyor.
- Parse başarı alert'i kayıt konumuyla birlikte yeniden gösteriliyor; alert kapatıldıktan sonra vurgu animasyonu oynuyor.
- Windows başlık çubuğundaki uygulama başlığı kaldırıldı.
- Aktif ATO/ACO/Ayarlar menüsü vurgulanıyor.
- Aktif menünün ikonu da accent renkte gösteriliyor ve sol seçili göstergesi açılıyor.
- Login alanlarında Enter tuşu ile giriş yapılabiliyor.
- Alt durum panelinde servis durumu, aktif ekran, son işlem ve `MYSClient v1.0` gösteriliyor.
- Menü ikonları fonta bağlı emoji yerine WPF vektör ikonları olarak gösteriliyor.
- ACO aramasında yalnızca `Start Time` ve `End Time` filtreleri bulunuyor.
- ATO aramasında yalnızca `Min Publish Date`, `Start Time`, `End Time` ve `Mission Name` filtreleri bulunuyor.
- Start/End filtreleri tek bir DatePicker alanından tarih seçilerek kullanılıyor; ayrı saat bileşeni bulunmuyor.
- ATO ve ACO aramasında Start Time ile End Time seçilmeden işlem başlatılmıyor; alan altında satır içi hata gösteriliyor. ATO `Min Publish Date` ek filtre olarak isteğe bağlıdır.
- Arama ve parse yükleme animasyonları en az 3 saniye gösteriliyor. İşlem 3 saniyeden uzun sürerse animasyon gerçek işlem tamamlanana kadar açık kalıyor.
- Arama, parse ve dosya yazma işleri UI thread dışında çalışıyor; pencere işlem sırasında cevap vermeye devam ediyor.
- Yeni arama önceki aramayı iptal ediyor; ekran kapatıldığında devam eden işler iptal ediliyor.
- Başlangıç/bitiş tarih aralığı kısıtının çalışma zamanında uygulanması
- Geçersiz DTG ve bilinmeyen ay kodlarının anlaşılır `FormatException` ile reddedilmesi
- Çıktı klasörünün yazılabilirlik kontrolü
- Mevcut çıktı dosyası için overwrite onayı
- Aktif menü öğesi vurgusu
- Aktif menüde ikon rengi ve sol seçili göstergesinin birlikte değişmesi
- Logout sonrasında ATO/ACO filtrelerinin, sonuç listelerinin ve sayaçların temizlenmesi
- Login alanlarında Enter tuşu desteği
- Login düğmesi klavyeden varsayılan düğme olarak çalışıyor; filtre alanlarında klavye odağı accent kenarlıkla görünür.
- ATO, ACO ve Settings menü ikonlarının görsel ağaçta bulunması

## Geçen işlev testleri

- Boş kullanıcı adı/parola doğrulaması
- Geçerli giriş akışı
- Logout ve alanların temizlenmesi
- ATO, ACO ve Ayarlar ekranları arasında gezinme
- ATO filtresiz aramada 8 kaydın gelmesi
- ACO filtresiz aramada 6 kaydın gelmesi
- ATO/ACO mesaj alanlarının ayrıştırılması
- ATO filtrelerini temizleme
- ACO eski Message ID, Area Name, Area Type ve Control Authority filtrelerinin kaldırılması
- ATO eski Message ID, Task Unit, Mission Type, Callsign ve Status filtrelerinin kaldırılması
- ATO Mission Name filtresinin büyük/küçük harf duyarsız çalışması
- ATO Min Publish Date filtresinin seçilen günü dahil etmesi
- ATO ve ACO Start/End Time filtrelerinin ayrı ayrı ve birlikte çalışması
- Yeni ATO/ACO filtrelerinin Clear ile varsayılan değerlere dönmesi
- Başlangıç tarihinden küçük bitiş tarihinin seçilememesi
- Başlangıç tarihi değişince eski/geçersiz bitiş tarihinin temizlenmesi
- Aynı gün başlangıç ve bitiş değerinin kabul edilmesi
- Seçili bitiş gününün tamamının (23:59:59) filtreye dahil edilmesi
- Takvimlerin açılması
- `HvBS` yazısının iki konumda bulunması ve doğrudan bir Border içinde olmaması
- Eski `HWBSE` / `MYS CONNECTOR` markalamasının ve daire logonun bulunmaması
- Minimum pencere boyutunun çalışma anında uygulanması
- ATO ve ACO araması sürerken WPF dispatcher'ın cevap vermesi
- Yükleme katmanının işlem sürerken görünür, işlem bitince kapalı olması
- ATO ve ACO parse işlerinin `Task` olarak çalışıp çıktı dosyası üretmesi
- Arama/parse animasyon süresinin `max(3 saniye, gerçek işlem süresi)` kuralına uyması
- İki parse butonunun da tam `Done` metnini kullanması
- İki ekranda da iptal desteğinin bulunması
- Pencere başlığının çalışma zamanında boş olması
- Parse durumunun `Parsing... → Done → başarı alert'i → Parse` sırasını izlemesi
- ATO ve ACO butonlarında pulse animasyonunun çalışma zamanında başlaması
- Bottom panel servis durumu arama/parse sırasında `BUSY`, tamamlanınca `READY`, hata sonrasında `ERROR` gösteriyor.
- Arama sonucu `0` olduğunda tablo üzerinde `NO MESSAGES FOUND`, seçilen tarih aralığı ve aktif filtre özeti gösteriliyor; kayıt bulunduğunda kart gizleniyor.
- ATO ve ACO ekranlarında servis durumu `READY → BUSY → READY` olarak gösteriliyor; hata sonrası `ERROR` durumuna geçiyor.
- Generated web-service çağrısının bağlanacağı ortak politika varsayılan olarak 30 saniye timeout ve en fazla 2 yeniden deneme uyguluyor; iptal token'ı UI işlemiyle birlikte taşınıyor.
- Timeout ve maksimum retry değerleri Settings ekranından değiştirilebiliyor; timeout `5–300` saniye, retry `0–5` aralığında doğrulanıyor ve sonraki isteğe uygulanıyor.
- 30 dakika kullanıcı etkinliği olmazsa oturum kapatılıyor, filtreler temizleniyor ve login ekranı gösteriliyor.

## Tespit edilen eksikler

### Yüksek

1. Giriş gerçek kimlik doğrulaması yapmıyor. Kullanıcı adı ve parola boş olmadığı sürece uygulama açılıyor.
2. Web servis generated class'ları ve parse işlemi ayrı solution'larda tutulduğu için bu UI projesinin test girdileri örnek kayıtlarla çalıştırıldı; dış solution referansları ve servis sözleşmesi bu proje kapsamında değildir.

### Orta

3. Gerçek generated service client bağlandığında endpoint'e özgü timeout istisnaları ve hangi hataların yeniden deneneceği servis sözleşmesine göre daraltılmalı; ortak 30 saniye/2 deneme politikası hazır durumdadır.
4. Tek DatePicker kullanımı nedeniyle Start/End filtreleri gün bazında çalışıyor; saat/dakika seçimi istenirse ayrı bir DateTime kontrolü gerekir.
