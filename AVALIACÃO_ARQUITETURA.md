# RifaTech — Avaliação de Arquitetura e Código

**Data:** 08/02/2026  
**Versão do .NET:** 8.0  
**Banco de Dados:** MySQL (Pomelo EF Core)  
**Autor da Avaliação:** GitHub Copilot

---

## 1. Visão Geral do Projeto

O **RifaTech** é uma plataforma de rifas online que permite criar, gerenciar e vender rifas com integração de pagamento via **Mercado Pago (PIX)**. O sistema possui:

- **RifaTech.API** — API REST (Minimal APIs) com autenticação JWT, integração MercadoPago, notificações e painel administrativo.
- **RifaTech.DTOs** — Projeto separado com DTOs, contratos (interfaces de serviço) e respostas.
- **RifaTech.UI** — Frontend Blazor (Server + WebAssembly híbrido com FluentUI).

### Funcionalidades Principais
| Módulo | Descrição |
|---|---|
| Rifas | CRUD de rifas com paginação, destaque e soft-delete |
| Tickets | Compra de tickets com números aleatórios |
| Pagamentos | Integração PIX via Mercado Pago, verificação automática de status |
| Clientes | Cadastro de clientes com CPF, email e telefone |
| Sorteios (Draw) | Gerenciamento de sorteios e números extras |
| Compra Rápida | Fluxo de compra sem autenticação |
| Notificações | Email + WhatsApp (multi-canal) |
| Admin Dashboard | Estatísticas, relatórios de vendas, tickets recentes |
| Webhooks | Recebe notificações do Mercado Pago |

---

## 2. Diagrama da Arquitetura Atual

```
┌─────────────────────────────────────────────────────────────┐
│                      RifaTech.UI                             │
│        (Blazor Server + WebAssembly + FluentUI)              │
└──────────────────────┬──────────────────────────────────────┘
                       │ HTTP/REST
┌──────────────────────▼──────────────────────────────────────┐
│                    RifaTech.API                               │
│  ┌──────────┐  ┌────────────┐  ┌─────────────┐              │
│  │ Endpoints │  │ Middleware │  │ Background  │              │
│  │ (Minimal) │  │ (Auth,CORS)│  │  Services   │              │
│  └─────┬─────┘  └────────────┘  └──────┬──────┘              │
│        │                                │                     │
│  ┌─────▼──────────────────────────────────────────────┐      │
│  │        "Repositories" (na verdade Services)         │      │
│  │  RifaService, PaymentService, TicketService, etc.   │      │
│  └─────┬───────────────────────────────────────────────┘      │
│        │                                                      │
│  ┌─────▼──────────────────────────────────────────────┐      │
│  │              Services Especializados                │      │
│  │  MercadoPago, Notifications, Cache, Webhook, etc.   │      │
│  └─────┬───────────────────────────────────────────────┘      │
│        │                                                      │
│  ┌─────▼─────────┐  ┌─────────────┐                          │
│  │  AppDbContext  │  │  AutoMapper │                          │
│  │ (EF Core +    │  │  (Profiles) │                          │
│  │  Identity)    │  │             │                          │
│  └─────┬─────────┘  └─────────────┘                          │
└────────┼─────────────────────────────────────────────────────┘
         │
┌────────▼──────────┐      ┌───────────────────┐
│   MySQL Database  │      │ RifaTech.DTOs     │
│   (RifaTechDB)    │      │ (DTOs, Contracts, │
└───────────────────┘      │  Responses)       │
                           └───────────────────┘
```

---

## 3. Pontos Positivos ✅

### 3.1. Separação de DTOs em projeto independente
O projeto `RifaTech.DTOs` mantém DTOs, interfaces de contratos e respostas separados da API — facilitando compartilhamento com o frontend Blazor.

### 3.2. Minimal APIs bem organizadas
Os endpoints estão separados por domínio em arquivos dedicados (`RifaEndpoints`, `PaymentEndpoints`, `ClienteEndpoints`, etc.) com extension methods, facilitando manutenção.

### 3.3. Cache com abstração
A `ICacheService` com `MemoryCacheService` permite trocar a implementação de cache sem alterar os serviços de negócio.

### 3.4. Sistema de notificações multi-canal
O pattern de `MultiChannelNotificationService` que orquestra `EmailNotificationService` e `IWhatsAppService` é um bom design com Strategy pattern implícito.

### 3.5. Background Services
O uso de `PaymentStatusVerificationService` e `NotificationBackgroundService` como `BackgroundService` do .NET é adequado para polling de pagamentos.

