# 📁 MCP BAŞLANGIÇ YÖNERGESİ

> **Bu dosya PromptOptimizer projesinin yapısını, işlevlerini ve geliştirme kurallarını detaylı şekilde açıklar. Tüm AI destekli geliştirme ve bakım süreçlerinde referans alınmalıdır. Her yeni task öncesi bu dosya okunmalı ve bağlam yüklenmelidir.**

---

## ◇ 1. PROJE ÖZETİ

- **Proje Adı:** PromptOptimizer  
- **Amacı:** Kullanıcıdan alınan doğal dildeki promptları analiz ederek, daha net, teknik, yaratıcı veya analitik hale getiren ve ardından AI modelleriyle optimize edilmiş yanıtlar üreten bir API servisidir. Özellikle LLM tabanlı uygulamalarda, kullanıcıdan gelen girdilerin kalitesini artırmak için kullanılır.  
- **Ana Teknolojiler:** .NET 8 (C# 12), ASP.NET Core, Entity Framework Core, JWT tabanlı kimlik doğrulama, LLM API istemcisi (ICortexApiClient)  
- **Uygulama Türü:** API (Backend Service)  
- **Başlangıç Tarihi:** [Doldurulacak]  
- **Geliştirici(ler):** [Doldurulacak]  

---

## ◇ 2. ANA ÖZELLİKLER & FONKSİYONLAR

1. **Prompt Optimizasyonu:** Kullanıcıdan gelen promptları analiz edip, belirsiz ifadeleri tespit ederek daha iyi hale getirir.  
2. **Konu Çıkarımı:** Konuşma geçmişinden promptun ana konusunu otomatik olarak belirler (AI ve regex tabanlı).  
3. **Çoklu Strateji Desteği:** "quality", "speed", "consensus", "cost_effective" gibi farklı model/yanıt stratejileriyle çalışır.  
4. **Oturum (Session) ve Hafıza Yönetimi:** Kullanıcıya özel oturumlar ve geçmiş mesajlardan bağlam oluşturma, session bazlı context yönetimi.  
5. **JWT ile Güvenli API:** Kimlik doğrulama ve kullanıcıya özel işlem desteği.  
6. **Model Orkestrasyonu:** Farklı AI modelleriyle (örn. gpt-4o, o3-mini, grok-3-mini-beta) çalışma ve sonuçları birleştirme.  
7. **Gelişmiş Loglama ve Hata Yönetimi:** Tüm önemli işlemler ve hatalar detaylı şekilde loglanır.  
8. **Veritabanı Oturum Yönetimi:** Session ve mesajlar Entity Framework Core ile kalıcı olarak saklanır.  

---

## ◇ 3. TEKNOLOJİ YIĞINI (Tech Stack)

- **Backend:** .NET 8, ASP.NET Core, C# 12  
- **LLM API:** ICortexApiClient (OpenAI/benzeri API istemcisi)  
- **Kimlik Doğrulama:** JWT (UserSecrets ile güvenli anahtar yönetimi)  
- **Veritabanı:** Entity Framework Core, AppDbContext, SQL tabanlı (örn. SQL Server veya SQLite)  
- **Diğer:** UserSecrets, Serilog/Microsoft.Extensions.Logging, Regex, Dependency Injection  

---

## ◇ 4. KOD YAPISI (Dosya/Yol Bilgileri)

- **Ana dizin:** [Proje ana dizini]  
- **Önemli klasörler:**  
  - `/PromptOptimizer.Application/Services` → İş mantığı (PromptOptimizerService, ModelOrchestrator, DatabaseSessionService)  
  - `/PromptOptimizer.Core/DTOs` → Veri transfer nesneleri (ChatCompletionRequest, OptimizationRequest, vb.)  
  - `/PromptOptimizer.Core/Entities` → Temel veri modelleri (ConversationMessage, ConversationSession, vb.)  
  - `/PromptOptimizer.Core/Interfaces` → Servis arayüzleri (IPromptOptimizerService, ICortexApiClient, ISessionCacheService, vb.)  
  - `/PromptOptimizer.Infrastructure/Services` → Altyapı servisleri (ör. DatabaseSessionService, AppDbContext)  
  - `/AppData/Roaming/Microsoft/UserSecrets` → Geliştirme ortamı için gizli anahtarlar (JWT, API Key)  

---

## ◇ 5. GÖREV LİSTESİ / ROADMAP

### ✅ Yapıldı:
- Prompt optimizasyon algoritması (belirsiz terim tespiti, konu çıkarımı, prompt temizleme)  
- Model orkestrasyonu ve çoklu strateji desteği  
- JWT ile kimlik doğrulama ve kullanıcı oturumu yönetimi  
- Oturum bazlı hafıza ve bağlam yönetimi  
- Gelişmiş loglama ve hata yönetimi  
- Veritabanı tabanlı session ve mesaj yönetimi (DatabaseSessionService, AppDbContext)  

### 🔜 Yapılacaklar:
- API endpointlerinin Swagger ile dokümantasyonu  
- Unit ve integration testlerinin kapsamının artırılması  
- Performans ve maliyet optimizasyonları  
- Kullanıcı arayüzü (varsa) ile entegrasyon  
- Uzun vadeli oturum saklama ve veri gizliliği  

### ❓ Bilinmeyen veya araştırılacak konular:
- LLM yanıtlarının daha iyi değerlendirilmesi için ek metrikler  
- Farklı LLM sağlayıcılarının entegrasyonu  
- Regex ve AI tabanlı içerik ayrıştırma  
- Production ortamında güvenli anahtar ve veri yönetimi  

---

## ◇ 6. MCP DESTEKLİ KURALLAR (İsteğe Bağlı)

### 💻 Kod yazarken:
- Fonksiyon ve değişken isimleri İngilizce olmalı  
- Her fonksiyonun başında `summary` açıklaması bulunmalı  
- DRY, SOLID ve Clean Code prensiplerine uyulmalı  
- Loglama ve hata yönetimi atlanmamalı  
- Entity ve DTO'lar açıkça tanımlanmalı, migration'lar güncel tutulmalı  

### 💬 Prompt’larda:
- Kod yazmadan önce kısa bir plan sunulmalı  
- İstek belirsizse kullanıcıya soru sorulmalı  
- Kod örnekleri C# 12 ve .NET 8 uyumlu olmalı  

---

## ◇ 7. BELLEK / HAFIZA NOTLARI (Memory bank için)

**🧠 Notlar:**
- Kullanıcı Türkçe iletişim kurar, kodlar ve fonksiyonlar İngilizce yazılır.  
- Proje, LLM tabanlı uygulamalarda prompt kalitesini ve verimliliğini artırmaya odaklıdır.  
- Performans ve maliyet optimizasyonu önemlidir.  
- Kodda UserSecrets ile gizli anahtarlar yönetilir, production için farklı bir gizli anahtar yönetimi gerekebilir.  
- Prompt optimizasyonunda regex ve AI tabanlı analiz birlikte kullanılır.  
- Oturum yönetimi ve bağlam, model yanıtlarının kalitesini artırmak için kullanılır.  
- Veritabanı yapısı: ConversationSession (oturumlar), ConversationMessage (mesajlar), User (kullanıcılar, opsiyonel)  
- DatabaseSessionService, AppDbContext ile tüm session ve mesaj işlemlerini yönetir, loglar ve veri bütünlüğünü sağlar.  

---

## ◇ 8. VERİTABANI YAPISI VE SERVİSLERİ

**Veritabanı:**  
- Entity Framework Core ile yönetilir.  
- Ana tablolar:  
  - `ConversationSession`: Oturum kimliği, kullanıcı, başlık, zaman damgaları, aktiflik, mesajlar (JSON veya ilişkili tablo)  
  - `ConversationMessage`: Oturumla ilişkili, rol, içerik, zaman, model bilgisi  
  - `User`: (Varsa) Oturumları kullanıcıya bağlar  

**DatabaseSessionService:**  
- Tüm oturum ve mesaj işlemlerini (CRUD) AppDbContext üzerinden gerçekleştirir.  
- Oturum oluşturma, mesaj ekleme, güncelleme, silme, oturum sorgulama, kullanıcıya göre filtreleme gibi işlemleri kapsar.  
- Tüm işlemler loglanır ve hata yönetimi yapılır.  
- Servis, session tabanlı hafıza ve bağlam yönetimi için merkezi rol oynar.  

---

## ◇ KULLANIM TALİMATI

Bu dosyayı proje kök dizinine `mcp_context.md` adıyla ekleyin ve her yeni task öncesi mutlaka okuyun: