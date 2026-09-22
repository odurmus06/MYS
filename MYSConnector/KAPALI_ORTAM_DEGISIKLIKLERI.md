# MYSClient Kapalı Ortam Değişiklik Talimatları

Bu dosya, internete veya kaynak kod deposuna bağlı olmayan kapalı ortamdaki MYSClient projesine değişikliklerin elle aktarılması için hazırlanmıştır.

Her değişiklik kaydında aşağıdaki bilgiler bulunur:

- Değişikliğin amacı
- Etkilenen dosyalar
- Bulunacak mevcut kod
- Eklenecek veya değiştirilecek kod
- Kontrol ve test adımları
- Geri alma yöntemi

---

## Değişiklik 001 — Login ekranında arka plan blur efekti

**Tarih:** 22 Eylül 2026  
**Hedef proje:** `MYSClient`  
**Hedef framework:** `.NET Framework 4.5 / WPF`  
**Etkilenen dosya sayısı:** 2

### Amaç

Login ekranı açıkken uygulamanın arka planını yoğun biçimde karartmak yerine flu göstermek. Login kartı net kalmalıdır. Kullanıcı giriş yaptığında blur kaldırılmalı; logout veya otomatik oturum zaman aşımı gerçekleştiğinde blur yeniden uygulanmalıdır.

### Değiştirilecek dosyalar

1. `MainWindow.xaml`
2. `MainWindow.xaml.cs`

## 1. MainWindow.xaml değişiklikleri

### 1.1. Uygulama içeriğine başlangıç blur efekti ekleyin

`MainWindow.xaml` dosyasını açın.

Aşağıdaki satırı bulun:

```xml
<Grid x:Name="AppContent" IsEnabled="False">
```

Bu satırın hemen altına aşağıdaki bloğu ekleyin:

```xml
<Grid.Effect>
    <BlurEffect Radius="10" KernelType="Gaussian"/>
</Grid.Effect>
```

Değişiklikten sonra bölüm tam olarak şöyle görünmelidir:

```xml
<!-- LAYER 1: Main App Content -->
<Grid x:Name="AppContent" IsEnabled="False">
    <Grid.Effect>
        <BlurEffect Radius="10" KernelType="Gaussian"/>
    </Grid.Effect>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="180"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
```

`Radius="10"` blur seviyesidir. Daha hafif bir görünüm için `6–8`, daha yoğun bir görünüm için `12–15` kullanılabilir.

### 1.2. Login ekranındaki siyah örtüyü hafifletin

Aynı dosyada aşağıdaki bölümü bulun:

```xml
<Grid x:Name="LoginOverlay" Visibility="Visible">
    <Rectangle Fill="#CC000000"/>
```

Yalnızca Rectangle satırını aşağıdaki şekilde değiştirin:

```xml
<Rectangle Fill="#55000000"/>
```

Değişiklikten sonra bölüm şöyle görünmelidir:

```xml
<!-- LAYER 2: Login Overlay -->
<Grid x:Name="LoginOverlay" Visibility="Visible">
    <Rectangle Fill="#55000000"/>
```

`#55` alfa değeridir. Bu değer siyah katmanı yarı saydam tutar ve arkadaki blur efektinin görünmesini sağlar.

## 2. MainWindow.xaml.cs değişiklikleri

### 2.1. BlurEffect namespace'ini ekleyin

`MainWindow.xaml.cs` dosyasının başındaki `using` satırlarında aşağıdaki satırı bulun:

```csharp
using System.Windows.Input;
```

Hemen altına şunu ekleyin:

```csharp
using System.Windows.Media.Effects;
```

