# 🧠 PromptOptimizer - AI Prompt Optimization Backend API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)](README.md)
[![API Version](https://img.shields.io/badge/API-v2.0-blue)](README.md)

---

## 📋 Genel Bakış

PromptOptimizer, geliştiricilerin kendi projelerine entegre edebilecekleri kapsamlı bir AI prompt optimizasyon backend API'sidir. Clean Architecture prensiplerine uygun olarak .NET 8 ile geliştirilmiş, production-ready bir çözümdür.

### 🎯 Projenin Amacı
- Kullanıcı promptlarını farklı stratejilerle optimize etme
- Çoklu AI model desteği ile en iyi yanıtları üretme
- Session tabanlı konuşma yönetimi
- Güvenli JWT authentication sistemi
- Rate limiting ve monitoring özellikleri

### ✨ Ana Özellikler

#### 🤖 AI & Model Desteği
- ✅ **Multi-Model Support**: GPT-4o, GPT-4o-mini, DeepSeek-V3, Grok-2-Vision-1212, Claude-3.5-Haiku
- ✅ **Smart Strategy System**: Quality, Speed, Consensus, Cost Effective stratejileri
- ✅ **Optimization Types**: Clarity, Technical, Creative, Analytical
- ✅ **Real-time Chat**: Streaming response desteği
- ✅ **Context Management**: Session bazlı konuşma geçmişi

#### 🌐 Public API
- ✅ **No-Auth Chat**: Authentication olmadan AI chat
- ✅ **Cost-Effective**: Sadece GPT-4o-mini model
- ✅ **Rate Limited**: Saatlik 30 request limiti
- ✅ **IP-Based Limiting**: Client IP bazlı kontrol
- ✅ **Memory-Free**: Session tutmayan lightweight service

#### 🔐 Güvenlik & Authentication
- ✅ **JWT Bearer Authentication**: Secure token-based auth
- ✅ **User Management**: Admin/User role sistemi
- ✅ **Rate Limiting**: API abuse koruması
- ✅ **Password Hashing**: BCrypt ile güvenli şifreleme

#### 🛠️ Teknik Özellikler
- ✅ **Clean Architecture**: Modüler ve sürdürülebilir kod yapısı
- ✅ **Entity Framework Core**: SQLite database
- ✅ **Serilog Logging**: Structured logging
- ✅ **Polly Resilience**: HTTP retry policies
- ✅ **Health Checks**: Sistem durumu monitöring
- ✅ **Swagger/OpenAPI**: İnteraktif API dokümantasyonu

### 🔗 Desteklenen AI Modelleri

| Model | Provider | Önerilen Kullanım | Maliyet | Hız |
|-------|----------|-------------------|---------|-----|
| GPT-4o | OpenAI | Genel amaçlı, kaliteli | Orta | Orta |
| GPT-4o-mini | OpenAI | Hızlı işlemler | Düşük | Yüksek |
| DeepSeek-V3 | DeepSeek | Kod ve teknik | Düşük | Yüksek |
| Grok-2-Vision-1212 | xAI | Görsel analiz | Yüksek | Orta |
| Claude-3.5-Haiku | Anthropic | Yaratıcı içerik | Orta | Yüksek |

---

## 🏗️ Mimari ve Teknolojiler

### Clean Architecture Katmanları

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              API Controllers                        │   │
│  │  • AuthController    • ChatController               │   │
│  │  • SessionController • SystemController             │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                   Services                          │   │
│  │  • AuthService       • ChatService                 │   │
│  │  • ValidationService • RateLimitService            │   │
│  │  • SessionTitleGenerator                           │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                      Core Layer                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │    Entities         │        Interfaces             │   │
│  │  • User             │  • IChatService               │   │
│  │  • ConversationSession  │  • IAuthService          │   │
│  │  • Message          │  • ICortexApiClient           │   │
│  │                     │  • ISessionService            │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Data Access        │     External Services         │   │
│  │  • AppDbContext     │  • CortexApiClient            │   │
│  │  • Migrations       │  • JwtTokenService            │   │
│  │  • DatabaseSession  │  • PasswordHashingService     │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 🛠️ Teknoloji Stack

#### Backend Framework
- **.NET 8.0** - Latest LTS framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 8.0** - ORM

#### Database & Storage
- **SQLite** - Lightweight, file-based database
- **Entity Framework Migrations** - Schema management

#### Security & Authentication
- **JWT Bearer Tokens** - Stateless authentication
- **BCrypt** - Password hashing
- **HTTPS/TLS** - Secure communication

#### Logging & Monitoring
- **Serilog** - Structured logging
- **Microsoft.Extensions.Logging** - Built-in logging
- **Health Checks** - Endpoint monitoring

#### HTTP & Resilience
- **HttpClient** - HTTP communication
- **Polly** - Retry policies and circuit breakers
- **CORS** - Cross-origin resource sharing

#### API Documentation
- **Swagger/OpenAPI 3.0** - Interactive documentation
- **Swashbuckle** - Swagger generation

#### Testing & Quality
- **xUnit** - Unit testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

### 📦 Dependencies

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

---

## 🚀 Hızlı Başlangıç

### ⚡ Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Visual Studio 2022 / VS Code / JetBrains Rider**
- **Git** - Version control

### 📝 Adım Adım Kurulum

#### 1. 📂 Projeyi Clone Edin

```bash
git clone https://github.com/yourusername/PromptOptimizer.git
cd PromptOptimizer
```

#### 2. 🔧 Dependencies Yükleyin

```bash
dotnet restore
```

#### 3. ⚙️ Konfigürasyon Ayarları

[`appsettings.json`](PromptOptimizer.API/appsettings.json) dosyasını düzenleyin:

```json
{
    "CortexApi": {
        "ApiKey": "YOUR_CORTEX_API_KEY_HERE",
        "BaseUrl": "https://api.claude.gg/v1/",
        "Timeout": 30
    },
    "Jwt": {
        "SecretKey": "YOUR_SECURE_JWT_SECRET_KEY_MIN_32_CHARS",
        "Issuer": "PromptOptimizer",
        "Audience": "PromptOptimizerUsers"
    },
    "AdminSetup": {
        "Password": "YourSecureAdminPassword123!",
        "Email": "admin@yourdomain.com"
    }
}
```

#### 4. 🗄️ Database Oluşturun

```bash
cd PromptOptimizer.API
dotnet ef database update
```

#### 5. 🚀 Uygulamayı Çalıştırın

```bash
dotnet run
```

#### 6. 🌐 Test Edin

- **API**: [`https://localhost:7089`](https://localhost:7089)
- **Swagger UI**: [`https://localhost:7089/swagger`](https://localhost:7089/swagger)

### 🔑 İlk Admin User Setup

Uygulama ilk çalıştırıldığında otomatik olarak admin user oluşturulur:

```bash
Username: admin
Password: [appsettings.json'daki AdminSetup:Password]
Email: [appsettings.json'daki AdminSetup:Email]
```

---

## 📚 API Dokümantasyonu

### 🔐 Authentication Endpoints

#### POST `/api/auth/login`
Kullanıcı girişi yapar ve JWT token döner.

**Request:**
```json
{
    "username": "admin",
    "password": "YourPassword123!"
}
```

**Response:**
```json
{
    "success": true,
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
        "id": 1,
        "username": "admin",
        "email": "admin@example.com",
        "isAdmin": true
    }
}
```

#### POST `/api/auth/register`
Yeni kullanıcı kaydı oluşturur.

**Request:**
```json
{
    "username": "newuser",
    "email": "user@example.com",
    "password": "SecurePassword123!"
}
```

#### POST `/api/auth/logout`
🔒 **Auth Required** - Kullanıcıyı çıkış yapar.

**Headers:**
```
Authorization: Bearer <token>
```

### 💬 Chat Endpoints

#### POST `/api/public/chat/send`
🌐 **No Auth Required** - Public AI chat (GPT-4o-mini, 30 req/hour)

**Request:**
```json
{
    "message": "Merhaba! Nasılsın?"
}
```

**Response:**
```json
{
    "message": "Merhaba! Ben bir AI asistanıyım ve iyiyim, teşekkür ederim.",
    "model": "gpt-4o-mini",
    "timestamp": "2024-12-01T12:00:00Z",
    "usage": {
        "promptTokens": 15,
        "completionTokens": 25,
        "totalTokens": 40
    },
    "success": true,
    "remainingRequests": 28,
    "resetTime": "2024-12-01T13:00:00Z"
}
```

**Rate Limit Error (429):**
```json
{
    "errorCode": "RATE_LIMIT_EXCEEDED",
    "message": "Public API rate limit exceeded. 0 requests remaining. Resets at 13:00 UTC.",
    "details": "Limit: 30/hour, Used: 30",
    "timestamp": "2024-12-01T12:45:00Z"
}
```

#### GET `/api/public/chat/rate-limit`
🌐 **No Auth Required** - Rate limit bilgilerini getirir.

**Response:**
```json
{
    "requestCount": 15,
    "limit": 30,
    "remainingRequests": 15,
    "resetTime": "2024-12-01T13:00:00Z",
    "isLimitExceeded": false
}
```

#### GET `/api/public/chat/info`
🌐 **No Auth Required** - Public API hakkında bilgi.

**Response:**
```json
{
    "description": "Public Chat API - No authentication required",
    "model": "gpt-4o-mini",
    "rateLimit": {
        "requests": 30,
        "period": "1 hour",
        "resetInterval": "Every hour at minute 0"
    },
    "features": {
        "authentication": false,
        "sessionMemory": false,
        "costOptimized": true,
        "maxTokens": 1000,
        "temperature": 0.7
    },
    "limits": {
        "maxMessageLength": 4000,
        "maxTokensPerResponse": 1000
    }
}
```

#### POST `/api/chat/send`
🔒 **Auth Required** - AI modeline mesaj gönderir.

**Request:**
```json
{
    "message": "Merhaba, nasılsın?",
    "model": "gpt-4o-mini",
    "sessionId": "optional-session-id",
    "temperature": 0.7,
    "maxTokens": 1000
}
```

**Response:**
```json
{
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "message": "Merhaba! Ben bir AI asistanıyım ve iyiyim, teşekkür ederim.",
    "model": "gpt-4o-mini",
    "timestamp": "2024-12-01T12:00:00Z",
    "usage": {
        "promptTokens": 15,
        "completionTokens": 25,
        "totalTokens": 40
    },
    "success": true,
    "sessionTitle": "Genel Sohbet",
    "isNewSession": false
}
```

#### POST `/api/chat/strategy`
🔒 **Auth Required** - Strateji tabanlı mesaj gönderimi.

**Request:**
```json
{
    "message": "Bu kodu optimize et: console.log('hello')",
    "strategy": "quality",
    "sessionId": "optional-session-id"
}
```

**Stratejiler:**
- `quality` - En kaliteli cevap (çoklu model)
- `speed` - Hızlı cevap
- `consensus` - Birden fazla model görüşü
- `cost_effective` - Maliyet odaklı

#### GET `/api/chat/models`
🔒 **Auth Required** - Kullanılabilir AI modellerini listeler.

**Response:**
```json
{
    "models": [
        {
            "name": "gpt-4o",
            "displayName": "GPT-4o",
            "costPer1KTokens": 0.03,
            "isRecommended": true
        },
        {
            "name": "gpt-4o-mini",
            "displayName": "GPT-4o Mini",
            "costPer1KTokens": 0.0015,
            "isRecommended": false
        }
    ]
}
```

### 📂 Session Endpoints

#### GET `/api/sessions`
🔒 **Auth Required** - Kullanıcının session'larını listeler.

#### GET `/api/sessions/{sessionId}`
🔒 **Auth Required** - Belirli session detaylarını getirir.

#### DELETE `/api/sessions/{sessionId}`
🔒 **Auth Required** - Session'ı siler.

### 🏥 System Endpoints

#### GET `/api/system/health`
Sistem sağlık durumunu kontrol eder.

**Response:**
```json
{
    "status": "Healthy",
    "timestamp": "2024-12-01T12:00:00Z",
    "version": "2.0.0"
}
```

### 🔒 Authentication Requirements

Tüm `/api/chat` ve `/api/sessions` endpoint'leri JWT authentication gerektirir:

```http
Authorization: Bearer <your-jwt-token>
```

### 📄 Swagger/OpenAPI

Tüm API dokümantasyonu interactive olarak şu adreste mevcuttur:
- **Development**: [`https://localhost:7089/swagger`](https://localhost:7089/swagger)

---

## ⚙️ Konfigürasyon

### 📋 appsettings.json Açıklaması

```json
{
    // Logging configuration
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    
    // Database connection
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=promptoptimizer.db"
    },
    
    // Cortex API settings
    "CortexApi": {
        "ApiKey": "YOUR_API_KEY",           // Cortex API anahtarı
        "BaseUrl": "https://api.claude.gg/v1/",  // Base URL
        "Timeout": 30                       // Request timeout (seconds)
    },
    
    // JWT configuration
    "Jwt": {
        "SecretKey": "MIN_32_CHAR_SECRET",  // JWT imzalama anahtarı
        "Issuer": "PromptOptimizer",        // Token yayınlayıcı
        "Audience": "PromptOptimizerUsers", // Token hedef kitlesi
        "AccessTokenExpirationMinutes": "1440", // 24 saat
        "RefreshTokenExpirationDays": "7"   // 7 gün
    },
    
    // Initial admin user
    "AdminSetup": {
        "Username": "admin",
        "Password": "SECURE_PASSWORD",
        "Email": "admin@domain.com"
    },
    
    // Structured logging with Serilog
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning"
            }
        },
        "WriteTo": [
            {
                "Name": "Console"               // Console output
            },
            {
                "Name": "File",
                "Args": {
                    "path": "logs/promptoptimizer-.txt",
                    "rollingInterval": "Day"    // Daily log files
                }
            }
        ]
    }
}
```

### 🌐 Environment Variables

Production'da sensitive bilgileri environment variable'lar ile override edebilirsiniz:

```bash
# JWT Secret
export JWT__SECRETKEY="your-production-jwt-secret-key"

# Cortex API Key
export CORTEXAPI__APIKEY="your-cortex-api-key"

# Admin Password
export ADMINSETUP__PASSWORD="your-secure-admin-password"

# Database Connection
export CONNECTIONSTRINGS__DEFAULTCONNECTION="your-production-db-connection"
```

### 🔐 Cortex API Key Setup

1. [Claude.gg](https://claude.gg) adresinden hesap oluşturun
2. API key alın
3. `appsettings.json` dosyasında `CortexApi:ApiKey` değerini güncelleyin
4. Alternatif olarak environment variable kullanın:

```bash
export CORTEXAPI__APIKEY="your-cortex-api-key"
```

---

## 🗄️ Database Schema

### Entity Relationships

```
┌─────────────────┐         ┌─────────────────────────┐
│      User       │  1:N    │   ConversationSession   │
├─────────────────┤ ────────├─────────────────────────┤
│ Id (PK)         │         │ SessionId (PK)          │
│ Username        │         │ UserId (FK)             │
│ Email           │         │ Title                   │
│ PasswordHash    │         │ CreatedAt               │
│ IsAdmin         │         │ LastActivityAt          │
│ IsActive        │         │ IsActive                │
│ CreatedAt       │         │ MessagesJson            │
│ LastLoginAt     │         │ MessageCount            │
│ SystemMessage   │         │ MaxMessages             │
└─────────────────┘         └─────────────────────────┘
```

### Message Structure (JSON)

ConversationSession içindeki MessagesJson alanı:

```json
[
    {
        "role": "user",
        "content": "Merhaba!",
        "timestamp": "2024-12-01T12:00:00Z",
        "metadata": {
            "model": "gpt-4o-mini",
            "tokens": 15
        }
    },
    {
        "role": "assistant",
        "content": "Merhaba! Size nasıl yardımcı olabilirim?",
        "timestamp": "2024-12-01T12:00:05Z",
        "metadata": {
            "model": "gpt-4o-mini",
            "tokens": 25,
            "usage": {
                "promptTokens": 15,
                "completionTokens": 25,
                "totalTokens": 40
            }
        }
    }
]
```

### Database Migration Komutları

```bash
# Yeni migration oluşturma
dotnet ef migrations add MigrationName

# Database güncelleme
dotnet ef database update

# Migration geri alma
dotnet ef database update PreviousMigrationName

# Database sıfırlama
dotnet ef database drop
dotnet ef database update
```

### Backup & Restore

```bash
# SQLite backup
cp promptoptimizer.db promptoptimizer_backup_$(date +%Y%m%d).db

# Restore
cp promptoptimizer_backup_20241201.db promptoptimizer.db
```

---

## 🔐 Güvenlik

### JWT Token Konfigürasyonu

```json
{
    "Jwt": {
        "SecretKey": "minimum-32-character-secret-key-for-production",
        "Issuer": "PromptOptimizer",
        "Audience": "PromptOptimizerUsers",
        "AccessTokenExpirationMinutes": "1440",
        "RefreshTokenExpirationDays": "7"
    }
}
```

### Password Hashing

BCrypt kullanılarak güvenli password hashing:

```csharp
// Password hashing (automatic)
var hashedPassword = passwordHashingService.HashPassword(plainPassword);

// Password verification
var isValid = passwordHashingService.VerifyPassword(plainPassword, hashedPassword);
```

### Rate Limiting

Built-in rate limiting sistemi:
- Her kullanıcı için chat endpoint'i 60 saniyede 30 request
- Memory-based cache kullanımı
- Custom rate limit kuralları [`RateLimitService`](PromptOptimizer.Application/Services/RateLimitService.cs) dosyasında

### API Key Management

Cortex API key'i güvenli bir şekilde saklanmalı:

```bash
# Development
appsettings.json içinde

# Production
Environment variable olarak:
export CORTEXAPI__APIKEY="your-secure-api-key"
```

### CORS Policy

Development için tüm origin'lere izin verilir. Production'da kısıtlanmalı:

```csharp
// Production CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder.WithOrigins("https://yourdomain.com")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

---

## 🚀 Deployment

### ⚠️ Database Persistence Uyarısı

**SORUN**: Her deploy'da SQLite database silinir ve tüm veriler (kullanıcılar, sessionlar) kaybolur.

**ÇÖZÜM**:
1. **Render.com için**: [`render.yaml`](render.yaml) dosyası ile persistent disk kullanın
2. **Diğer platformlar için**: Volume mounting yapın

```yaml
# render.yaml (Önerilen)
disk:
  name: promptoptimizer-data
  mountPath: /app/data
  sizeGB: 1
```

**Test**: Deploy sonrası admin login olup session'ların korunduğunu kontrol edin.

Detaylı bilgi: [`DEPLOYMENT.md`](DEPLOYMENT.md)

---

### 🐳 Docker Support

#### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PromptOptimizer.API/PromptOptimizer.API.csproj", "PromptOptimizer.API/"]
COPY ["PromptOptimizer.Application/PromptOptimizer.Application.csproj", "PromptOptimizer.Application/"]
COPY ["PromptOptimizer.Core/PromptOptimizer.Core.csproj", "PromptOptimizer.Core/"]
COPY ["PromptOptimizer.Infrastructure/PromptOptimizer.Infrastructure.csproj", "PromptOptimizer.Infrastructure/"]
RUN dotnet restore "PromptOptimizer.API/PromptOptimizer.API.csproj"
COPY . .
WORKDIR "/src/PromptOptimizer.API"
RUN dotnet build "PromptOptimizer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PromptOptimizer.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PromptOptimizer.API.dll"]
```

#### docker-compose.yml

```yaml
version: '3.8'
services:
  promptoptimizer-api:
    build: .
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - JWT__SECRETKEY=${JWT_SECRET_KEY}
      - CORTEXAPI__APIKEY=${CORTEX_API_KEY}
      - ADMINSETUP__PASSWORD=${ADMIN_PASSWORD}
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    restart: unless-stopped
```

#### Build & Run Commands

```bash
# Docker build
docker build -t promptoptimizer .

# Docker run
docker run -d \
  --name promptoptimizer \
  -p 8080:80 \
  -e JWT__SECRETKEY="your-secure-jwt-key" \
  -e CORTEXAPI__APIKEY="your-cortex-api-key" \
  -v $(pwd)/data:/app/data \
  promptoptimizer

# Docker compose
docker-compose up -d
```

### ☁️ Azure Deployment

#### Azure App Service

```bash
# Azure CLI ile deployment
az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name promptoptimizer --runtime "DOTNETCORE|8.0"

# App settings
az webapp config appsettings set --resource-group myResourceGroup --name promptoptimizer --settings \
  JWT__SECRETKEY="your-secure-jwt-key" \
  CORTEXAPI__APIKEY="your-cortex-api-key" \
  ADMINSETUP__PASSWORD="your-admin-password"

# Deploy
dotnet publish -c Release
az webapp deployment source config-zip --resource-group myResourceGroup --name promptoptimizer --src publish.zip
```

#### Azure Container Instances

```bash
# Container deployment
az container create \
  --resource-group myResourceGroup \
  --name promptoptimizer \
  --image promptoptimizer:latest \
  --ports 80 \
  --environment-variables \
    JWT__SECRETKEY="your-secure-jwt-key" \
    CORTEXAPI__APIKEY="your-cortex-api-key"
```

### 🌐 AWS Deployment

#### Elastic Beanstalk

```bash
# EB CLI ile deployment
eb init
eb create promptoptimizer-prod
eb setenv JWT__SECRETKEY="your-secure-jwt-key" CORTEXAPI__APIKEY="your-cortex-api-key"
eb deploy
```

#### ECS Deployment

Task definition example:

```json
{
    "family": "promptoptimizer",
    "cpu": "256",
    "memory": "512",
    "networkMode": "awsvpc",
    "containerDefinitions": [
        {
            "name": "promptoptimizer",
            "image": "promptoptimizer:latest",
            "portMappings": [
                {
                    "containerPort": 80,
                    "protocol": "tcp"
                }
            ],
            "environment": [
                {
                    "name": "JWT__SECRETKEY",
                    "value": "your-secure-jwt-key"
                },
                {
                    "name": "CORTEXAPI__APIKEY",
                    "value": "your-cortex-api-key"
                }
            ]
        }
    ]
}
```

### 🔄 Environment-Specific Configurations

#### appsettings.Production.json

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Warning",
            "PromptOptimizer": "Information"
        }
    },
    "Serilog": {
        "WriteTo": [
            {
                "Name": "File",
                "Args": {
                    "path": "/var/log/promptoptimizer/promptoptimizer-.txt",
                    "rollingInterval": "Day",
                    "retainedFileCountLimit": 30
                }
            }
        ]
    }
}
```

### 🏥 Health Checks

Built-in health check endpoints:

```bash
# Basic health check
GET /api/system/health

# Detailed health check
GET /health

# Response example
{
    "status": "Healthy",
    "totalDuration": "00:00:00.0012345",
    "entries": {
        "database": {
            "status": "Healthy",
            "duration": "00:00:00.0001234"
        },
        "cortex_api": {
            "status": "Healthy",
            "duration": "00:00:00.0005678"
        }
    }
}
```

---

## 👨‍💻 Geliştirici Rehberi

### 📁 Project Structure

```
PromptOptimizer/
├── PromptOptimizer.API/                 # Web API Layer
│   ├── Controllers/                     # API Controllers
│   │   ├── AuthController.cs           # Authentication endpoints
│   │   ├── ChatController.cs           # Chat endpoints (Auth required)
│   │   ├── PublicChatController.cs     # Public chat (No auth)
│   │   ├── SessionsController.cs       # Session management
│   │   └── SystemController.cs         # System endpoints
│   ├── Properties/
│   │   └── launchSettings.json         # Development settings
│   ├── Resource/
│   │   └── ErrorResponse.cs            # Error response models
│   ├── Program.cs                      # Application entry point
│   └── appsettings.json               # Configuration
├── PromptOptimizer.Core/               # Domain Layer
│   ├── Configuration/                  # Configuration models
│   ├── Constants/                      # Application constants
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Entities/                      # Domain entities
│   ├── Extensions/                    # Extension methods
│   └── Interfaces/                    # Service interfaces
├── PromptOptimizer.Application/        # Application Layer
│   └── Services/                      # Business logic services
│       ├── AuthService.cs             # Authentication service
│       ├── ChatService.cs             # Chat service
│       ├── RateLimitService.cs        # Rate limiting
│       ├── SessionTitleGenerator.cs   # Session title generation
│       └── ValidationService.cs       # Input validation
├── PromptOptimizer.Infrastructure/     # Infrastructure Layer
│   ├── Clients/                       # External API clients
│   │   └── CortexApiClient.cs         # Cortex API integration
│   ├── Data/                          # Database context
│   │   └── AppDbContext.cs            # EF Core context
│   ├── Migrations/                    # EF Core migrations
│   └── Services/                      # Infrastructure services
│       ├── DatabaseSessionService.cs  # Session persistence
│       ├── JwtTokenService.cs         # JWT token handling
│       └── PasswordHashingService.cs  # Password hashing
└── PromptOptimizer.Tests/             # Test Project
    └── UnitTest1.cs                   # Unit tests
```

### 🔧 Yeni Feature Ekleme

#### 1. New Entity Ekleme

```csharp
// Core/Entities/YourEntity.cs
public class YourEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### 2. DTO Oluşturma

```csharp
// Core/DTOs/YourModels.cs
public class YourRequest
{
    public string Name { get; set; } = string.Empty;
}

public class YourResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

#### 3. Interface Tanımlama

```csharp
// Core/Interfaces/IYourService.cs
public interface IYourService
{
    Task<YourResponse> CreateAsync(YourRequest request);
    Task<List<YourResponse>> GetAllAsync();
    Task<YourResponse?> GetByIdAsync(int id);
    Task<bool> DeleteAsync(int id);
}
```

#### 4. Service Implementation

```csharp
// Application/Services/YourService.cs
public class YourService : IYourService
{
    private readonly AppDbContext _context;
    private readonly ILogger<YourService> _logger;

    public YourService(AppDbContext context, ILogger<YourService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<YourResponse> CreateAsync(YourRequest request)
    {
        var entity = new YourEntity
        {
            Name = request.Name
        };

        _context.YourEntities.Add(entity);
        await _context.SaveChangesAsync();

        return new YourResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            CreatedAt = entity.CreatedAt
        };
    }

    // Implement other methods...
}
```

#### 5. Controller Ekleme

```csharp
// API/Controllers/YourController.cs
[Route("api/[controller]")]
public class YourController : BaseApiController
{
    private readonly IYourService _yourService;

    public YourController(IYourService yourService)
    {
        _yourService = yourService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] YourRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return UnauthorizedResult();

        try
        {
            var response = await _yourService.CreateAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return ServiceError(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _yourService.GetAllAsync();
        return Ok(entities);
    }
}
```

#### 6. DI Registration

```csharp
// Program.cs
builder.Services.AddScoped<IYourService, YourService>();
```

#### 7. Migration

```bash
dotnet ef migrations add AddYourEntity
dotnet ef database update
```

### 🧪 Testing Stratejileri

#### Unit Test Example

```csharp
[Fact]
public async Task ChatService_SendMessage_ShouldReturnValidResponse()
{
    // Arrange
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    using var context = new AppDbContext(options);
    var mockCortexClient = new Mock<ICortexApiClient>();
    var mockLogger = new Mock<ILogger<ChatService>>();

    mockCortexClient.Setup(x => x.CreateChatCompletionAsync(It.IsAny<ChatCompletionRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ChatCompletionResponse
        {
            Choices = new List<Choice>
            {
                new Choice { Message = new OpenAI.Message { Content = "Test response" } }
            }
        });

    var service = new ChatService(context, mockCortexClient.Object, mockLogger.Object);

    // Act
    var result = await service.SendMessageAsync("Test message", "gpt-4o-mini", null, 0.7, 1000, 1, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test response", result.Message);
    Assert.True(result.Success);
}
```

#### Integration Test Example

```csharp
public class ChatControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ChatControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SendMessage_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new ChatRequest
        {
            Message = "Hello",
            Model = "gpt-4o-mini"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/chat/send", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var chatResponse = JsonSerializer.Deserialize<ChatResponse>(content);
        Assert.NotNull(chatResponse);
    }
}
```

### 📝 Code Contribution Guidelines

#### 1. Branch Strategy

```bash
# Feature branch
git checkout -b feature/your-feature-name

# Bug fix branch
git checkout -b bugfix/issue-description

# Release branch
git checkout -b release/v2.1.0
```

#### 2. Commit Messages

```bash
# Format: type(scope): description
git commit -m "feat(chat): add support for streaming responses"
git commit -m "fix(auth): resolve JWT token expiration issue"
git commit -m "docs(readme): update API documentation"
```

#### 3. Code Style

- **C# Naming Conventions** kullanın
- **XML documentation** ekleyin
- **Unit tests** yazın
- **Clean Code** principles uygulayın

#### 4. Pull Request Process

1. Feature branch oluşturun
2. Tests yazın ve geçirin
3. Documentation güncelleyin
4. Pull request açın
5. Code review bekleyin
6. Merge yapın

---

## 🐛 Troubleshooting

### 📋 Common Issues

#### 1. JWT Token Issues

**Problem:** "Unauthorized" error

**Çözüm:**
```bash
# Token kontrolü
curl -H "Authorization: Bearer YOUR_TOKEN" https://localhost:7089/api/chat/models

# Token decode (jwt.io kullanın)
# Expiration time kontrol edin
```

#### 2. Database Connection Issues

**Problem:** "Database connection failed"

**Çözüm:**
```bash
# Connection string kontrol
cat appsettings.json | grep ConnectionStrings

# Database file permissions
ls -la promptoptimizer.db

# Manual migration
dotnet ef database update --verbose
```

#### 3. Cortex API Issues

**Problem:** "API Error 401: Unauthorized"

**Çözüm:**
```bash
# API key kontrol
echo $CORTEXAPI__APIKEY

# Manual test
curl -H "Authorization: Bearer YOUR_API_KEY" https://api.claude.gg/v1/models
```

#### 4. Memory/Performance Issues

**Problem:** High memory usage

**Çözüm:**
```bash
# Memory profiling
dotnet-dump collect -p PID

# Log analysis
tail -f logs/promptoptimizer-$(date +%Y%m%d).txt | grep "Memory"

# Rate limiting check
grep "Rate limit" logs/promptoptimizer-*.txt
```

### 📊 Log Dosyalarını İnceleme

#### Log Locations

```bash
# Development
./logs/promptoptimizer-YYYYMMDD.txt

# Production (Docker)
/app/logs/promptoptimizer-YYYYMMDD.txt

# Production (Linux)
/var/log/promptoptimizer/promptoptimizer-YYYYMMDD.txt
```

#### Log Analysis Commands

```bash
# Error logs
grep "ERROR" logs/promptoptimizer-*.txt

# Performance logs
grep -E "(took|ms|elapsed)" logs/promptoptimizer-*.txt

# API call logs
grep "CortexAPI" logs/promptoptimizer-*.txt

# Authentication logs
grep -E "(login|logout|token)" logs/promptoptimizer-*.txt

# Real-time monitoring
tail -f logs/promptoptimizer-$(date +%Y%m%d).txt | grep ERROR
```

#### Log Level Configuration

```json
{
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning",
                "PromptOptimizer": "Debug"
            }
        }
    }
}
```

### ⚡ Performance Tuning

#### 1. Database Optimization

```csharp
// Add indexes to frequent queries
[Index(nameof(UserId))]
[Index(nameof(CreatedAt))]
public class ConversationSession
{
    // Entity properties
}

// Use async operations
var sessions = await _context.ConversationSessions
    .Where(s => s.UserId == userId)
    .OrderByDescending(s => s.LastActivityAt)
    .Take(50)
    .ToListAsync();
```

#### 2. HTTP Client Optimization

```csharp
// Connection pooling
builder.Services.AddHttpClient<ICortexApiClient, CortexApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    MaxConnectionsPerServer = 10,
});
```

#### 3. Memory Management

```csharp
// Use memory cache for frequently accessed data
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.2;
});