### 3.6. Soft-delete implementado
Entidades usam `EhDeleted` e `DeletedAt` para marcar registros como deletados sem remoção física.

### 3.7. Entidade base consistente
`EntityBase` provê `Id`, `CreatedAt`, `UpdatedAt` e `DeletedAt` herdados por todas as entidades.

### 3.8. Documentação Swagger completa
Todos os endpoints têm `WithOpenApi()` com `Summary` e `Description` em português.

---

## 4. Problemas Críticos 🔴

### 4.1. Credenciais expostas no `appsettings.json`
```json
"Jwt": { "Key": "YcxjOMewdFfeZFQm5iGAYxTjR23Z93rLbyZucty3" },
"MercadoPago": { "AccessToken": "APP_USR-213291017..." }
```
**Impacto:** Chaves JWT, tokens do Mercado Pago e credenciais de email estão hardcoded e comitados no repositório.  
**Correção:**
- Usar **User Secrets** para desenvolvimento local.
- Usar **Azure Key Vault**, **AWS Secrets Manager** ou variáveis de ambiente em produção.
- Adicionar `appsettings.json` ao `.gitignore` para valores sensíveis ou separar em `appsettings.Secrets.json`.

### 4.2. CORS AllowAll (Aceita qualquer origem)
```csharp
builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
```
**Impacto:** Qualquer site pode fazer requisições à API, o que é uma vulnerabilidade de segurança grave em produção.  
**Correção:** Restringir a origens conhecidas (domínio do frontend).

### 4.3. DbContext registrado DUAS vezes no `Program.cs`
```csharp
// Linha ~63 - primeira vez
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Linha ~113 - segunda vez com MigrationsAssembly
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        x => x.MigrationsAssembly("RifaTech.API")));
```
**Impacto:** Comportamento imprevisível na resolução de dependências, apenas o último registro é usado.  
**Correção:** Manter apenas um registro unificado.

### 4.4. `AddApplicationServices()` chamado DUAS vezes
```csharp
// Linha 20
builder.Services.AddApplicationServices();
// Linha 108
builder.Services.AddApplicationServices();
```
**Impacto:** Serviços são registrados em duplicata, `HostedService` como `PaymentStatusVerificationService` pode executar duas instâncias simultâneas.  
**Correção:** Remover a chamada duplicada.

### 4.5. `BuildServiceProvider()` para inicialização de roles
```csharp
await builder.Services.InitializeRoles(builder.Services.BuildServiceProvider());
```
**Impacto:** Cria um `ServiceProvider` separado que não compartilha estado com o container principal — gera aviso de análise e possíveis memory leaks.  
**Correção:** Mover a inicialização de roles para após `builder.Build()` usando `app.Services`.

### 4.6. Tipo inconsistente para valores monetários
- `Payment.Amount` é `float` (imprecisão com moeda!)
- `ExtraNumber.PrizeAmount` é `float`
- `Rifa.TicketPrice` é `decimal` ✅
- `Rifa.PriceValue` é `decimal` ✅
- `RifaDTO.TicketPrice` é `float` (inconsistente com a entidade que é `decimal`)

**Impacto:** `float` causa erros de arredondamento com valores monetários (ex: R$ 0.10 + R$ 0.20 ≠ R$ 0.30).  
**Correção:** Usar `decimal` para TODOS os campos monetários em entidades e DTOs.

### 4.7. Webhook sem validação de assinatura
```csharp
app.MapPost("/webhooks/mercadopago", ...).AllowAnonymous();
```
**Impacto:** Qualquer pessoa pode enviar webhooks falsos para manipular status de pagamentos.  
**Correção:** Validar a assinatura HMAC do header `x-signature` do Mercado Pago.

---

## 5. Problemas de Arquitetura e Design 🟠

### 5.1. Pasta "Repositories" contém Services, não Repositories
A pasta `Repositories/` contém classes como `RifaService`, `PaymentService`, `TicketService` — que na verdade são **serviços de aplicação** e não implementam o padrão Repository.

Essas classes acessam o `DbContext` diretamente E contêm lógica de negócio, resultando em violação do **SRP** (Single Responsibility Principle).

**Recomendação:**
- Renomear `Repositories/` para `Services/` ou unificar na pasta `Services/`.
- Ou implementar um verdadeiro **Repository Pattern** separando acesso a dados de lógica de negócio.

### 5.2. Todos os serviços registrados como Transient
```csharp
services.AddTransient<IRifaService, RifaService>();
services.AddTransient<ITicketService, TicketService>();
services.AddTransient<IPaymentService, Repositories.PaymentService>();
```
**Problema:** `Transient` cria uma nova instância a cada injeção. Como `AppDbContext` é `Scoped`, usar `Transient` para serviços que dependem dele pode causar problemas de tracking.  
**Correção:** Usar `AddScoped` para todos os serviços que dependem do `DbContext`.

