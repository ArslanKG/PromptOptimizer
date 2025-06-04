# 📁 MCP BAŞLANGIÇ YÖNERGESİ

> **Bu dosya PromptOptimizer projesinin yapısını, işlevlerini ve geliştirme kurallarını detaylı şekilde açıklar. Tüm AI destekli geliştirme ve bakım süreçlerinde referans alınmalıdır. Her yeni task öncesi bu dosya okunmalı ve bağlam yüklenmelidir.**

---

## ◇ 1. PROJE ÖZETİ

- **Proje Adı:** PromptOptimizer - AI Prompt Optimization Backend API
- **Amacı:** Geliştiricilerin kendi projelerine entegre edebilecekleri kapsamlı bir AI prompt optimizasyon backend API'si. Kullanıcı promptlarını optimize ederek çoklu AI model desteği ile en iyi yanıtları üretir.
- **Ana Teknolojiler:** .NET 8 (C# 12), ASP.NET Core Web API, Entity Framework Core 8.0, JWT Bearer Authentication, Cortex API Client, SQLite Database
- **Uygulama Türü:** RESTful API Backend Service (Production-ready)
- **Mimari:** Clean Architecture (Presentation, Application, Core, Infrastructure layers)
- **Başlangıç Tarihi:** 2025
- **Lisans:** MIT License

---

## ◇ 2. ANA ÖZELLİKLER & FONKSİYONLAR

### 🤖 AI & Model Desteği
1. **Multi-Model Support:** GPT-4o, GPT-4o-mini, DeepSeek-V3, Grok-2-Vision-1212, Claude-3.5-Haiku
2. **Smart Strategy System:** Quality, Speed, Consensus, Cost Effective stratejileri
3. **Optimization Types:** Clarity, Technical, Creative, Analytical prompt optimizasyonu
4. **Model Orchestration:** Farklı AI modelleri arasında akıllı seçim ve yönlendirme

### 🔐 Güvenlik & Kimlik Doğrulama
5. **JWT Bearer Authentication:** Stateless, secure token-based authentication
6. **User Management:** Admin/User role sistemi
7. **Rate Limiting:** API abuse koruması ve kullanıcı bazlı kota yönetimi
8. **Password Hashing:** BCrypt ile güvenli şifre saklama

### 💬 Session & Context Management
9. **Session-based Conversation:** Kullanıcıya özel oturum yönetimi
10. **Context Preservation:** Konuşma geçmişi ve bağlam korunması
11. **Session Title Generation:** AI tabanlı otomatik başlık oluşturma
12. **Message History:** Tüm mesajların JSON formatında kalıcı saklanması

### 🛠️ Teknik Özellikler
13. **Clean Architecture:** Modüler ve sürdürülebilir kod yapısı
14. **Entity Framework Core:** Database operations ve migrations
15. **Serilog Logging:** Structured logging ve file output
16. **Polly Resilience:** HTTP retry policies ve circuit breakers
17. **Health Checks:** Sistem durumu monitoring
18. **Swagger/OpenAPI:** İnteraktif API dokümantasyonu

---

## ◇ 3. TEKNOLOJİ YIĞINI (Tech Stack)

### Backend Framework
- **.NET 8.0** - Latest LTS framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0** - ORM ve database operations

### Database & Storage
- **SQLite** - Lightweight, file-based database
- **Entity Framework Migrations** - Schema management

### Security & Authentication
- **JWT Bearer Tokens** - Stateless authentication
- **BCrypt** - Password hashing algorithm
- **HTTPS/TLS** - Secure communication

### External Integrations
- **Cortex API** - Multi-model AI service client
- **ICortexApiClient** - Custom HTTP client implementation

### Logging & Monitoring
- **Serilog** - Structured logging framework
- **Microsoft.Extensions.Logging** - Built-in logging interface
- **Health Checks** - Application monitoring

### HTTP & Resilience
- **HttpClient** - HTTP communication
- **Polly** - Retry policies ve circuit breakers
- **CORS** - Cross-origin resource sharing

### Development & Testing
- **Swagger/OpenAPI 3.0** - API documentation
- **xUnit** - Unit testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

---

## ◇ 4. KOD YAPISI (Clean Architecture)

```
PromptOptimizer/
├── PromptOptimizer.API/                 # 🎯 Presentation Layer
│   ├── Controllers/                     # API Controllers
│   │   ├── AuthController.cs           # Authentication endpoints
│   │   ├── ChatController.cs           # Chat ve strategy endpoints
│   │   ├── SessionsController.cs       # Session management
│   │   ├── SystemController.cs         # Health checks
│   │   └── BaseApiController.cs        # Base controller functionality
│   ├── Properties/launchSettings.json  # Development settings
│   ├── Resource/ErrorResponse.cs       # Error response models
│   ├── Program.cs                      # Application entry point & DI setup
│   └── appsettings.json               # Configuration settings
├── PromptOptimizer.Core/               # 🏛️ Domain Layer
│   ├── Configuration/                  # Configuration models
│   │   ├── JwtSettings.cs             # JWT configuration
│   │   └── StrategyConfiguration.cs    # Strategy definitions
│   ├── Constants/                      # Application constants
│   │   ├── ErrorMessages.cs           # Error message constants
│   │   ├── LogMessages.cs             # Log message templates
│   │   └── PromptTemplates.cs         # Prompt optimization templates
│   ├── DTOs/                          # Data Transfer Objects
│   │   ├── AuthModels.cs              # Authentication DTOs
│   │   ├── ChatModels.cs              # Chat request/response DTOs
│   │   ├── CortexApiModels.cs         # External API models
│   │   └── SessionModels.cs           # Session management DTOs
│   ├── Entities/                      # Domain entities
│   │   ├── User.cs                    # User entity
│   │   ├── ConversationSession.cs     # Session entity
│   │   ├── Message.cs                 # Message entity
│   │   └── ModelInfo.cs               # AI model information
│   ├── Extensions/                    # Extension methods
│   │   └── ValidationExtensions.cs    # Validation helpers
│   └── Interfaces/                    # Service contracts
│       ├── IAuthService.cs            # Authentication interface
│       ├── IChatService.cs            # Chat service interface
│       ├── ICortexApiClient.cs        # External API interface
│       ├── ISessionService.cs         # Session management interface
│       └── [Other service interfaces]
├── PromptOptimizer.Application/        # 🔧 Application Layer
│   └── Services/                      # Business logic services
│       ├── AuthService.cs             # Authentication business logic
│       ├── ChatService.cs             # Chat ve prompt optimization
│       ├── RateLimitService.cs        # Rate limiting logic
│       ├── SessionTitleGenerator.cs   # AI-based title generation
│       └── ValidationService.cs       # Input validation logic
├── PromptOptimizer.Infrastructure/     # 🏗️ Infrastructure Layer
│   ├── Clients/                       # External service clients
│   │   └── CortexApiClient.cs         # Cortex API HTTP client
│   ├── Data/                          # Database context
│   │   └── AppDbContext.cs            # EF Core context
│   ├── Migrations/                    # EF Core migrations
│   │   └── [Migration files]          # Database schema versions
│   └── Services/                      # Infrastructure services
│       ├── DatabaseSessionService.cs  # Session persistence
│       ├── JwtTokenService.cs         # JWT token operations
│       └── PasswordHashingService.cs  # Password hashing
└── PromptOptimizer.Tests/             # 🧪 Test Project
    └── UnitTest1.cs                   # Unit tests
```

---

## ◇ 5. VERİTABANI YAPISI

### Entity Relationships
```
User (1) ────────── (N) ConversationSession
│                       │
├─ Id (PK)             ├─ SessionId (PK)
├─ Username            ├─ UserId (FK)
├─ Email               ├─ Title
├─ PasswordHash        ├─ CreatedAt
├─ IsAdmin             ├─ LastActivityAt
├─ IsActive            ├─ IsActive
├─ CreatedAt           ├─ MessagesJson
├─ LastLoginAt         ├─ MessageCount
└─ SystemMessage       └─ MaxMessages
```

### Message Structure (JSON)
ConversationSession.MessagesJson field contains:
```json
[
    {
        "role": "user",
        "content": "User message content",
        "timestamp": "2024-12-01T12:00:00Z",
        "metadata": {
            "model": "gpt-4o-mini",
            "tokens": 15
        }
    },
    {
        "role": "assistant",
        "content": "AI response content",
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

---

## ◇ 6. API ENDPOINTS

### 🔐 Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration  
- `POST /api/auth/logout` - User logout (Auth required)

### 💬 Chat
- `POST /api/chat/send` - Send message to AI model (Auth required)
- `POST /api/chat/strategy` - Send with strategy (Auth required)
- `GET /api/chat/models` - Get available models (Auth required)

### 📂 Sessions
- `GET /api/sessions` - List user sessions (Auth required)
- `GET /api/sessions/{id}` - Get session details (Auth required)
- `DELETE /api/sessions/{id}` - Delete session (Auth required)

### 🏥 System
- `GET /api/system/health` - Health check
- `GET /health` - Detailed health check

---

## ◇ 7. GÖREV LİSTESİ / ROADMAP

### ✅ Tamamlandı:
- ✅ Clean Architecture implementation
- ✅ JWT Bearer Authentication sistemi
- ✅ Multi-model AI support (Cortex API integration)
- ✅ Smart strategy system (Quality, Speed, Consensus, Cost Effective)
- ✅ Session-based conversation management
- ✅ Database design ve Entity Framework setup
- ✅ Rate limiting implementation
- ✅ Comprehensive logging (Serilog)
- ✅ HTTP resilience (Polly)
- ✅ Health checks
- ✅ Swagger/OpenAPI documentation
- ✅ Password hashing (BCrypt)
- ✅ Error handling ve validation
- ✅ Docker support preparation
- ✅ Configuration management

### 🔜 Yapılacaklar:
- 🔄 Unit test coverage improvement
- 🔄 Integration test suite
- 🔄 Performance monitoring
- 🔄 Caching strategies
- 🔄 Background job processing
- 🔄 Admin dashboard
- 🔄 Usage analytics
- 🔄 Backup/restore procedures

### ❓ Araştırma Konuları:
- 🔍 Advanced prompt engineering techniques
- 🔍 Model performance comparison metrics
- 🔍 Cost optimization algorithms
- 🔍 Real-time streaming responses
- 🔍 Multi-tenant architecture
- 🔍 AI model fine-tuning integration

---

## ◇ 8. MCP DESTEKLİ GELIŞTIRME KURALLARI

### 💻 Kod Yazım Kuralları:
- **Dil:** Fonksiyon ve değişken isimleri İngilizce, açıklamalar Türkçe
- **Naming Convention:** PascalCase (classes, methods), camelCase (variables, parameters)
- **Documentation:** Her public method için XML documentation comments
- **Architecture:** Clean Architecture principles (Dependency Inversion, SOLID)
- **Error Handling:** Try-catch blocks with proper logging
- **Async/Await:** Async operations için await kullanımı
- **Validation:** Input validation her endpoint'te mandatory

### 🏗️ Mimari Kuralları:
- **Core Layer:** External dependencies yok, sadece interfaces
- **Application Layer:** Business logic, Core'a dependency
- **Infrastructure Layer:** External services, database implementations
- **Presentation Layer:** Controllers, minimal business logic

### 📝 Commit ve Branch Kuralları:
```bash
# Branch naming
feature/add-streaming-support
bugfix/fix-jwt-expiration
hotfix/critical-security-patch

# Commit format
feat(chat): add streaming response support
fix(auth): resolve JWT token expiration issue
docs(readme): update API documentation
```

### 🧪 Test Kuralları:
- Unit tests için `[ClassName]Tests` naming
- Integration tests için `[Controller]IntegrationTests`
- Test coverage minimum %70
- Mock objects external dependencies için

### 📊 Logging Kuralları:
```csharp
// Information level
_logger.LogInformation("User {UserId} started new session {SessionId}", userId, sessionId);

// Warning level
_logger.LogWarning("Rate limit approaching for user {UserId}: {RequestCount}/{Limit}", userId, count, limit);

// Error level
_logger.LogError(ex, "Failed to process chat request for user {UserId}", userId);
```

---

## ◇ 9. CONFIGURATION & SECRETS

### appsettings.json Structure:
```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=promptoptimizer.db"
    },
    "CortexApi": {
        "ApiKey": "YOUR_API_KEY",
        "BaseUrl": "https://api.claude.gg/v1/",
        "Timeout": 30
    },
    "Jwt": {
        "SecretKey": "SECURE_SECRET_KEY",
        "Issuer": "PromptOptimizer",
        "Audience": "PromptOptimizerUsers",
        "AccessTokenExpirationMinutes": "1440"
    },
    "AdminSetup": {
        "Username": "admin",
        "Password": "SECURE_PASSWORD",
        "Email": "admin@domain.com"
    }
}
```

### Environment Variables (Production):
```bash
JWT__SECRETKEY="production-jwt-secret"
CORTEXAPI__APIKEY="production-cortex-key"
ADMINSETUP__PASSWORD="secure-admin-password"
```

---

## ◇ 10. DEPLOYMENT & DEVOPS

### Docker Commands:
```bash
# Build image
docker build -t promptoptimizer .