// Implement proper disposal
public class YourService : IDisposable
{
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Dispose managed resources
        }
        _disposed = true;
    }
}
```

#### 4. Response Compression

```csharp
// Add to Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

app.UseResponseCompression();
```

### 🔍 Monitoring & Metrics

#### Health Check Implementation

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddUrlGroup(new Uri("https://api.claude.gg/v1/models"), "cortex-api");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

#### Custom Metrics

```csharp
// Add metrics collection
var meterName = "PromptOptimizer.Metrics";
var meter = new Meter(meterName);
var requestCounter = meter.CreateCounter<long>("api_requests_total");
var responseTimeHistogram = meter.CreateHistogram<double>("api_response_time_seconds");

// Usage in controllers
requestCounter.Add(1, new("endpoint", "chat"), new("method", "POST"));
responseTimeHistogram.Record(elapsed.TotalSeconds);
```

---

## 📄 Lisans ve Katkı

### 📜 License

Bu proje [MIT License](LICENSE) altında lisanslanmıştır. Detaylar için [`LICENSE`](LICENSE) dosyasına bakınız.

**Özet:**
- ✅ Ticari kullanım allowed
- ✅ Değiştirme allowed
- ✅ Dağıtım allowed
- ✅ Özel kullanım allowed
- ❌ Sorumluluk yok
- ❌ Garanti yok

### 🤝 Contribution Guidelines

#### Nasıl Katkıda Bulunabilirsiniz?

1. **🐛 Bug Reports**
   - Issue açmadan önce mevcut issue'ları kontrol edin
   - Detaylı reproduksiyon adımları ekleyin
   - Environment bilgilerini paylaşın

2. **✨ Feature Requests**
   - Use case'i açıklayın
   - Önerilen implementasyonu tartışın
   - Breaking changes olup olmadığını belirtin

3. **💻 Code Contributions**
   - Fork yapın ve feature branch oluşturun
   - Tests yazın
   - Documentation güncelleyin
   - Pull request açın

#### Code Review Process

1. **Automated Checks**
   - Build success
   - Test coverage
   - Code quality checks

2. **Manual Review**
   - Code style consistency
   - Architecture compliance
   - Security considerations
   - Performance impact

3. **Approval & Merge**
   - En az 1 approver gerekli
   - Squash and merge preferred
   - Release notes güncellenir

### 📞 Contact Information

- **Project Maintainer**: [Your Name](mailto:your.email@domain.com)
- **GitHub Issues**: [Issues Page](https://github.com/yourusername/PromptOptimizer/issues)
- **Discord**: [Community Discord](https://discord.gg/your-invite)
- **Documentation**: [Wiki Pages](https://github.com/yourusername/PromptOptimizer/wiki)

### 🙏 Acknowledgments

- **.NET Team** - Framework ve tooling
- **Serilog Contributors** - Structured logging
- **Entity Framework Team** - ORM framework
- **Polly Contributors** - Resilience patterns
- **Community Contributors** - Bug reports ve feature suggestions

---

## 📚 Additional Resources

### 🔗 Useful Links

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Guide](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Serilog Documentation](https://serilog.net/)
- [JWT.io](https://jwt.io/) - JWT token decoder
- [OpenAPI Specification](https://swagger.io/specification/)

### 📖 Learning Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [RESTful API Design](https://restfulapi.net/)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

### 🛠️ Development Tools

- **IDE**: Visual Studio 2022, VS Code, JetBrains Rider
- **API Testing**: Postman, Insomnia, REST Client
- **Database**: DB Browser for SQLite, Azure Data Studio
- **Monitoring**: Application Insights, Seq, Grafana
- **Performance**: dotMemory, PerfView, BenchmarkDotNet

---

**PromptOptimizer** - AI Prompt Optimization made simple and scalable! 🚀

*Last updated: June 2025*