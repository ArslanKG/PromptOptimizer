# 🧠 Multi-Model Prompt Optimizer

## Tanım

Multi-Model Prompt Optimizer, kullanıcıdan alınan bir prompt'u çeşitli yapay zeka modelleri aracılığıyla önce optimize eden, ardından en iyi cevabı oluşturmak için farklı stratejilerle işleyen bir yapay zeka zinciri projesidir.

Bu sistem:
- Prompt'u daha spesifik ve etkili hale getirir
- Farklı modellerle cevaplar alır
- Gelişmiş ve detaylandırılmış final yanıt oluşturur

## 🚀 Özellikler

- ✅ Prompt iyileştirme (optimizasyon)
- ✅ AI cevap geliştirme (enhancement)
- ✅ Çoklu model desteği (gpt-4o, gemini, deepseek vs.)
- ✅ Strateji seçimi: `quality`, `speed`, `consensus`, `cost_effective`
- ✅ .NET 8 backend (ASP.NET Core Web API)
- ✅ Swagger/OpenAPI arayüzü
- ✅ Polly ile hata dayanıklılığı
- ✅ Serilog ile loglama
- ✅ Health check endpoint'i
- ✅ Frontend için React uyumlu JSON API

## 🧱 Proje Yapısı

```bash
PromptOptimizer/
├── src/
│   ├── PromptOptimizer.API/          # ASP.NET Core API
│   ├── PromptOptimizer.Core/         # Entity, DTO, Interface
│   ├── PromptOptimizer.Application/  # Servisler ve orchestrator
│   └── PromptOptimizer.Infrastructure/ # CortexAPI istemcisi
└── tests/
    └── PromptOptimizer.Tests/        # Unit test'ler