# Run container
docker run -d --name promptoptimizer \
  -p 8080:80 \
  -e JWT__SECRETKEY="secure-key" \
  -e CORTEXAPI__APIKEY="api-key" \
  promptoptimizer
```

### Database Commands:
```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop
```

---

## ◇ 11. BELLEK / HAFIZA NOTLARI

**🧠 Önemli Context Bilgileri:**
- **Communication:** Kullanıcı Türkçe iletişim kurar, kod İngilizce yazılır
- **Focus:** AI prompt optimization ve multi-model orchestration
- **Architecture:** Clean Architecture, dependency injection heavy usage
- **Performance:** Rate limiting, caching, async operations kritik
- **Security:** JWT tokens, password hashing, input validation şart
- **Database:** EF Core, SQLite, JSON message storage pattern
- **External APIs:** Cortex API client, HTTP resilience policies
- **Logging:** Structured logging, correlation IDs, performance metrics

**📋 Sık Kullanılan Patterns:**
- Repository pattern (DatabaseSessionService)
- Strategy pattern (ChatService strategies)
- Factory pattern (Model selection)
- Options pattern (Configuration)
- Mediator pattern (Controller → Service)

**⚠️ Kritik Noktalar:**
- API keys güvenli saklanmalı (Environment variables)
- Rate limiting bypass edilmemeli
- Session data JSON serialization/deserialization dikkatli
- HTTP client dispose patterns
- Database connection lifecycle management

---

## ◇ 12. KULLANIM TALİMATI

Bu dosyayı proje kök dizinine `mcp_context.md` adıyla ekleyin ve her yeni task öncesi mutlaka okuyun:

```bash
# MCP context load command
mcp load-context ./mcp_context.md
```

**Her task başlangıcında:**
1. Bu dosyayı tamamen okuyun
2. Güncel proje durumunu değerlendirin  
3. İlgili kod bölümlerini inceleyin
4. Clean Architecture principles'a uygun hareket edin
5. Test-driven development approach benimseyin

---

*Last updated: June 2025*
*Project Status: Production Ready*
*Documentation Version: v2.0*