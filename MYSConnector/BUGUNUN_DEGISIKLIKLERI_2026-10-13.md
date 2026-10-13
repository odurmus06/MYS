# MYSClient — 13 Ekim 2026 değişiklik aktarım notu

Bu belge, bugünkü UI ve filtre iyileştirmelerini kapalı ortamdaki `MYSClient` WPF projesine elle aktarmak içindir. Proje hedefi `.NET Framework 4.5`'tir; MVC/Core dönüşümü yapılmaz.

> **Entegrasyon sınırı:** Kapalı ortamdaki mevcut arama akışında web servisinin generate edilmiş sınıflarını ve ayrı solution'daki parser'ı koruyun; bu not ikisini yeniden yazmaz. DatePicker alanları gün bazlı kalır; saat/dakika seçimi eklenmez. Bu çalışma kopyasındaki `SearchMessagesAsync` ise generate edilmiş servise bağlanmamış bir iskelettir. Bu nedenle aşağıdaki servis adımları, kapalı ortamdaki gerçek mevcut servis çağrısına uyarlanmalıdır; buradaki iskele kodunu oraya taşımayın.

## Değişiklik özeti

| No | Değişiklik | Dosyalar | Beklenen sonuç |
|---|---|---|---|
| 1 | Login required alanları kırmızı vurgulama | `MainWindow.xaml`, `MainWindow.xaml.cs` | Boş kullanıcı adı/parola ilgili alanı kırmızı gösterir; yazıldıkça uyarı güncellenir; logout eski uyarıyı siler. |
| 2 | ATO/ACO DataGrid başlık-hücre hizası ve sade kolon tutamaçları | `Views/AtoListView.xaml`, `Views/AcoListView.xaml` | Başlık ve metin aynı iç boşlukla başlar; kolon boyutlandırma çalışır, tutamaçlarda koyu dikey çizgiler görünmez. |
| 3 | Enter ile arama | `Views/AtoListView.xaml`, `Views/AcoListView.xaml` | Enter, geçerli filtre doğrulamasından geçerek Search işlemini başlatır. |
| 4 | ATO Min Publish Date için bugün üst sınırı | `Views/AtoListView.xaml`, `Views/AtoListView.xaml.cs` | Takvim gelecek günleri seçtirmez; metinle gelecek tarih girilirse alan uyarılır ve arama başlamaz. |

---

## 1. Login'de boş alanları kırmızı gösterme

### `MainWindow.xaml`

`LoginUsername` alanını bulun. Önceki halinde Border'ın adı ve TextBox'ın `TextChanged` olayı yoktur:

```xml
<Border Background="{DynamicResource ThemeBgInput}" CornerRadius="8"
        BorderBrush="{DynamicResource ThemeBorderGlassBrush}"
        BorderThickness="1" Margin="0,0,0,16">
    <TextBox x:Name="LoginUsername" KeyDown="LoginField_KeyDown"
             Background="Transparent" BorderThickness="0"
             Foreground="{DynamicResource ThemeTextPrimaryBrush}"
             Padding="12,10" FontSize="13"
             CaretBrush="{DynamicResource ThemeAccentPrimaryBrush}"/>
</Border>
```

Bu bloğu aşağıdakiyle değiştirin:

```xml
<Border x:Name="LoginUsernameBorder" Background="{DynamicResource ThemeBgInput}"
        CornerRadius="8" BorderBrush="{DynamicResource ThemeBorderGlassBrush}"
        BorderThickness="1" Margin="0,0,0,16">
    <TextBox x:Name="LoginUsername" KeyDown="LoginField_KeyDown"
             TextChanged="LoginUsername_TextChanged"
             Background="Transparent" BorderThickness="0"
             Foreground="{DynamicResource ThemeTextPrimaryBrush}"
             Padding="12,10" FontSize="13"
             CaretBrush="{DynamicResource ThemeAccentPrimaryBrush}"/>
</Border>
```

