# RifaTech - Configuração de Ambiente

## Portas e URLs

### Ambiente de Desenvolvimento
- API: https://localhost:7212 (HTTPS) ou http://localhost:5127 (HTTP)
- UI Web: https://localhost:7230 (HTTPS) ou http://localhost:7231 (HTTP)

### Configuração da Aplicação
Todas as configurações de URLs e endpoints estão centralizadas na classe `AppConfig` no projeto `RifaTech.UI.Shared`.

## Como Configurar

1. Clone o repositório
2. Configure as portas nos arquivos de configuração se necessário:
   - API: `RifaTech.API/Properties/launchSettings.json`
   - Web UI: `RifaTech.UI.Web/Properties/launchSettings.json`
   
3. Caso precise alterar a URL da API, modifique os seguintes arquivos:
   - `RifaTech.UI.Shared/Config/AppConfig.cs` (URL principal)
   - `RifaTech.UI.Web/appsettings.json` (para a aplicação Web)
   - `RifaTech.UI/MauiProgram.cs` (para a aplicação Mobile)

4. Para publicação em produção, edite os arquivos `appsettings.Production.json` com as URLs correspondentes.