İlgili bölüm şöyle görünmelidir:

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Threading;
```

### 2.2. Başarılı login sonrasında blur efektini kaldırın

`LoginButton_Click` metodunda aşağıdaki satırları bulun:

```csharp
LoginOverlay.Visibility = Visibility.Collapsed;
AppContent.IsEnabled = true;
```

Bu iki satırın hemen altına aşağıdaki satırı ekleyin:

```csharp
AppContent.Effect = null;
```

Değişiklikten sonra bölüm şöyle görünmelidir:

```csharp
LoginOverlay.Visibility = Visibility.Collapsed;
AppContent.IsEnabled = true;
AppContent.Effect = null;
_lastActivity = DateTime.Now;
_sessionTimer.Start();
```

### 2.3. Logout sonrasında blur efektini tekrar etkinleştirin

`Logout_Click` metodunda aşağıdaki satırları bulun:

```csharp
LoginOverlay.Visibility = Visibility.Visible;
AppContent.IsEnabled = false;
```

Bu iki satırın hemen altına aşağıdaki satırı ekleyin:

```csharp
AppContent.Effect = new BlurEffect
{
    Radius = 10,
    KernelType = KernelType.Gaussian
};
```

Tek satırlık kullanım da geçerlidir:

```csharp
AppContent.Effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian };
```

Değişiklikten sonra ilgili bölüm şöyle görünmelidir:

```csharp
LoginError.Visibility = Visibility.Collapsed;
LoginOverlay.Visibility = Visibility.Visible;
AppContent.IsEnabled = false;
AppContent.Effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian };
UpdateBottomStatus("READY", "SIGNED OUT", "Signed out");
LoginUsername.Focus();
```

Otomatik oturum zaman aşımı `Logout_Click` metodunu çağırdığı için ayrıca `SessionTimer_Tick` içinde değişiklik yapılmasına gerek yoktur.

## 3. Derleme kontrolü

Visual Studio'da:

1. `Build > Clean Solution` seçin.
2. `Build > Rebuild Solution` seçin.
3. Önce `Debug`, ardından `Release` konfigürasyonunda derleyin.
4. Error List içinde hata bulunmadığını doğrulayın.

Beklenen çıktı adı:

```text
MYSClient.exe
```

## 4. Manuel test adımları

1. Uygulamayı açın.
2. Login kartının net kaldığını doğrulayın.
3. Arkadaki ATO ekranının flu göründüğünü doğrulayın.
4. Arka planın tamamen siyaha yakın görünmediğini doğrulayın.
5. Kullanıcı adı ve parola girerek login olun.
6. Login ekranı kapandığında ana ekranın yeniden netleştiğini doğrulayın.
7. `Logout` düğmesine basın.
8. Login ekranı açıldığında arka plan blur efektinin yeniden geldiğini doğrulayın.
9. Oturum zaman aşımı özelliği kullanılıyorsa zaman aşımı sonrasında da blur efektinin geldiğini doğrulayın.

## 5. Beklenen sonuç

- Login açık: `AppContent` flu ve devre dışı.
- Login başarılı: `AppContent` net ve kullanılabilir.
- Logout/zaman aşımı: `AppContent` tekrar flu ve devre dışı.
- Login kartı her durumda net.

## 6. Geri alma

Bu değişikliği geri almak için:

1. `MainWindow.xaml` içindeki `<Grid.Effect>...</Grid.Effect>` bloğunu silin.
2. Login Rectangle rengini `#55000000` değerinden `#CC000000` değerine döndürün.
3. `MainWindow.xaml.cs` içindeki `using System.Windows.Media.Effects;` satırını silin.
4. `AppContent.Effect = null;` satırını silin.
5. Logout metodundaki `AppContent.Effect = new BlurEffect ...` satırını silin.
6. Solution'ı yeniden derleyin.


## Sonraki değişiklikler için kayıt formatı

Yeni değişiklikler bu dosyaya artan değişiklik numarasıyla eklenecektir. Her kayıtta eski/yeni kod blokları ve manuel test adımları bulunacaktır.

---

## Değişiklik 002 — ATO tablosunu responsive hale getirme

**Tarih:** 22 Eylül 2026  
**Hedef proje:** `MYSClient`  
**Hedef framework:** `.NET Framework 4.5 / WPF`  
**Etkilenen dosya sayısı:** 1  
**Etkilenen dosya:** `Views/AtoListView.xaml`

### Amaç

API'den dönen ATO kayıtlarını aşağıdaki kolonlarla göstermek:

1. `Parse`
2. `Message ID`
3. `Mission Name`
4. `Status`

Tablo genişletildiğinde `Mission Name` kolonu kalan boş alanı doldurmalıdır. `Parse` solda, `Status` en sağda kalmalıdır. İçerik bazlı kolonlar verinin uzunluğuna göre büyümeli ve kullanıcı kolonları fareyle yeniden boyutlandırabilmelidir.