`LoginPassword` alanında önceki kod:

```xml
<Border Background="{DynamicResource ThemeBgInput}" CornerRadius="8"
        BorderBrush="{DynamicResource ThemeBorderGlassBrush}"
        BorderThickness="1" Margin="0,0,0,24">
    <PasswordBox x:Name="LoginPassword" KeyDown="LoginField_KeyDown"
                 Background="Transparent" BorderThickness="0"
                 Foreground="{DynamicResource ThemeTextPrimaryBrush}"
                 Padding="12,10" FontSize="13"
                 CaretBrush="{DynamicResource ThemeAccentPrimaryBrush}"/>
</Border>
```

Şununla değiştirin:

```xml
<Border x:Name="LoginPasswordBorder" Background="{DynamicResource ThemeBgInput}"
        CornerRadius="8" BorderBrush="{DynamicResource ThemeBorderGlassBrush}"
        BorderThickness="1" Margin="0,0,0,24">
    <PasswordBox x:Name="LoginPassword" KeyDown="LoginField_KeyDown"
                 PasswordChanged="LoginPassword_PasswordChanged"
                 Background="Transparent" BorderThickness="0"
                 Foreground="{DynamicResource ThemeTextPrimaryBrush}"
                 Padding="12,10" FontSize="13"
                 CaretBrush="{DynamicResource ThemeAccentPrimaryBrush}"/>
</Border>
```

Diğer özellikler varsa koruyun; değişiklikler Border'a isim vermek ve alan değişikliği olayını eklemektir.

### `MainWindow.xaml.cs`

Önceki `using` bölümünde aşağıdaki satır yoktur; ekleyin:

```csharp
using System.Windows.Media;
```

`MainWindow` alanları arasında aşağıdaki alan yoktur; ekleyin:

```csharp
private bool _loginRequiredValidationShown;
```

`LoginButton_Click` içindeki önceki boş alan kontrolünü bulun:

```csharp
string user = LoginUsername.Text != null ? LoginUsername.Text.Trim() : "";
string pass = LoginPassword.Password != null ? LoginPassword.Password.Trim() : "";

if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
{
    ShowLoginError("Please enter username and password.");
    return;
}
```

Bu kontrolü şununla değiştirin:

```csharp
_loginRequiredValidationShown = true;
if (!ValidateLoginFields())
    return;
```

`LoginField_KeyDown` sonrasında önceden bulunmayan aşağıdaki metotları ekleyin:

```csharp
private void LoginUsername_TextChanged(object sender, TextChangedEventArgs e)
{
    if (_loginRequiredValidationShown)
        ValidateLoginFields();
}

private void LoginPassword_PasswordChanged(object sender, RoutedEventArgs e)
{
    if (_loginRequiredValidationShown)
        ValidateLoginFields();
}

private bool ValidateLoginFields()
{
    bool missingUsername = string.IsNullOrWhiteSpace(LoginUsername.Text);
    bool missingPassword = string.IsNullOrWhiteSpace(LoginPassword.Password);
    SetLoginRequiredBorders(missingUsername, missingPassword);

    if (missingUsername || missingPassword)
    {
        ShowLoginError(missingUsername && missingPassword
            ? "Please enter username and password."
            : missingUsername ? "Please enter username." : "Please enter password.");
        return false;
    }

    _loginRequiredValidationShown = false;
    LoginError.Visibility = Visibility.Collapsed;
    return true;
}

private void SetLoginRequiredBorders(bool missingUsername, bool missingPassword)
{
    Brush errorBrush = (Brush)FindResource("ThemeAccentRedBrush");
    Brush normalBrush = (Brush)FindResource("ThemeBorderGlassBrush");
    LoginUsernameBorder.BorderBrush = missingUsername ? errorBrush : normalBrush;
    LoginUsernameBorder.BorderThickness = new Thickness(missingUsername ? 2 : 1);
    LoginPasswordBorder.BorderBrush = missingPassword ? errorBrush : normalBrush;
    LoginPasswordBorder.BorderThickness = new Thickness(missingPassword ? 2 : 1);
}
```

