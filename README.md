# 🧠 PromptOptimizer - Multi-Model AI Prompt Optimization API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📋 Açıklama

PromptOptimizer, kullanıcıdan alınan promptları optimize eden ve birden fazla AI modelini kullanarak en iyi yanıtı üreten gelişmiş bir API'dir. Clean Architecture prensiplerine uygun olarak .NET 8 ile geliştirilmiştir.

## 🚀 Özellikler

### Temel Özellikler
- ✅ **Multi-Model Desteği**: GPT-4o, Deepseek, Grok ve daha fazlası
- ✅ **Prompt Optimizasyonu**: 4 farklı optimizasyon tipi (Clarity, Technical, Creative, Analytical)
- ✅ **Strateji Seçenekleri**: Quality, Speed, Consensus, Cost Effective
- ✅ **Session Management**: Konuşma geçmişi ve context yönetimi
- ✅ **JWT Authentication**: Güvenli API erişimi
- ✅ **Swagger/OpenAPI**: İnteraktif API dokümantasyonu

### Teknik Özellikler
- 🏗️ Clean Architecture
- 🔒 JWT Bearer Authentication
- 📝 Serilog ile yapılandırılmış loglama
- 🔄 Polly ile HTTP resilience
- 💾 Entity Framework Core (SQLite)
- 🧪 Unit test altyapısı


## 🧱 Proje Yapısı

```bash
PromptOptimizer/
├── src/
│ ├── PromptOptimizer.API/ # Web API katmanı
│ ├── PromptOptimizer.Core/ # Domain entities, interfaces, DTOs
│ ├── PromptOptimizer.Application/ # Business logic, services
│ └── PromptOptimizer.Infrastructure/# External services, data access
└── tests/
└── PromptOptimizer.Tests/ # Unit tests



## 🛠️ Kurulum

### Gereksinimler
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code / Rider
- SQLite (otomatik kurulur)

### Adım Adım Kurulum

1. **Projeyi klonlayın**
   ```bash
   git clone https://github.com/yourusername/PromptOptimizer.git
   cd PromptOptimizer