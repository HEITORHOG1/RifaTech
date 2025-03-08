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



tarefas que vamos fazer para o projeto
Fluxo ideal simplificado
Página inicial:
Banner com rifas em destaque
Listagem de todas as rifas disponíveis (com imagens atraentes)
Busca/filtro simples (opcional)
Zero necessidade de login para visualizar
Compra rápida:
Cliente seleciona a rifa desejada
Escolhe quantidade de bilhetes
Fornece apenas informações essenciais (nome, telefone, email)
Pagamento via PIX (sem necessidade de criar conta)
Recebimento do comprovante/número por email/WhatsApp
Admin único:
Painel simplificado para criação de rifas
Gestão de vendas
Realização de sorteios
Controle de ganhadores
Melhorias específicas para este modelo
Frontend (UI)
Simplificar a página inicial:
Redesenhar a home para mostrar rifas em destaque bem visíveis
Adicionar um banner rotativo com as principais rifas
Mostrar contador regressivo para sorteios próximos (cria urgência)
Simplificar o processo de compra:
Reduzir para 2-3 cliques até o pagamento
Formulário mínimo (apenas dados essenciais)
Opção "comprar novamente" para clientes recorrentes (via cookie/local storage)
Otimização mobile:
Garantir que a experiência em celular seja perfeita (a maioria virá do WhatsApp)
Botões grandes e fáceis de clicar
Layout que funcione bem em telas pequenas
Backend (API)
Simplificar autenticação:
Manter autenticação apenas para admin
Clientes podem comprar sem criar conta
Otimizar endpoints:
Criar endpoint específico para "compra rápida" que faça tudo em uma chamada
Endpoint para verificar pagamento PIX em tempo real
Integração WhatsApp:
Adicionar envio automático de comprovante por WhatsApp
Enviar lembretes do sorteio
Banco de dados
Simplificar o modelo para apenas o essencial:
Rifas (titulo, descrição, preço, data sorteio, imagem)
Clientes (só os dados básicos, sem necessidade de senha)
Tickets (associação cliente-rifa)
Pagamentos (status, comprovante)
Exemplos concretos de melhorias:
Na página inicial (RifasList.razor):
Remover a necessidade de login para ver rifas
Adicionar banners em destaque no topo
Simplificar a navegação
Na tela de compra (BuyTickets.razor):
Reduzir os passos para no máximo 2 telas
Formulário minimalista
Botão de "Comprar com WhatsApp" para facilitar contato
No processo de pagamento:
Mostrar QR code PIX maior e mais visível
Botão para enviar comprovante por WhatsApp
Página de "compra realizada" mais celebrativa
API (Backend)
Tarefa 1: Simplificar Autenticação e Acesso
Manter autenticação apenas para admin
Remover restrições de autenticação para endpoints públicos (listagem de rifas, visualização de detalhes)
Criar middleware para identificar facilmente se o usuário é admin
Tarefa 2: Otimizar Endpoint de Rifas
Ajustar endpoint GET /rifas para retornar dados otimizados (incluir dados importantes como status, contagem de tickets vendidos)
Adicionar parâmetro para rifas em destaque
Implementar cache para melhorar performance
Tarefa 3: Implementar Compra Rápida
Criar endpoint POST /compra-rapida que combine criação de cliente e compra de tickets
Validar dados mínimos necessários (nome, telefone, email)
Implementar lógica para clientes recorrentes (identificação por email/telefone)
Tarefa 4: Melhorar Sistema de Pagamento
Atualizar endpoint de pagamento PIX para processar transações mais rapidamente
Implementar verificação automática de pagamentos
Adicionar notificação por webhook para confirmação instantânea
Tarefa 5: Implementar Notificações
Criar serviço para envio de email com comprovante
Preparar estrutura para integração com WhatsApp API
Implementar templates de mensagens (compra, confirmação, lembrete)
Tarefa 6: Otimizar Admin Dashboard
Criar endpoints específicos para estatísticas simples (vendas diárias, prêmios entregues)
Endpoint para gerenciar sorteios de forma simplificada
Implementar busca e filtros eficientes para tickets vendidos
Frontend (UI)
Tarefa 7: Redesenhar Página Inicial
Implementar banner rotativo com rifas em destaque
Criar grid de rifas com design atraente e informações essenciais
Adicionar contadores regressivos para próximos sorteios
Otimizar carregamento para exibição rápida de imagens
Tarefa 8: Simplificar Processo de Compra
Reduzir fluxo para máximo 2-3 telas
Criar formulário minimalista (apenas nome, telefone, email)
Implementar seleção rápida de quantidade de números
Adicionar detecção de cliente recorrente via localStorage
Tarefa 9: Melhorar Experiência de Pagamento
Redesenhar tela de QR Code PIX para maior visibilidade
Implementar verificação automática de pagamento (polling)
Adicionar opções de "Copiar código PIX" e "Abrir app do banco"
Criar tela de sucesso celebrativa após confirmação
Tarefa 10: Otimizar para Mobile
Garantir responsividade perfeita para todos os componentes
Aumentar tamanho de botões e elementos interativos
Implementar gestos de swipe para navegação entre rifas
Testar e otimizar performance em dispositivos móveis
Tarefa 11: Implementar Compartilhamento Social
Adicionar botões para compartilhar rifas via WhatsApp, Instagram
Criar links de compartilhamento com preview personalizado
Implementar opção para compartilhar após compra concluída
Tarefa 12: Desenvolver Painel Admin Simplificado
Criar dashboard com métricas essenciais (vendas, pagamentos pendentes)
Implementar CRUD simplificado para rifas
Desenvolver interface para realização de sorteios
Adicionar gerenciamento de ganhadores e prêmios entregues