### 1. DataGrid responsive ayarlarını ekleyin

`Views/AtoListView.xaml` dosyasını açın ve `x:Name="AtoDataGrid"` olan DataGrid'i bulun.

DataGrid başlangıcındaki şu bölümü bulun:

```xml
x:Name="AtoDataGrid"
SelectionChanged="AtoDataGrid_SelectionChanged"
HeadersVisibility="Column" SelectionMode="Single"
VerticalScrollBarVisibility="Auto"
HorizontalScrollBarVisibility="Auto">
```

Bu bölümü aşağıdakiyle değiştirin:

```xml
x:Name="AtoDataGrid"
SelectionChanged="AtoDataGrid_SelectionChanged"
HeadersVisibility="Column" SelectionMode="Single"
ColumnWidth="Auto" MinColumnWidth="70" CanUserResizeColumns="True"
FrozenColumnCount="1"
VerticalScrollBarVisibility="Auto"
HorizontalScrollBarVisibility="Auto">
```

Bu özelliklerin görevleri:

- `ColumnWidth="Auto"`: Açıkça genişlik verilmeyen yeni kolonları başlık ve hücre içeriğine göre boyutlandırır.
- `MinColumnWidth="70"`: Kolonların okunamayacak kadar daralmasını önler.
- `CanUserResizeColumns="True"`: Kullanıcının kolon genişliklerini fareyle değiştirmesine izin verir.
- `FrozenColumnCount="1"`: İlk kolon olan Parse kolonunu yatay kaydırmada solda tutar.

### 2. ATO kolonlarını değiştirin

Aynı dosyada `<DataGrid.Columns>` bloğunu bulun.

Mevcut kolon bloğunda Parse kolonunun başlangıcı şu şekildedir:

```xml
<DataGridTemplateColumn Header="" Width="75">
```

Bunu aşağıdakiyle değiştirin:

```xml
<DataGridTemplateColumn Header="PARSE" Width="Auto" MinWidth="82">
```

Parse kolonunun içindeki `<DataGridTemplateColumn.CellTemplate>...</DataGridTemplateColumn.CellTemplate>` kodunu değiştirmeyin. Sadece kolonun başlangıç satırı değişecektir.

Parse kolonundan sonra gelen eski kolonları bulun:

```xml
<DataGridTextColumn Header="MSG ID" Binding="{Binding MsgId}" Width="110"/>
<DataGridTextColumn Header="DTG" Binding="{Binding Dtg}" Width="130"/>
<DataGridTextColumn Header="TASK UNIT" Binding="{Binding TaskUnit}" Width="*"/>
<DataGridTextColumn Header="MISSION NAME" Binding="{Binding MissionName}" Width="135"/>
<DataGridTextColumn Header="CALL" Binding="{Binding Callsign}" Width="80"/>
<DataGridTextColumn Header="STATUS" Binding="{Binding Status}" Width="65"/>
```

Bu altı satırın tamamını silin ve yerine aşağıdaki dört kolonu ekleyin:

```xml
<DataGridTextColumn Header="MESSAGE ID"
                    Binding="{Binding MsgId}"
                    Width="Auto"
                    MinWidth="130"/>
<DataGridTextColumn Header="MISSION NAME"
                    Binding="{Binding MissionName}"
                    Width="*"
                    MinWidth="180"/>
<DataGridTextColumn Header="STATUS"
                    Binding="{Binding Status}"
                    Width="Auto"
                    MinWidth="100"/>
```

Tek satırlık yazım tercih edilirse aynı kod şu şekilde kullanılabilir:

```xml
<DataGridTextColumn Header="MESSAGE ID" Binding="{Binding MsgId}" Width="Auto" MinWidth="130"/>
<DataGridTextColumn Header="MISSION NAME" Binding="{Binding MissionName}" Width="*" MinWidth="180"/>
<DataGridTextColumn Header="STATUS" Binding="{Binding Status}" Width="Auto" MinWidth="100"/>
```

Kolon bloğunun son sırası şu olmalıdır:

```text
PARSE | MESSAGE ID | MISSION NAME | STATUS
```

### 3. API model alanlarını kontrol edin

API sonucundaki model property adları XAML binding adlarıyla aynı olmalıdır:

| Ekrandaki kolon | Binding property |
|---|---|
| Message ID | `MsgId` |
| Mission Name | `MissionName` |
| Status | `Status` |

API tarafından üretilen class farklı property adları kullanıyorsa yalnızca binding bölümü değiştirilmelidir. Örnek:

```xml
Binding="{Binding MessageId}"
```

### 4. Sonradan yeni bir field/kolon ekleme

Yeni API alanlarını `MESSAGE ID` ile `MISSION NAME` kolonları arasına ekleyin. İçeriğe göre otomatik genişlemesi için `Width="Auto"` kullanın.

Örnek:

```xml
<DataGridTextColumn Header="TASK UNIT"
                    Binding="{Binding TaskUnit}"
                    Width="Auto"
                    MinWidth="100"/>
```

Uzun metin taşıyan yalnızca bir kolon `Width="*"` kullanmalıdır. Bu projede esnek kolon `MISSION NAME` kolonudur. Böylece:

- Tablo büyüdüğünde Mission Name genişler.
- Status en sağ kenarda kalır.
- Yeni Auto kolonlar içerik kadar yer kaplar.
- Gerekirse yatay kaydırma otomatik açılır.

### 5. Tam olması gereken kolon bloğu

Parse butonunun mevcut CellTemplate kodu korunarak `<DataGrid.Columns>` bölümü aşağıdaki yapıda olmalıdır:

```xml
<DataGrid.Columns>
    <DataGridTemplateColumn Header="PARSE" Width="Auto" MinWidth="82">
        <DataGridTemplateColumn.CellTemplate>
            <!-- Mevcut Parse butonu template kodu burada aynen kalacak. -->
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>

    <DataGridTextColumn Header="MESSAGE ID" Binding="{Binding MsgId}" Width="Auto" MinWidth="130"/>
    <DataGridTextColumn Header="MISSION NAME" Binding="{Binding MissionName}" Width="*" MinWidth="180"/>
    <DataGridTextColumn Header="STATUS" Binding="{Binding Status}" Width="Auto" MinWidth="100"/>
</DataGrid.Columns>
```

`<!-- Mevcut Parse butonu template kodu burada aynen kalacak. -->` açıklamasını gerçek XAML dosyasına eklemeyin; mevcut buton kodunu koruyun.

### 6. Derleme kontrolü

1. Visual Studio'da solution'ı açın.
2. `Build > Clean Solution` çalıştırın.
3. `Build > Rebuild Solution` çalıştırın.
4. Debug ve Release konfigürasyonlarını ayrı ayrı derleyin.
5. XAML parse veya binding derleme hatası olmadığını doğrulayın.

### 7. Manuel test adımları

1. Uygulamayı açıp login olun.
2. ATO ekranına geçin.
3. Start Time ve End Time seçerek arama yapın.
4. Kolon sırasının `PARSE | MESSAGE ID | MISSION NAME | STATUS` olduğunu doğrulayın.
5. Pencereyi yatay olarak büyütün.
6. Mission Name kolonunun boş alanı dolduracak şekilde genişlediğini doğrulayın.
7. Status kolonunun sağ kenarda kaldığını doğrulayın.
8. Kolon ayırıcılarını fareyle sürükleyerek genişliklerin değiştirilebildiğini doğrulayın.
9. Uzun Message ID veya Status değeriyle kolonun içeriği gösterecek kadar genişlediğini doğrulayın.
10. Pencere daraltıldığında minimum pencere genişliği ve gerekiyorsa yatay scrollbar'ın çalıştığını doğrulayın.

### 8. Beklenen sonuç

- Parse kolonu sol tarafta ve yatay kaydırmada sabittir.
- Status kolonu mevcut dört kolonlu görünümde sağ kenardadır.
- Mission Name tablodaki kalan alanı kullanır.
- Auto genişlikli kolonlar içerik uzunluğuna göre boyutlanır.
- Kolonlar kullanıcı tarafından yeniden boyutlandırılabilir.

### 9. Geri alma