### 5.3. Swashbuckle referenciado duas vezes com versões diferentes
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.7.3" />
```
**Correção:** Manter apenas a versão mais recente.

### 5.4. Ausência de camada de validação (FluentValidation)
As validações estão apenas nos Data Annotations das entidades. Não há validação nos DTOs de entrada dos endpoints.

**Recomendação:** Adicionar **FluentValidation** para validação robusta nos endpoints com `EndpointFilter`.

### 5.5. Exception Handling inconsistente
Alguns endpoints usam `throw` (propaga exceção bruta), outros retornam `Results.Problem()`, e o middleware `ExceptionHandlingMiddleware` existe mas **não é usado** no `Program.cs`.

```csharp
// Existe mas NÃO é registrado:
app.UseCustomExceptionHandling(logger);
```
**Correção:** Registrar um middleware global de tratamento de exceções e padronizar respostas de erro usando `RFC 7807 (ProblemDetails)`.

### 5.6. SQL Raw como fallback no `GetAllRifasAsync`
```csharp
catch (FormatException ex) when (ex.Message.Contains("Could not parse CHAR(36) value as Guid"))
{
    var query = "SELECT * FROM Rifas WHERE EhDeleted = 0";
    // ... ADO.NET direto
}
```
**Problema:** Isso é um workaround para dados corrompidos no banco, não uma solução. Além disso, cria o objeto `Rifa` manualmente sem preencher todas as propriedades.  
**Correção:** Corrigir os dados inválidos no banco e remover o fallback.

### 5.7. Imagens armazenadas como Base64 no banco
```csharp
public string? Base64Img { get; set; }
```
**Impacto:** Base64 é ~33% maior que binário. Armazenar images no banco de dados aumenta drasticamente o tamanho e lentidão das queries.  
**Correção:** Usar **Azure Blob Storage**, **AWS S3** ou armazenamento em disco com URL de referência.

### 5.8. `MercadoPagoConfig.AccessToken` é estático global
```csharp
MercadoPagoConfig.AccessToken = _accessToken;
```
**Problema:** O SDK do MercadoPago usa configuração estática global, o que não é thread-safe em cenários multi-tenant.  
**Impacto limitado** para single-tenant, mas é um risco a monitorar.

---

## 6. Problemas Menores 🟡

### 6.1. String interpolation em logs
```csharp
logger.LogInformation($"Retrieved rifa with ID {id} successfully");
```
**Correção:** Usar structured logging do ASP.NET:
```csharp
logger.LogInformation("Retrieved rifa with ID {RifaId} successfully", id);
```

### 6.2. DTOs com lógica computada
```csharp
public bool? IsDone => DrawDateTime < DateTime.UtcNow;
public int? TicketsSold => Tickets?.Count ?? 0;
```
**Problema:** DTOs devem ser POCOs puros para serialização. Lógica computada pode gerar resultados inesperados ao deserializar.

### 6.3. Falta de paginação global
Apenas `GetRifasPaginatedAsync` tem paginação. Endpoints como `GetAllPaymentsAsync()` e `GetAllRifasAsync()` retornam TODOS os registros sem limite.

### 6.4. Sem Health Checks
O projeto não possui endpoints de health check para monitoramento.  
**Correção:** Adicionar `builder.Services.AddHealthChecks().AddMySql(connectionString)`.

### 6.5. Sem Rate Limiting
Endpoints públicos (compra rápida, consulta de rifas) não têm rate limiting, vulneráveis a abuse.  
**Correção:** Usar `builder.Services.AddRateLimiter()` do .NET 8.

### 6.6. Falta de projeto de testes
Não há nenhum projeto de testes unitários ou de integração na solução.  
**Correção:** Criar `RifaTech.Tests` com xUnit ou NUnit.

### 6.7. `TicketSearchEndpoints.cs` dentro de `Services/`
O arquivo `Services/TicketSearchEndpoints.cs` é um arquivo de endpoints na pasta errada.

---

## 7. Recomendações de Melhoria (Priorizado)

### 🔴 Prioridade Alta (Segurança/Produção)

| # | Ação | Esforço |
|---|---|---|
| 1 | Remover credenciais do `appsettings.json` e usar User Secrets/Env Vars | Baixo |
| 2 | Restringir CORS para domínios conhecidos | Baixo |
| 3 | Validar assinatura de webhooks do Mercado Pago | Médio |
| 4 | Corrigir registro duplicado de DbContext e Services no `Program.cs` | Baixo |
| 5 | Mudar todos os campos monetários para `decimal` | Médio |
| 6 | Ativar middleware global de exceções | Baixo |

### 🟠 Prioridade Média (Qualidade/Manutenção)

| # | Ação | Esforço |
|---|---|---|
| 7 | Unificar pastas `Repositories/` e `Services/` | Baixo |
| 8 | Mudar lifetime de `Transient` para `Scoped` nos serviços | Baixo |
| 9 | Remover fallback SQL raw de `GetAllRifasAsync` | Médio |
| 10 | Migrar imagens para storage externo (blob/S3/disco) | Alto |
| 11 | Adicionar FluentValidation | Médio |
| 12 | Implementar paginação em todos os endpoints de listagem | Médio |
| 13 | Corrigir referência duplicada do Swashbuckle no `.csproj` | Baixo |
| 14 | Mover inicialização de roles para após `Build()` | Baixo |

### 🟡 Prioridade Baixa (Boas Práticas)

| # | Ação | Esforço |
|---|---|---|
| 15 | Adicionar Health Checks | Baixo |
| 16 | Adicionar Rate Limiting nos endpoints públicos | Médio |
| 17 | Criar projeto de testes `RifaTech.Tests` | Alto |
| 18 | Structured logging (remover interpolação em logs) | Baixo |
| 19 | Remover lógica computada dos DTOs | Baixo |
| 20 | Mover `TicketSearchEndpoints.cs` para pasta `Endpoints/` | Baixo |

---

## 8. Sugestão de Arquitetura Melhorada

```
RifaTech/
├── RifaTech.API/                    # Camada de apresentação
│   ├── Endpoints/                   # Minimal API endpoints
│   ├── Middleware/                   # Auth, Exception handling
│   ├── Filters/                     # Validação, Rate limiting
│   └── Program.cs
│
├── RifaTech.Application/            # 🆕 Camada de aplicação
│   ├── Services/                    # Lógica de negócio
│   ├── Validators/                  # FluentValidation
│   ├── Interfaces/                  # Contratos de serviço
│   └── Mappings/                    # AutoMapper profiles
│
├── RifaTech.Domain/                 # 🆕 Camada de domínio
│   ├── Entities/                    # Entidades de domínio
│   ├── Enums/                       # Enumerações
│   ├── Events/                      # Domain events
│   └── Exceptions/                  # Exceções de domínio
│
├── RifaTech.Infrastructure/         # 🆕 Camada de infraestrutura
│   ├── Data/                        # DbContext, Migrations
│   ├── Repositories/                # Repository pattern real
│   ├── ExternalServices/            # MercadoPago, WhatsApp
│   ├── Caching/                     # Cache implementations
│   └── Notifications/               # Email, SMS
│
├── RifaTech.DTOs/                   # Contratos compartilhados
│   ├── DTOs/
│   ├── Contracts/
│   └── Responses/
│
├── RifaTech.UI/                     # Frontend Blazor
│
└── RifaTech.Tests/                  # 🆕 Testes
    ├── Unit/
    ├── Integration/
    └── Functional/