`Logout_Click` içindeki eski temizleme kodu şöyledir:

```csharp
_atoView.ResetFilters();
_acoView.ResetFilters();
LoginUsername.Text = "";
LoginPassword.Password = "";
LoginError.Visibility = Visibility.Collapsed;
```

Şu hale getirin:

```csharp
_atoView.ResetFilters();
_acoView.ResetFilters();
_loginRequiredValidationShown = false;
LoginUsername.Text = "";
LoginPassword.Password = "";
SetLoginRequiredBorders(false, false);
LoginError.Visibility = Visibility.Collapsed;
```

Oturum zaman aşımı `Logout_Click` yolunu kullanıyorsa ayrıca değişiklik gerekmez.

---

## 2. ATO/ACO DataGrid başlık ve satır hizası

İki XAML dosyasında da ilgili DataGrid'in `DataGrid.Resources` bölümünü düzenleyin.

### 2.1 Başlık template'ı ve dikey tutamaç çizgileri

Önce `Style TargetType="DataGridColumnHeader"` bölümünü bulun. Değişiklik öncesinde stil, `BorderBrush` setter'ından sonra kapanır ve WPF'in varsayılan header template'ını kullanır. Aranacak son satırlar:

```xml
<Setter Property="BorderBrush" Value="{DynamicResource ThemeBorderGlassBrush}"/>
</Style>
```

`</Style>` satırını kaldırmayın; aşağıdaki yeni Template setter'ını bu satırın hemen öncesine ekleyin. Header stilinde aşağıdaki hizalama değerleri de bulunmalıdır:

```xml
<Setter Property="Padding" Value="10,0"/>
<Setter Property="HorizontalContentAlignment" Value="Left"/>
<Setter Property="VerticalContentAlignment" Value="Center"/>
```

Sonrasında `DataGridColumnHeader` stili, mevcut setter'lar korunarak ve yeni Template kapanış `</Style>` öncesine eklenerek şöyle bitmelidir:

```xml
<Setter Property="BorderBrush" Value="{DynamicResource ThemeBorderGlassBrush}"/>
<Setter Property="Template">
    <Setter.Value>
        <!-- Aşağıdaki tam ControlTemplate bloğunu buraya koyun. -->
    </Setter.Value>
</Setter>
</Style>
```

`<!-- ... -->` satırı açıklamadır, XAML'e aynen yapıştırmayın; tam template aşağıdadır. Saydam `Thumb` şablonları kolon genişliğini fareyle değiştirmeyi korurken koyu tutamaç çizgilerini gizler; sıralama oku da çalışır:

```xml
<Setter Property="Template">
    <Setter.Value>
        <ControlTemplate TargetType="DataGridColumnHeader">
            <Grid>
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        Padding="{TemplateBinding Padding}"
                        SnapsToDevicePixels="True">
                    <DockPanel LastChildFill="True">
                        <Path x:Name="SortArrow" DockPanel.Dock="Right"
                              Width="8" Height="5" Margin="6,0,0,0"
                              Stretch="Fill" Fill="{TemplateBinding Foreground}"
                              Visibility="Collapsed"
                              Data="M 0,4 L 4,0 L 8,4 Z"/>
                        <ContentPresenter RecognizesAccessKey="True"
                                          HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                          VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                                          SnapsToDevicePixels="{TemplateBinding SnapsToDevicePixels}"/>
                    </DockPanel>
                </Border>
                <Thumb x:Name="PART_LeftHeaderGripper" Width="4"
                       HorizontalAlignment="Left" Cursor="SizeWE"
                       Background="Transparent" Focusable="False">
                    <Thumb.Template>
                        <ControlTemplate TargetType="Thumb">
                            <Border Background="Transparent"/>
                        </ControlTemplate>
                    </Thumb.Template>
                </Thumb>
                <Thumb x:Name="PART_RightHeaderGripper" Width="4"
                       HorizontalAlignment="Right" Cursor="SizeWE"
                       Background="Transparent" Focusable="False">
                    <Thumb.Template>
                        <ControlTemplate TargetType="Thumb">
                            <Border Background="Transparent"/>
                        </ControlTemplate>
                    </Thumb.Template>
                </Thumb>
            </Grid>
            <ControlTemplate.Triggers>
                <Trigger Property="SortDirection" Value="Ascending">
                    <Setter TargetName="SortArrow" Property="Visibility" Value="Visible"/>
                    <Setter TargetName="SortArrow" Property="Data" Value="M 0,4 L 4,0 L 8,4 Z"/>
                </Trigger>
                <Trigger Property="SortDirection" Value="Descending">
                    <Setter TargetName="SortArrow" Property="Visibility" Value="Visible"/>
                    <Setter TargetName="SortArrow" Property="Data" Value="M 0,0 L 4,4 L 8,0 Z"/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>
    </Setter.Value>
</Setter>
```

### 2.2 Hücre metni ve satır vurgusu

ATO metin kolonunda önceki örnek kod `ElementStyle` olmadan görünür:

```xml
<DataGridTextColumn Header="MESSAGE ID" Binding="{Binding MsgId}"
                    Width="Auto" MinWidth="130"/>
```

Şununla değiştirin:

```xml
<DataGridTextColumn Header="MESSAGE ID" Binding="{Binding MsgId}"
                    Width="Auto" MinWidth="130"
                    ElementStyle="{StaticResource AlignedCellTextStyle}"/>
```

ACO'daki metin kolonlarına da aynı `ElementStyle` özelliğini ekleyin. Her iki DataGrid'in `DataGrid.Resources` bölümüne aşağıdaki yeni TextBlock stili eklenmelidir:

```xml
<Style x:Key="AlignedCellTextStyle" TargetType="TextBlock">
    <Setter Property="Margin" Value="0"/>
    <Setter Property="VerticalAlignment" Value="Center"/>
</Style>
```

Mevcut hücre stilinde `Padding="10,0"` olmalı ve hücre Template'ındaki Border padding'i `{TemplateBinding Padding}`'e bağlanmalıdır. Satırın eski sol vurgusu BorderThickness ile hücreleri sağa itiyorsa aşağıdaki eski satırı bulun:

```xml
<Border x:Name="RowBorder" Background="{TemplateBinding Background}"
        BorderThickness="3,0,0,0" CornerRadius="6" BorderBrush="Transparent">
```

Yeni halinde Border kalınlığı sıfır olur; sol vurgu ayrı Rectangle olarak çizilir:

```xml
<Border x:Name="RowBorder" Background="{TemplateBinding Background}"
        BorderThickness="0" CornerRadius="6" BorderBrush="Transparent">
    <Grid>
        <!-- Mevcut SelectiveScrollingGrid'i burada aynen koruyun. -->
        <Rectangle x:Name="RowAccent" Width="3" RadiusX="6" RadiusY="6"
                   HorizontalAlignment="Left" Fill="Transparent"
                   IsHitTestVisible="False"/>
    </Grid>
</Border>
```

Hover/seçim trigger'larında da `RowBorder.BorderBrush` yerine `RowAccent.Fill` değerini güncelleyin. Böylece vurgu hücreleri itmez.

Örneğin hover trigger'ındaki eski satır:

```xml
<Setter TargetName="RowBorder" Property="BorderBrush"
        Value="{DynamicResource ThemeAccentPrimaryBrush}"/>
```

Yeni satır:

```xml
<Setter TargetName="RowAccent" Property="Fill"
        Value="{DynamicResource ThemeAccentPrimaryBrush}"/>
```