1. DataGrid'den `ColumnWidth`, `MinColumnWidth`, `CanUserResizeColumns` ve `FrozenColumnCount` özelliklerini kaldırın.
2. Parse kolonunu `Header="" Width="75"` değerine döndürün.
3. Eski `MSG ID`, `DTG`, `TASK UNIT`, `MISSION NAME`, `CALL` ve `STATUS` kolonlarını geri ekleyin.
4. Solution'ı yeniden derleyin.

---

## Değişiklik 003 — İşlem sırasında Search ve Clear butonlarını kilitleme

**Tarih:** 22 Eylül 2026  
**Hedef proje:** `MYSClient`  
**Hedef framework:** `.NET Framework 4.5 / WPF`  
**Etkilenen dosya sayısı:** 4

### Amaç

ATO veya ACO ekranında search/parse preloader'ı görünürken yeni bir aramanın başlatılmasını ve filtrelerin temizlenmesini engellemek. İşlem tamamlandığında, hata verdiğinde veya iptal edildiğinde butonlar yeniden etkinleşmelidir.

### Değiştirilecek dosyalar

1. `Views/AtoListView.xaml`
2. `Views/AtoListView.xaml.cs`
3. `Views/AcoListView.xaml`
4. `Views/AcoListView.xaml.cs`

## 1. ATO XAML değişiklikleri

### 1.1. Search butonuna isim verin

`Views/AtoListView.xaml` dosyasında aşağıdaki satırı bulun:

```xml
<Button Height="40" Cursor="Hand" Click="Search_Click">
```

Şununla değiştirin:

```xml
<Button x:Name="SearchButton" Height="40" Cursor="Hand" Click="Search_Click">
```

Search butonunun `<ControlTemplate.Triggers>` bölümünde bulunan `IsMouseOver` Trigger'ından sonra aşağıdaki Trigger'ı ekleyin:

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter TargetName="bd" Property="Opacity" Value="0.45"/>
</Trigger>
```

Trigger bölümü şöyle görünmelidir:

```xml
<ControlTemplate.Triggers>
    <Trigger Property="IsMouseOver" Value="True">
        <Setter TargetName="bd" Property="Opacity" Value="0.85"/>
    </Trigger>
    <Trigger Property="IsEnabled" Value="False">
        <Setter TargetName="bd" Property="Opacity" Value="0.45"/>
    </Trigger>
</ControlTemplate.Triggers>
```

### 1.2. Clear butonuna isim verin

Aynı dosyada aşağıdaki satırı bulun:

```xml
<Button Height="32" Cursor="Hand" Margin="0,8,0,0" Click="Clear_Click">
```

Şununla değiştirin:

```xml
<Button x:Name="ClearFiltersButton" Height="32" Cursor="Hand" Margin="0,8,0,0" Click="Clear_Click">
```

Clear butonunun `<ControlTemplate.Triggers>` bölümündeki `IsMouseOver` Trigger'ından sonra şunu ekleyin:

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter TargetName="bd" Property="Opacity" Value="0.45"/>
</Trigger>
```

## 2. ATO code-behind değişiklikleri

`Views/AtoListView.xaml.cs` dosyasında `ShowOverlay` metodunu bulun.

Metodun başında `_statusDetail = text;` satırının hemen altına şunları ekleyin:

```csharp
SearchButton.IsEnabled = false;
ClearFiltersButton.IsEnabled = false;
```

İlgili bölüm şöyle görünmelidir:

```csharp
private void ShowOverlay(string text)
{
    _serviceError = false;
    _statusDetail = text;
    SearchButton.IsEnabled = false;
    ClearFiltersButton.IsEnabled = false;
    EmptyStatePanel.Visibility = Visibility.Collapsed;
```

Aynı dosyada `HideOverlay` metodunu bulun. `SearchOverlay.Visibility = Visibility.Collapsed;` satırının hemen altına şunları ekleyin:

```csharp
SearchButton.IsEnabled = true;
ClearFiltersButton.IsEnabled = true;
```

İlgili bölüm şöyle görünmelidir:

```csharp
private void HideOverlay()
{
    SearchOverlay.Visibility = Visibility.Collapsed;
    SearchButton.IsEnabled = true;
    ClearFiltersButton.IsEnabled = true;
```

`HideOverlay` arama ve parse işlemlerinin `finally` bloğunda çağrıldığı için başarı, hata ve iptal durumlarında butonlar yeniden açılır.

## 3. ACO değişiklikleri