```

---

## 9. Nota Final

| Critério | Nota (1-10) | Observação |
|---|---|---|
| Organização de código | 6/10 | Boa separação de endpoints, mas nomenclatura confusa (Repos vs Services) |
| Segurança | 3/10 | Credenciais expostas, CORS aberto, webhook sem validação |
| Tratamento de erros | 4/10 | Inconsistente, middleware existe mas não é usado |
| Modelagem de dados | 6/10 | Boa entidade base, mas tipos monetários incorretos |
| Escalabilidade | 5/10 | MemoryCache local, sem paginação universal, imagens no banco |
| Testabilidade | 3/10 | Nenhum teste, serviços Transient dificultam mocking |
| Integração de pagamento | 7/10 | MercadoPago bem integrado com webhook e polling |
| Notificações | 8/10 | Multi-canal bem projetado com fallback |
| **Média** | **5.25/10** | **Funcional, mas precisa de melhorias para produção** |

---

> **Resumo:** O projeto tem uma base funcional sólida com boas escolhas em Minimal APIs, integração MercadoPago e notificações multi-canal. Porém, **não está pronto para produção** devido a problemas de segurança críticos (credenciais expostas, CORS aberto, webhooks sem validação). Com as correções de prioridade alta (itens 1-6), pode ir para produção de forma segura. As melhorias de médio prazo (itens 7-14) irão melhorar significativamente a manutenibilidade e robustez do sistema.
