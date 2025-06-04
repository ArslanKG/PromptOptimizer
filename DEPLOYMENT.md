# 🚀 Render.com Deployment Guide

Bu rehber PromptOptimizer API'sini Render.com'da nasıl deploy edeceğinizi adım adım açıklar.

## 📋 Gereksinimler

- GitHub hesabı (kod repository için)
- Render.com hesabı (ücretsiz)
- Cortex API key (https://claude.gg)

## 🔧 Hazırlık Adımları

### 1. GitHub Repository Hazırlama

```bash
# Kodunuzu GitHub'a push edin
git add .
git commit -m "Add Render.com deployment configuration"
git push origin main
```

### 2. Environment Variables Hazırlama

Aşağıdaki environment variable'ları Render.com dashboard'da set etmeniz gerekiyor:

```bash
# ZORUNLU - JWT Secret Key (min 32 karakter)
JWT__SECRETKEY=your-super-secure-production-jwt-secret-key-32-chars-minimum

# ZORUNLU - Cortex API Key
CORTEXAPI__APIKEY=your-cortex-api-key-from-claude-gg

# ZORUNLU - Admin Password
ADMINSETUP__PASSWORD=YourSecureAdminPassword123!

# ZORUNLU - Admin Email
ADMINSETUP__EMAIL=admin@yourdomain.com

# OPSIYONEL - CORS için allowed origins
ALLOWEDORIGINS__0=https://your-frontend-domain.com
ALLOWEDORIGINS__1=https://your-app.onrender.com
```

## 🌐 Render.com Deployment

### Adım 1: Render.com'da Yeni Service Oluşturma

1. [Render.com](https://render.com) adresine gidin
2. "Dashboard" → "New +" → "Web Service" seçin
3. GitHub repository'nizi bağlayın
4. Repository'nizi seçin

### Adım 2: Service Konfigürasyonu

#### Basic Settings:
```
Name: promptoptimizer-api
Environment: Docker
Region: Hangi region istiyorsanız
Branch: main
```

#### Build & Deploy Settings:
```
Build Command: (boş bırakın - Dockerfile kullanacak)
Start Command: (boş bırakın - Dockerfile'da tanımlı)
```

#### Advanced Settings:
```
Dockerfile Path: ./Dockerfile
Port: 10000
Health Check Path: /health
```

### Adım 3: Environment Variables Ekleme

Render.com dashboard'da "Environment" sekmesinde aşağıdaki variable'ları ekleyin:

```bash
JWT__SECRETKEY = your-super-secure-production-jwt-secret-key-32-chars-minimum
CORTEXAPI__APIKEY = your-cortex-api-key
ADMINSETUP__PASSWORD = YourSecureAdminPassword123!
ADMINSETUP__EMAIL = admin@yourdomain.com
```

### Adım 4: Deploy

1. "Create Web Service" butonuna tıklayın
2. Build process otomatik başlayacak (5-10 dakika sürer)
3. Deploy tamamlandığında size bir URL verilecek

## 🔍 Deployment Sonrası Kontroller

### 1. Health Check
```bash
curl https://your-app.onrender.com/health
```

Beklenen response:
```json
{
  "status": "Healthy",
  "results": {
    "database": {
      "status": "Healthy"
    },
    "self": {
      "status": "Healthy"
    }
  }
}
```

### 2. API Dokümantasyonu
Swagger UI: `https://your-app.onrender.com/swagger`

### 3. Admin Login Test
```bash
curl -X POST https://your-app.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "YourSecureAdminPassword123!"
  }'
```

### 4. Public Chat Test
```bash
curl -X POST https://your-app.onrender.com/api/public/chat/send \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Merhaba! Test mesajı."
  }'
```

## 🔧 Render.com Özel Ayarlar

### Free Tier Limitations
- **Sleep Mode**: 15 dakika inaktiflikten sonra uyur
- **Cold Start**: İlk request 1-2 dakika sürebilir
- **750 saat/ay**: Ücretsiz kullanım limiti

### Database Persistence
- SQLite database `/app/data/` dizininde persist olur
- Container restart'ta data korunur
- Backup önerilir (manuel export yapabilirsiniz)

### Custom Domain (Opsiyonel)
Render.com'da "Settings" → "Custom Domains" bölümünden kendi domain'inizi ekleyebilirsiniz.

## 🚨 Güvenlik Notları

### JWT Secret Key
```bash
# Güçlü bir secret key oluşturun (min 32 karakter)
openssl rand -base64 32
```

### CORS Configuration
Production'da sadece güvendiğiniz domain'leri allowed origins'e ekleyin:
```bash
ALLOWEDORIGINS__0=https://yourdomain.com
ALLOWEDORIGINS__1=https://yourapp.com
```

### Admin Credentials
- Güçlü admin password kullanın
- İlk login'den sonra password'u değiştirin
- Admin email'i gerçek email adresiniz olsun

## 📊 Monitoring & Logs

### Render.com Logs
Dashboard'da "Logs" sekmesinden real-time logları görüntüleyebilirsiniz.

### Health Monitoring
- Health check endpoint: `/health`
- Uptime monitoring için external servis kullanın (UptimeRobot vs.)

### Performance
- First request: ~2 saniye (cold start)
- Subsequent requests: ~100-500ms

## 🔄 Güncelleme & Deployment

### Otomatik Deployment
GitHub'a her push'ta otomatik deploy olur:

```bash
git add .
git commit -m "Update feature"
git push origin main
# Render.com otomatik build başlatacak
```

### Manual Deployment
Render.com dashboard'da "Manual Deploy" → "Deploy latest commit"

## 🆘 Troubleshooting

### Build Hatası
1. Logs'u kontrol edin
2. Environment variables'ları kontrol edin
3. Dockerfile syntax'ını kontrol edin

### Runtime Hatası
1. Health check'i test edin
2. Environment variables eksik mi kontrol edin
3. Database connection sorunları için logs'a bakın

### Performance Sorunları
1. Free tier limitlerine yaklaştınız mı kontrol edin
2. Cold start süresini dikkate alın
3. Health check ping'i yaparak warm tutun

## 📞 Destek

- Render.com Documentation: https://render.com/docs
- Bu proje için: GitHub Issues

---

**🎉 Tebrikler! API'niz artık canlıda!**

Swagger UI: `https://your-app.onrender.com/swagger`