Yukarıdaki ATO adımlarının aynısını aşağıdaki dosyalara uygulayın:

- `Views/AcoListView.xaml`
- `Views/AcoListView.xaml.cs`

ACO XAML içindeki Search butonu:

```xml
<Button x:Name="SearchButton" Height="40" Cursor="Hand" Click="Search_Click">
```

ACO XAML içindeki Clear butonu:

```xml
<Button x:Name="ClearFiltersButton" Height="32" Cursor="Hand" Margin="0,8,0,0" Click="Clear_Click">
```

Her iki buton template'ine de şu disabled Trigger'ı ekleyin:

```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter TargetName="bd" Property="Opacity" Value="0.45"/>
</Trigger>
```

ACO `ShowOverlay` metoduna:

```csharp
SearchButton.IsEnabled = false;
ClearFiltersButton.IsEnabled = false;
```

ACO `HideOverlay` metoduna:

```csharp
SearchButton.IsEnabled = true;
ClearFiltersButton.IsEnabled = true;
```

satırlarını ekleyin.

## 4. Çalışma akışı

```text
SEARCH/PARSE başlar
    → ShowOverlay çağrılır
    → Search ve Clear disabled olur
    → Preloader görünür
    → İşlem tamamlanır, hata verir veya iptal edilir
    → finally bloğunda HideOverlay çağrılır
    → Search ve Clear yeniden enabled olur
```

## 5. Derleme kontrolü

1. Visual Studio'da `Build > Clean Solution` çalıştırın.
2. `Build > Rebuild Solution` çalıştırın.
3. Debug ve Release konfigürasyonlarında hata olmadığını doğrulayın.
4. `SearchButton` veya `ClearFiltersButton` için “does not exist in the current context” hatası alınırsa XAML içindeki `x:Name` değerlerini kontrol edin.

## 6. Manuel test adımları

1. Uygulamayı açıp login olun.
2. ATO ekranında geçerli Start/End tarihleri seçin.
3. `SEARCH` butonuna basın.
4. Preloader görünürken Search ve Clear butonlarının soluk olduğunu doğrulayın.
5. Preloader görünürken iki butona tıklanamadığını doğrulayın.
6. İşlem bittikten sonra iki butonun tekrar aktif olduğunu doğrulayın.
7. Aynı testi ACO ekranında tekrarlayın.
8. Parse işlemi başlatıldığında da iki butonun geçici olarak kilitlendiğini doğrulayın.
9. Bir servis hatası durumunda preloader kapandıktan sonra butonların yeniden aktif olduğunu doğrulayın.

## 7. Beklenen sonuç

- Aynı anda ikinci bir search başlatılamaz.
- İşlem sırasında filtreler temizlenemez.
- Butonların disabled olduğu opaklık değişiminden anlaşılır.
- İşlem sonrasında butonlar her koşulda yeniden kullanılabilir.

## 8. Geri alma

1. ATO/ACO Search butonlarından `x:Name="SearchButton"` değerini kaldırın.
2. ATO/ACO Clear butonlarından `x:Name="ClearFiltersButton"` değerini kaldırın.
3. Dört buton template'indeki `IsEnabled=False` Trigger'larını kaldırın.
4. İki `ShowOverlay` metodundaki `IsEnabled = false` satırlarını kaldırın.
5. İki `HideOverlay` metodundaki `IsEnabled = true` satırlarını kaldırın.
6. Solution'ı yeniden derleyin.
+---

## Değişiklik 004 — Ekrana geri dönüldüğünde mesaj sayısını koruma

**Tarih:** 22 Eylül 2026  
**Hedef proje:** `MYSClient`  
**Hedef framework:** `.NET Framework 4.5 / WPF`  
**Etkilenen dosya sayısı:** 3

### Amaç

ATO veya ACO aramasından sonra bottom panelde görünen `100 message(s) found` bilgisinin başka bir ekrana geçip geri dönüldüğünde kaybolmasını engellemek.

Mesaj listesi zaten aynı view instance'ında korunmaktadır. Sorun, ekran değişiminde `SetBottomContext` metodunun son işlem metnini `Ready` olarak sıfırlamasıdır. Bu değişiklik, view içinde saklanan son durum bilgisini bottom panele yeniden gönderir.

