# 🎟️ RifaTech

Plataforma completa para criação e gerenciamento de rifas online com integração de pagamento via **Mercado Pago (PIX)**, notificações por **e-mail** e **WhatsApp**, painel administrativo e aplicação mobile via **.NET MAUI**.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura do Projeto](#-arquitetura-do-projeto)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
  - [Opção 1 — Docker Compose (Recomendado)](#opção-1--docker-compose-recomendado)
  - [Opção 2 — Execução Local](#opção-2--execução-local)
- [Configuração](#-configuração)
  - [Banco de Dados](#banco-de-dados)
  - [JWT (Autenticação)](#jwt-autenticação)
  - [Mercado Pago](#mercado-pago)
  - [E-mail](#e-mail)
  - [WhatsApp](#whatsapp)
- [Executando o Projeto](#-executando-o-projeto)
- [Executando os Testes](#-executando-os-testes)
- [Endpoints Principais da API](#-endpoints-principais-da-api)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Variáveis de Ambiente](#-variáveis-de-ambiente)
- [Licença](#-licença)

---

## 🔍 Visão Geral

O RifaTech permite:

- **Criar e gerenciar rifas** com imagens, preço, data de sorteio e quantidade de bilhetes.
- **Compra rápida** sem necessidade de cadastro — basta nome, telefone e e-mail.
- **Pagamento via PIX** com integração ao Mercado Pago e verificação automática por webhook.
- **Sorteios** com seleção automática de ganhadores.
- **Notificações** por e-mail e WhatsApp (comprovante, lembretes, resultado).
- **Painel administrativo** protegido por JWT com dashboard de métricas.
- **App mobile** (Android/iOS/Windows) via .NET MAUI Blazor Hybrid.

---

## 🏗️ Arquitetura do Projeto

| Projeto | Framework | Descrição |
|---|---|---|
| **RifaTech.API** | ASP.NET Core 8.0 (Minimal APIs) | Backend — API REST, autenticação, pagamentos, sorteios |
| **RifaTech.DTOs** | .NET 8.0 (Class Library) | Contratos, DTOs e Responses compartilhados |
| **RifaTech.UI.Shared** | .NET 9.0 (Razor Class Library) | Componentes Blazor compartilhados entre Web e Mobile |
| **RifaTech.UI.Web** | ASP.NET Core 9.0 (Blazor Server) | Aplicação Web (SSR + WebAssembly interativo) |
| **RifaTech.UI.Web.Client** | .NET 9.0 (Blazor WebAssembly) | Componentes interativos client-side |
| **RifaTech.UI** | .NET MAUI 9.0 (Blazor Hybrid) | App Mobile (Android, iOS, Windows, macOS) |
| **RifaTech.Tests** | xUnit + Moq + FluentAssertions | Testes unitários e de integração |

**Banco de dados:** MySQL 8.0 (via Pomelo EF Core)

---

## ✅ Pré-requisitos

### Para execução com Docker (Recomendado)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (inclui Docker Compose)

### Para execução local
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (para API e DTOs)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (para projetos UI)
- [MySQL 8.0](https://dev.mysql.com/downloads/mysql/) (ou MariaDB compatível)
- [Git](https://git-scm.com/)

### Para desenvolvimento mobile (opcional)
- [.NET MAUI workload](https://learn.microsoft.com/dotnet/maui/get-started/installation)
- Visual Studio 2022 17.9+ com workload ".NET Multi-platform App UI"

---

## 📦 Instalação

### 1. Clonar o repositório

```bash
git clone https://github.com/seu-usuario/RifaTech.git
cd RifaTech
```

---

### Opção 1 — Docker Compose (Recomendado)

A forma mais rápida de subir todo o ambiente (MySQL + API + UI Web) com um único comando.

```bash
docker compose up -d
```

Isso irá:
1. Criar um container **MySQL 8.0** com o banco `RifaTechDB` na porta `3306`.
2. Aguardar o MySQL ficar saudável (health check automático).
3. Subir a **API** na porta `5127` com migrations automáticas.
4. Subir a **UI Web** na porta `7230`.

**URLs após inicialização:**

| Serviço | URL |
|---|---|
| API | http://localhost:5127 |
| Swagger (API) | http://localhost:5127/swagger |
| UI Web | http://localhost:7230 |
| MySQL | localhost:3306 |

**Para parar os containers:**

```bash
docker compose down
```

**Para parar e remover os dados do banco:**

```bash
docker compose down -v
```

---

### Opção 2 — Execução Local

#### 2.1. Instalar o MySQL

Instale o MySQL 8.0 e crie o banco de dados:

```sql
CREATE DATABASE RifaTechDB;
```

Ou use Docker apenas para o MySQL:

```bash
docker run -d \
  --name rifatech-mysql \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=RifaTechDB \
  -p 3306:3306 \
  mysql:8.0
```

#### 2.2. Restaurar dependências

Na raiz do projeto:

```bash
dotnet restore RifaTech.sln
```

#### 2.3. Configurar a connection string

Verifique se o arquivo `RifaTech.API/appsettings.json` contém a connection string correta:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;DataBase=RifaTechDB;Uid=root;Pwd=root;"
  }
}
```

> **Dica:** Para não expor senhas no repositório, use [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):
> ```bash
> cd RifaTech.API
> dotnet user-secrets init
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;DataBase=RifaTechDB;Uid=root;Pwd=SUA_SENHA;"
> ```

#### 2.4. Aplicar as migrations

As migrations são aplicadas **automaticamente** ao iniciar a API. Caso prefira aplicar manualmente:

```bash
cd RifaTech.API
dotnet ef database update
```

#### 2.5. Executar a API

```bash
cd RifaTech.API
dotnet run
```

A API estará disponível em:
- **HTTP:** http://localhost:5127
- **HTTPS:** https://localhost:7212
- **Swagger:** http://localhost:5127/swagger

#### 2.6. Executar a UI Web

Em outro terminal:

```bash
cd RifaTech.UI/RifaTech.UI.Web
dotnet run
```

A UI Web estará disponível em:
- http://localhost:7230

---

## ⚙️ Configuração

Todas as configurações ficam em `RifaTech.API/appsettings.json`. Para ambientes de produção, use variáveis de ambiente ou User Secrets.

### Banco de Dados

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;DataBase=RifaTechDB;Uid=root;Pwd=root;"
}
```

### JWT (Autenticação)

A API usa JWT Bearer para autenticação do painel admin. Configure uma chave segura com no mínimo 32 caracteres:

```json
"Jwt": {
  "Key": "SUA_CHAVE_SECRETA_COM_PELO_MENOS_32_CARACTERES!!",
  "Issuer": "https://localhost:7145",
  "Audience": "https://localhost:7145"
}
```

> **Importante:** Em produção, **nunca** deixe a chave JWT no `appsettings.json`. Use variáveis de ambiente ou User Secrets.

### Mercado Pago

Para habilitar pagamentos via PIX:

1. Crie uma conta no [Mercado Pago Developers](https://www.mercadopago.com.br/developers).
2. Obtenha as credenciais (Access Token, Public Key, Client Secret).
3. Configure o webhook no painel do Mercado Pago apontando para `https://seu-dominio/api/webhook/mercadopago`.

```json
"MercadoPago": {
  "AccessToken": "SEU_ACCESS_TOKEN",
  "PublicKey": "SUA_PUBLIC_KEY",
  "ClientSecret": "SEU_CLIENT_SECRET",
  "WebhookSecret": "SEU_WEBHOOK_SECRET"
}
```

### E-mail

Para envio de comprovantes e notificações por e-mail:

```json
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "seu-email@gmail.com",
  "Password": "sua-senha-de-app",
  "SenderEmail": "rifatech@exemplo.com",
  "SenderName": "RifaTech",
  "Enabled": true
}
```

> Para Gmail, use uma [Senha de App](https://support.google.com/accounts/answer/185833).

### WhatsApp

Para envio de mensagens via WhatsApp Business API:

```json
"WhatsApp": {
  "AccessToken": "SEU_ACCESS_TOKEN",
  "PhoneNumberId": "SEU_PHONE_NUMBER_ID",
  "Enabled": true
}
```

---

## 🚀 Executando o Projeto

### Com Docker Compose (tudo de uma vez)

```bash
docker compose up -d
```

### Localmente (dois terminais)

**Terminal 1 — API:**
```bash
cd RifaTech.API
dotnet run
```

**Terminal 2 — UI Web:**
```bash
cd RifaTech.UI/RifaTech.UI.Web
dotnet run
```

### App Mobile (MAUI)

Abra a solução no **Visual Studio 2022**, selecione o projeto `RifaTech.UI` como projeto de inicialização, escolha o emulador/dispositivo e pressione **F5**.

---

## 🧪 Executando os Testes

```bash
dotnet test RifaTech.Tests/RifaTech.Tests.csproj
```

Com cobertura de código:

```bash
dotnet test RifaTech.Tests/RifaTech.Tests.csproj --collect:"XPlat Code Coverage"
```

---

## 📡 Endpoints Principais da API

| Método | Rota | Descrição | Autenticação |
|---|---|---|---|
| `POST` | `/api/account/login` | Login do admin (retorna JWT) | Não |
| `GET` | `/api/rifas` | Listar rifas disponíveis | Não |
| `GET` | `/api/rifas/{id}` | Detalhes de uma rifa | Não |
| `POST` | `/api/rifas` | Criar nova rifa | Admin |
| `POST` | `/api/compra-rapida` | Compra rápida (sem cadastro) | Não |
| `POST` | `/api/payments/pix` | Gerar pagamento PIX | Não |
| `GET` | `/api/tickets` | Listar bilhetes | Admin |
| `POST` | `/api/draws` | Realizar sorteio | Admin |
| `GET` | `/api/admin/dashboard` | Dashboard administrativo | Admin |
| `GET` | `/health` | Health check | Não |

> Acesse a documentação completa da API via **Swagger**: http://localhost:5127/swagger

---

## 📂 Estrutura do Projeto

```
RifaTech/
├── docker-compose.yml              # Orquestração dos containers
├── RifaTech.sln                     # Solution file
│
├── RifaTech.API/                    # Backend (API REST)
│   ├── Context/                     # DbContext e Identity
│   ├── Endpoints/                   # Minimal API endpoints
│   ├── Entities/                    # Entidades do domínio
│   ├── Exceptions/                  # Middleware de exceções
│   ├── Extensions/                  # Service registrations
│   ├── Migrations/                  # EF Core migrations
│   ├── Repositories/                # Serviços de dados
│   ├── Services/                    # Serviços de negócio
│   ├── Validators/                  # FluentValidation
│   └── appsettings.json             # Configurações
│
├── RifaTech.DTOs/                   # DTOs e contratos compartilhados
│   ├── Contracts/                   # Interfaces de serviço
│   ├── DTOs/                        # Data Transfer Objects
│   └── Responses/                   # Modelos de resposta
│
├── RifaTech.UI/                     # Frontend
│   ├── RifaTech.UI/                 # App Mobile (.NET MAUI)
│   ├── RifaTech.UI.Shared/          # Componentes Blazor compartilhados
│   ├── RifaTech.UI.Web/             # Blazor Server (SSR)
│   └── RifaTech.UI.Web.Client/      # Blazor WebAssembly
│
└── RifaTech.Tests/                  # Testes (xUnit)
    ├── Services/                    # Testes de serviços
    └── Validators/                  # Testes de validadores
```

---

## 🔐 Variáveis de Ambiente

Ao usar Docker ou produção, as configurações podem ser passadas via variáveis de ambiente:

| Variável | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string MySQL | `Server=mysql;DataBase=RifaTechDB;Uid=root;Pwd=root;Port=3306;` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Development` / `Production` |
| `Jwt__Key` | Chave secreta JWT (32+ chars) | `MinhaChaveSuperSecreta1234567890!!` |
| `Jwt__Issuer` | Emissor do token JWT | `https://meudominio.com` |
| `Jwt__Audience` | Audiência do token JWT | `https://meudominio.com` |
| `MercadoPago__AccessToken` | Token de acesso Mercado Pago | `APP_USR-...` |
| `MercadoPago__PublicKey` | Chave pública Mercado Pago | `APP_USR-...` |
| `Email__Enabled` | Habilitar envio de e-mails | `true` / `false` |
| `WhatsApp__Enabled` | Habilitar envio WhatsApp | `true` / `false` |
| `ApiSettings__BaseUrl` | URL da API (para UI) | `http://api:8080` |
| `ApiSettings__BrowserBaseUrl` | URL da API no browser | `http://localhost:5127` |

---

## 🔧 Dicas de Desenvolvimento

- A API aplica **migrations automaticamente** ao iniciar — não precisa rodar `dotnet ef database update` manualmente.
- Os **roles** `Admin`, `User` e `Mestre` são criados automaticamente na inicialização.
- O **Swagger** só está disponível no ambiente `Development`.
- A API possui **rate limiting** nos endpoints públicos (60 req/min) e de pagamento (10 req/min).
- Use `docker compose logs -f api` para acompanhar os logs da API em tempo real.

---

## 📄 Licença

Este projeto está licenciado sob a [MIT License](LICENSE.txt).