Seçili satır için de `ThemeAccentSecondaryBrush` değerini koruyarak aynı hedef/properties değişikliğini yapın.

Bu ortak hizalama/kolon tutamacı değişikliğini iki DataGrid'e de uygulayın; kolon genişliğini değiştirme ve sıralama davranışını kaldırmayın.

---

## 3. Enter ile Search

`Views/AtoListView.xaml` ve `Views/AcoListView.xaml` içinde filtre alanındaki `SearchButton` tanımını bulun. Önce:

```xml
<Button x:Name="SearchButton" Height="40" Cursor="Hand" Click="Search_Click">
```

Sonra:

```xml
<Button x:Name="SearchButton" Height="40" Cursor="Hand"
        Click="Search_Click" IsDefault="True">
```

Mevcut stil ve diğer özellikleri koruyun. Search handler'ındaki required tarih doğrulaması değişmeden kalmalıdır.

---

## 4. ATO Min Publish Date bugünle sınırlandırma

### `Views/AtoListView.xaml`

Önce `SearchMinPublishDate` tanımını bulun. Değişiklik öncesi hali:

```xml
<DatePicker x:Name="SearchMinPublishDate" Style="{StaticResource FieldDate}"/>
```

Şununla değiştirin ve aynı kapsayıcı Grid'e validation Border'ını ekleyin:

```xml
<DatePicker x:Name="SearchMinPublishDate" Style="{StaticResource FieldDate}"
            SelectedDateChanged="SearchMinPublishDate_SelectedDateChanged"
            DateValidationError="SearchMinPublishDate_DateValidationError"/>
<Border x:Name="MinPublishDateValidationBorder"
        BorderBrush="{DynamicResource ThemeAccentRedBrush}"
        BorderThickness="2" CornerRadius="8"
        IsHitTestVisible="False" Visibility="Collapsed"/>
```

DatePicker Grid'inin hemen altına uyarı metnini ekleyin:

```xml
<TextBlock x:Name="MinPublishDateValidationError"
           Text="Min Publish Date cannot be after today."
           Foreground="{DynamicResource ThemeAccentRedBrush}" FontSize="10"
           TextWrapping="Wrap" Margin="0,-10,0,10" Visibility="Collapsed"/>
```

### `Views/AtoListView.xaml.cs`

Sınıfta ve constructor'da bu kodlar önceden yoktur. Alanı sınıfa ekleyin:

```csharp
private bool _rejectingFuturePublishDate;
```

Constructor'da `InitializeComponent();` satırından hemen sonra takvim üst sınırını ekleyin:

```csharp
SearchMinPublishDate.DisplayDateEnd = DateTime.Today;
```

`Search_Click` içindeki önceki doğrulamayı bulun:

```csharp
if (!ValidateRequiredDates())
    return;
```

Şununla değiştirin:

```csharp
if (!ValidateRequiredDates() || !ValidateMinPublishDate())
    return;
```

Önce aşağıdaki iki event handler ve iki doğrulama metodu yoktur. Sınıfa ekleyin:

```csharp
private void SearchMinPublishDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
{
    if (_rejectingFuturePublishDate)
        return;

    DateTime? selectedDate = SearchMinPublishDate.SelectedDate;
    if (selectedDate.HasValue && selectedDate.Value.Date > DateTime.Today)
    {
        _rejectingFuturePublishDate = true;
        SearchMinPublishDate.SelectedDate = null;
        _rejectingFuturePublishDate = false;
        ShowMinPublishDateValidationError();
        return;
    }

    MinPublishDateValidationError.Visibility = Visibility.Collapsed;
    MinPublishDateValidationBorder.Visibility = Visibility.Collapsed;
}

private void SearchMinPublishDate_DateValidationError(
    object sender, DatePickerDateValidationErrorEventArgs e)
{
    DateTime enteredDate;
    if (DateTime.TryParse(e.Text, out enteredDate) && enteredDate.Date > DateTime.Today)
    {
        e.ThrowException = false;
        _rejectingFuturePublishDate = true;
        SearchMinPublishDate.SelectedDate = null;
        _rejectingFuturePublishDate = false;
        ShowMinPublishDateValidationError();
    }
}

private bool ValidateMinPublishDate()
{
    DateTime? selectedDate = SearchMinPublishDate.SelectedDate;
    if (selectedDate.HasValue && selectedDate.Value.Date > DateTime.Today)
    {
        ShowMinPublishDateValidationError();
        return false;
    }

    return MinPublishDateValidationError.Visibility != Visibility.Visible;
}

private void ShowMinPublishDateValidationError()
{
    MinPublishDateValidationError.Visibility = Visibility.Visible;
    MinPublishDateValidationBorder.Visibility = Visibility.Visible;
}
```