### Değiştirilecek dosyalar

1. `Views/AtoListView.xaml.cs`
2. `Views/AcoListView.xaml.cs`
3. `MainWindow.xaml.cs`

## 1. ATO ekranına durum geri yükleme metodu ekleyin

`Views/AtoListView.xaml.cs` dosyasında `UpdateHostStatus` metodunu bulun:

```csharp
private void UpdateHostStatus(string serviceStatus, string operation)
{
    MainWindow host = Window.GetWindow(this) as MainWindow;
    if (host != null && host.IsCurrentView(this))
        host.UpdateBottomStatus(serviceStatus, "ATO MESSAGES", operation);
}
```

Bu metodun hemen altına aşağıdaki public metodu ekleyin:

```csharp
public void RestoreBottomStatus()
{
    UpdateHostStatus(_serviceError ? "ERROR" : "READY", _statusDetail);
}
```

`_statusDetail`, başarılı aramadan sonra `ResultCount.Text` değerini içerdiği için mesaj sayısı korunur. Sıfır sonuçlu aramada da `0 message(s) found` değeri geri yüklenir.

## 2. ACO ekranına durum geri yükleme metodu ekleyin

`Views/AcoListView.xaml.cs` dosyasında ACO `UpdateHostStatus` metodunu bulun ve hemen altına şunu ekleyin:

```csharp
public void RestoreBottomStatus()
{
    UpdateHostStatus(_serviceError ? "ERROR" : "READY", _statusDetail);
}
```

## 3. MainWindow navigasyonunu güncelleyin

`MainWindow.xaml.cs` dosyasında `NavAto_Click` metodunu bulun.

Mevcut bölüm:

```csharp
private void NavAto_Click(object sender, RoutedEventArgs e)
{
    MainContentFrame.Content = _atoView;
    PageTitle.Text = "ATO — Air Tasking Order";
    SetActiveNavigation(NavAtoButton);
    SetBottomContext("ATO MESSAGES");
}
```

`SetBottomContext` satırından sonra aşağıdaki satırı ekleyin:

```csharp
_atoView.RestoreBottomStatus();
```

Metodun son hali:

```csharp
private void NavAto_Click(object sender, RoutedEventArgs e)
{
    MainContentFrame.Content = _atoView;
    PageTitle.Text = "ATO — Air Tasking Order";
    SetActiveNavigation(NavAtoButton);
    SetBottomContext("ATO MESSAGES");
    _atoView.RestoreBottomStatus();
}
```

Aynı işlemi `NavAco_Click` metoduna uygulayın. `SetBottomContext("ACO MESSAGES");` satırının altına şunu ekleyin:

```csharp
_acoView.RestoreBottomStatus();
```

Metodun son hali:

```csharp
private void NavAco_Click(object sender, RoutedEventArgs e)
{
    MainContentFrame.Content = _acoView;
    PageTitle.Text = "ACO — Airspace Control Order";
    SetActiveNavigation(NavAcoButton);
    SetBottomContext("ACO MESSAGES");
    _acoView.RestoreBottomStatus();
}
```

## 4. İptal edilen aramada eski durumu geri yükleyin

Bu adım, kullanıcı search devam ederken başka ekrana geçerse bottom panelin daha sonra `Searching...` durumunda kalmasını önler.

ATO `Search_Click` metodunda `ShowOverlay("Searching...");` satırından hemen önce şunları ekleyin:

```csharp
string previousStatusDetail = _statusDetail;
bool previousServiceError = _serviceError;
```

ATO search içindeki boş `OperationCanceledException` bloğunu:

```csharp
catch (OperationCanceledException)
{
}
```

şununla değiştirin:

```csharp
catch (OperationCanceledException)
{
    _statusDetail = previousStatusDetail;
    _serviceError = previousServiceError;
}
```

Aynı değişikliği ACO `Search_Click` metoduna da uygulayın.

## 5. İptal edilen parse işleminde eski durumu geri yükleyin

ATO ve ACO `ParseRow_Click` metotlarında `ShowOverlay("Parsing...");` satırından hemen önce aşağıdaki değişkenleri ekleyin:

```csharp
string previousStatusDetail = _statusDetail;
bool previousServiceError = _serviceError;
```

Her iki parse metodundaki `OperationCanceledException` bloğuna şu iki satırı ekleyin:

```csharp
_statusDetail = previousStatusDetail;
_serviceError = previousServiceError;
```

Örnek son hali:

```csharp
catch (OperationCanceledException)
{
    _statusDetail = previousStatusDetail;
    _serviceError = previousServiceError;
    msg.ParseButtonText = "Parse";
}
```

## 6. Başarılı parse durumunu finally öncesinde kaydedin

ATO parse metodunda aşağıdaki satırı bulun:

```csharp
msg.Status = "Parsed";
```

Hemen altına ekleyin:

```csharp
_statusDetail = "Completed";
```

ACO parse metodunda aşağıdaki satırı bulun:

```csharp
savedFilePath = filePath;
```

Hemen altına ekleyin:

```csharp
_statusDetail = "Completed";
```

Her iki dosyada da `if (savedFilePath != null)` bloğunun içindeki eski `_statusDetail = "Completed";` satırını kaldırın. Böylece `HideOverlay`, bottom paneli güncellerken tamamlanmış durumu önceden bilir.

## 7. Durum akışı

```text
Arama tamamlanır
    → _statusDetail = "100 message(s) found"
    → Kullanıcı Settings veya diğer mesaj ekranına geçer
    → ATO/ACO listesindeki veriler bellekte kalır
    → Kullanıcı önceki ekrana döner
    → RestoreBottomStatus çağrılır
    → Bottom panel tekrar "100 message(s) found" gösterir
```

İşlem ekran değişimi nedeniyle iptal edilirse:

```text
Searching... / Parsing...
    → ekran değiştirilir
    → CancellationToken iptal edilir
    → önceki tamamlanmış durum geri yüklenir
    → ekrana dönüldüğünde geçici işlem metni gösterilmez
```

## 8. Derleme kontrolü

1. Visual Studio'da `Build > Clean Solution` çalıştırın.
2. `Build > Rebuild Solution` çalıştırın.
3. Debug ve Release konfigürasyonlarında hata olmadığını doğrulayın.
4. `RestoreBottomStatus` bulunamadı hatası alınırsa metodun `public` olduğunu ve doğru view class'ına eklendiğini kontrol edin.

## 9. Manuel test adımları

1. Uygulamayı açıp login olun.
2. ATO ekranında tarih aralığı seçip arama yapın.
3. Bottom panelde örneğin `8 message(s) found` yazdığını doğrulayın.
4. ACO veya Settings ekranına geçin.
5. Tekrar ATO ekranına dönün.
6. Mesaj listesinin ve `8 message(s) found` bilgisinin korunduğunu doğrulayın.
7. ACO ekranında arama yapıp aynı ekran değiştirme testini tekrarlayın.
8. Sıfır sonuç veren bir arama yapın; başka ekrana geçip dönün ve `0 message(s) found` bilgisinin korunduğunu doğrulayın.
9. Search devam ederken diğer ekrana geçin; geri döndüğünüzde `Searching...` yazısının takılı kalmadığını doğrulayın.
10. Logout yapın, tekrar login olun ve filtrelerle birlikte durum bilgisinin `Ready` değerine sıfırlandığını doğrulayın.

## 10. Beklenen sonuç

- Liste ile bottom panel mesaj sayısı birbiriyle tutarlı kalır.
- ATO ve ACO kendi son durumlarını ayrı ayrı korur.
- Settings ekranına geçiş sonuç durumlarını silmez.
- İptal edilen işlemler geçici durum metni bırakmaz.
- Logout sonrasında durum temizlenir.

## 11. Geri alma

1. ATO ve ACO view'larından `RestoreBottomStatus` metotlarını kaldırın.
2. `NavAto_Click` içindeki `_atoView.RestoreBottomStatus();` satırını kaldırın.
3. `NavAco_Click` içindeki `_acoView.RestoreBottomStatus();` satırını kaldırın.
4. Search/parse metotlarındaki `previousStatusDetail` ve `previousServiceError` değişkenleriyle geri yükleme satırlarını kaldırın.
5. Parse success içinde taşınan `_statusDetail = "Completed";` satırlarını eski konumlarına döndürün.
6. Solution'ı yeniden derleyin.

---