Mevcut `ResetFilters()` içinde Min Publish Date uyarısı önceden sıfırlanmıyordu. Son temizleme satırlarından sonra aşağıdakileri ekleyin:

```csharp
MinPublishDateValidationError.Visibility = Visibility.Collapsed;
MinPublishDateValidationBorder.Visibility = Visibility.Collapsed;
```

---

## Derleme ve kapalı ortam test listesi

Visual Studio'da `MYSClient.sln` açın. Debug ve Release konfigürasyonlarında Rebuild çalıştırın. Hedef Framework'ün `.NET Framework 4.5` kaldığını ve servis referanslarının/proxy sınıflarının çözümlendiğini kontrol edin.

| Test | Adımlar | Beklenen |
|---|---|---|
| İlk login görünümü | Uygulamayı açın; giriş denemesi yapmadan önce alanlara bakın. | Alanlar başlangıçta normal renkte. |
| Required login | İki alan boşken SIGN IN; sonra alanları tek tek doldurun. | Eksik alan(lar) kırmızı; yazıldıkça eksik alanın vurgusu kalkar ve mesaj güncellenir. |
| Logout temizliği | Giriş sonrası logout yapın. | Alanlar ve eski required uyarısı temizlenmiş olur. |
| DataGrid görünümü | ATO ve ACO ekranlarında başlık/hücre başlangıçlarını karşılaştırın; kolonları büyütüp küçültün. | Aynı kolon başlığı ve metin aynı X başlangıcına sahip; tutamak çizgileri görünmez; resize/sort çalışır. |
| Enter ile arama | Gerekli tarihleri seçip filtre alanındayken Enter'a basın. | Search düğmesine basılmış gibi arama ve doğrulama başlar. |
| Gelecek Min Publish Date | Takvimden gelecek gün seçmeyi ve alana gelecek tarih yazmayı deneyin. | Gelecek tarih kabul edilmez; alan kırmızı uyarılır; servis araması başlamaz. |
| Tarih-only davranışı | Tarih seçicileri açıp arama isteğini kontrol edin. | Kullanıcı yalnızca gün seçer; Start gün başına, End seçilen günün sonuna kadar filtrelenir. |
| Servis/Release kontrolü | Gerçek servis bağlantısıyla Release başlatın; servis hata/boş yanıt senaryolarını deneyin. | UI donmaz; sonuç sayısı ve boş/hata durumu doğru kalır. |

> **Kaynak farkı notu:** Bu workspace'te ATO/ACO için `LoadDummyData()` ve örnek koleksiyon üzerinde çalışan `SearchMessagesAsync` iskeleti bulunuyor. Kapalı ortamdaki mevcut generate edilmiş servis akışını bu örneklerle değiştirmeyin.

## Geri alma yaklaşımı

Her değişiklik yalnızca bu belgedeki ilgili dosya ve kontrol/metot adlarından geri alınabilir. Geri almadan önce kapalı ortamda dosyaların yedeğini alın; tüm XAML kaynaklarını veya servis referanslarını topluca silmeyin. Her geri alma sonrasında Debug ve Release Rebuild yapın.
