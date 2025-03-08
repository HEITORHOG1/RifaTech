using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RifaTech.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Inserir dados de clientes de exemplo
            InsertClientes(migrationBuilder);

            // Inserir rifas
            InsertRifas(migrationBuilder);

            // Inserir tickets
            InsertTickets(migrationBuilder);

            // Inserir payments
            InsertPayments(migrationBuilder);
        }

        private void InsertClientes(MigrationBuilder migrationBuilder)
        {
            // Inserir 5 clientes de exemplo
            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Name", "Email", "PhoneNumber", "CPF", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[] { "70A29C3A-24AB-4FD9-8E99-E2E2D7E7CA5D", "João Silva", "joao.silva@exemplo.com", "11999887766", "12345678900", DateTime.UtcNow, null, null }
            );

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Name", "Email", "PhoneNumber", "CPF", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[] { "E23A72D2-3CC5-48D1-A459-5C659B18B13F", "Maria Santos", "maria.santos@exemplo.com", "11988776655", "98765432100", DateTime.UtcNow, null, null }
            );

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Name", "Email", "PhoneNumber", "CPF", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[] { "3B6FCCA4-96DF-48C9-AEB3-7F166D138C27", "Pedro Oliveira", "pedro.oliveira@exemplo.com", "11977665544", "45678912300", DateTime.UtcNow, null, null }
            );

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Name", "Email", "PhoneNumber", "CPF", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[] { "F2DFD30A-E09F-45E6-BC14-C757969BC675", "Ana Souza", "ana.souza@exemplo.com", "11966554433", "78912345600", DateTime.UtcNow, null, null }
            );

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Name", "Email", "PhoneNumber", "CPF", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[] { "8954B3A9-32C7-4A7A-95CE-34D3F087E5FD", "Carlos Pereira", "carlos.pereira@exemplo.com", "11955443322", "32165498700", DateTime.UtcNow, null, null }
            );
        }

        private void InsertRifas(MigrationBuilder migrationBuilder)
        {
            // Inserir rifas de exemplo com diferentes prazos e características

            // Rifa 1 - iPhone 14 - Prestes a encerrar
            var rifa1Id = "BA7B8C1D-3DA3-4B5C-A7E5-2A9C8B45F6D7";
            migrationBuilder.InsertData(
                table: "Rifas",
                columns: new[] {
                    "Id", "Name", "Description", "StartDate", "EndDate", "TicketPrice",
                    "DrawDateTime", "WinningNumber", "MinTickets", "MaxTickets",
                    "Base64Img", "UserId", "RifaLink", "UniqueId", "EhDeleted", "PriceValue",
                    "CreatedAt", "UpdatedAt", "DeletedAt"
                },
                values: new object[] {
                    rifa1Id,
                    "iPhone 14 Pro Max",
                    "Concorra a um iPhone 14 Pro Max novo na caixa, com garantia Apple. O sorteio ocorrerá em breve, não perca esta chance!",
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow.AddDays(3),
                    100.00m, // Preço do ticket
                    DateTime.UtcNow.AddDays(3), // Data do sorteio em 3 dias
                    null, // Ainda não tem número vencedor
                    50, // Mínimo de tickets
                    500, // Máximo de tickets
                    null, // Sem imagem por enquanto
                    "22A8599E-41C5-4D3E-BD43-A8C26055AD89", // ID do usuário admin
                    $"/rifas/{rifa1Id}", // Link
                    "ABC12345", // ID único
                    false, // Não está deletado
                    8499.00m, // Valor do prêmio
                    DateTime.UtcNow, // Data de criação
                    null, // Data de atualização
                    null // Data de deleção
                }
            );

            // Rifa 2 - Smart TV - Prazo intermediário
            var rifa2Id = "7E29F3B8-6C5D-4A9E-8F1G-2H6I7J8K9L0M";
            migrationBuilder.InsertData(
                table: "Rifas",
                columns: new[] {
                    "Id", "Name", "Description", "StartDate", "EndDate", "TicketPrice",
                    "DrawDateTime", "WinningNumber", "MinTickets", "MaxTickets",
                    "Base64Img", "UserId", "RifaLink", "UniqueId", "EhDeleted", "PriceValue",
                    "CreatedAt", "UpdatedAt", "DeletedAt"
                },
                values: new object[] {
                    rifa2Id,
                    "Smart TV 50\" 4K",
                    "Concorra a uma Smart TV 50 polegadas 4K UHD com acesso a todos os aplicativos de streaming. TV topo de linha!",
                    DateTime.UtcNow.AddDays(-15),
                    DateTime.UtcNow.AddDays(15),
                    50.00m, // Preço do ticket
                    DateTime.UtcNow.AddDays(15), // Data do sorteio em 15 dias
                    null, // Ainda não tem número vencedor
                    100, // Mínimo de tickets
                    1000, // Máximo de tickets
                    null, // Sem imagem por enquanto
                    "22A8599E-41C5-4D3E-BD43-A8C26055AD89", // ID do usuário admin
                    $"/rifas/{rifa2Id}", // Link
                    "DEF67890", // ID único
                    false, // Não está deletado
                    3299.00m, // Valor do prêmio
                    DateTime.UtcNow, // Data de criação
                    null, // Data de atualização
                    null // Data de deleção
                }
            );

            // Rifa 3 - Playstation 5 - Prazo longo
            var rifa3Id = "9O8N7M6L-5K4J-3I2H-1G0F-E5D4C3B2A1P";
            migrationBuilder.InsertData(
                table: "Rifas",
                columns: new[] {
                    "Id", "Name", "Description", "StartDate", "EndDate", "TicketPrice",
                    "DrawDateTime", "WinningNumber", "MinTickets", "MaxTickets",
                    "Base64Img", "UserId", "RifaLink", "UniqueId", "EhDeleted", "PriceValue",
                    "CreatedAt", "UpdatedAt", "DeletedAt"
                },
                values: new object[] {
                    rifa3Id,
                    "PlayStation 5",
                    "Concorra a um PlayStation 5 novinho, com dois controles e três jogos à sua escolha. O console mais desejado do momento pode ser seu!",
                    DateTime.UtcNow.AddDays(-5),
                    DateTime.UtcNow.AddDays(30),
                    25.00m, // Preço do ticket
                    DateTime.UtcNow.AddDays(30), // Data do sorteio em 30 dias
                    null, // Ainda não tem número vencedor
                    200, // Mínimo de tickets
                    2000, // Máximo de tickets
                    null, // Sem imagem por enquanto
                    "22A8599E-41C5-4D3E-BD43-A8C26055AD89", // ID do usuário admin
                    $"/rifas/{rifa3Id}", // Link
                    "GHI13579", // ID único
                    false, // Não está deletado
                    4999.00m, // Valor do prêmio
                    DateTime.UtcNow, // Data de criação
                    null, // Data de atualização
                    null // Data de deleção
                }
            );

            // Rifa 4 - Viagem para Cancún - Rifa premium de alto valor
            var rifa4Id = "Q1W2E3R4-T5Y6-U7I8-O9P0-A1S2D3F4G5H";
            migrationBuilder.InsertData(
                table: "Rifas",
                columns: new[] {
                    "Id", "Name", "Description", "StartDate", "EndDate", "TicketPrice",
                    "DrawDateTime", "WinningNumber", "MinTickets", "MaxTickets",
                    "Base64Img", "UserId", "RifaLink", "UniqueId", "EhDeleted", "PriceValue",
                    "CreatedAt", "UpdatedAt", "DeletedAt"
                },
                values: new object[] {
                    rifa4Id,
                    "Viagem para Cancún - 7 dias",
                    "Concorra a uma viagem para Cancún com tudo incluso para duas pessoas! 7 dias e 6 noites em resort 5 estrelas all-inclusive, passagens aéreas e transfer inclusos.",
                    DateTime.UtcNow.AddDays(-20),
                    DateTime.UtcNow.AddDays(45),
                    200.00m, // Preço do ticket
                    DateTime.UtcNow.AddDays(45), // Data do sorteio em 45 dias
                    null, // Ainda não tem número vencedor
                    100, // Mínimo de tickets
                    500, // Máximo de tickets
                    null, // Sem imagem por enquanto
                    "22A8599E-41C5-4D3E-BD43-A8C26055AD89", // ID do usuário admin
                    $"/rifas/{rifa4Id}", // Link
                    "JKL24680", // ID único
                    false, // Não está deletado
                    15000.00m, // Valor do prêmio
                    DateTime.UtcNow, // Data de criação
                    null, // Data de atualização
                    null // Data de deleção
                }
            );

            // Rifa 5 - Moto Honda - Rifa de veículo
            var rifa5Id = "Z9X8C7V6-B5N4-M3L2-K1J0-H6G5F4D3S2A";
            migrationBuilder.InsertData(
                table: "Rifas",
                columns: new[] {
                    "Id", "Name", "Description", "StartDate", "EndDate", "TicketPrice",
                    "DrawDateTime", "WinningNumber", "MinTickets", "MaxTickets",
                    "Base64Img", "UserId", "RifaLink", "UniqueId", "EhDeleted", "PriceValue",
                    "CreatedAt", "UpdatedAt", "DeletedAt"
                },
                values: new object[] {
                    rifa5Id,
                    "Moto Honda CG 160",
                    "Concorra a uma moto Honda CG 160 Start 0KM, ano 2025, com documentação e primeiro emplacamento inclusos. A moto será entregue na concessionária mais próxima do ganhador.",
                    DateTime.UtcNow.AddDays(-10),
                    DateTime.UtcNow.AddDays(60),
                    30.00m, // Preço do ticket
                    DateTime.UtcNow.AddDays(60), // Data do sorteio em 60 dias
                    null, // Ainda não tem número vencedor
                    300, // Mínimo de tickets
                    3000, // Máximo de tickets
                    null, // Sem imagem por enquanto
                    "22A8599E-41C5-4D3E-BD43-A8C26055AD89", // ID do usuário admin
                    $"/rifas/{rifa5Id}", // Link
                    "MNO35791", // ID único
                    false, // Não está deletado
                    16990.00m, // Valor do prêmio
                    DateTime.UtcNow, // Data de criação
                    null, // Data de atualização
                    null // Data de deleção
                }
            );
        }

        private void InsertTickets(MigrationBuilder migrationBuilder)
        {
            // Como não podemos inserir tickets aleatórios em uma migração (pois queremos que seja determinístico),
            // vamos inserir alguns tickets fixos para demonstração

            // Inserir 10 tickets para a primeira rifa (iPhone)
            var rifaId = "BA7B8C1D-3DA3-4B5C-A7E5-2A9C8B45F6D7";
            var clienteId = "70A29C3A-24AB-4FD9-8E99-E2E2D7E7CA5D"; // João Silva

            for (int i = 1; i <= 10; i++)
            {
                migrationBuilder.InsertData(
                    table: "Tickets",
                    columns: new[] { "Id", "RifaId", "ClienteId", "Number", "EhValido", "PaymentId", "GeneratedTime", "CreatedAt" },
                    values: new object[] {
                        Guid.NewGuid().ToString(),
                        rifaId,
                        clienteId,
                        i, // Número sequencial 
                        true, // Válido
                        null, // PaymentId será preenchido depois
                        DateTime.UtcNow,
                        DateTime.UtcNow
                    }
                );
            }

            // Inserir 5 tickets para a segunda rifa (TV)
            rifaId = "7E29F3B8-6C5D-4A9E-8F1G-2H6I7J8K9L0M";
            clienteId = "E23A72D2-3CC5-48D1-A459-5C659B18B13F"; // Maria Santos

            for (int i = 1; i <= 5; i++)
            {
                migrationBuilder.InsertData(
                    table: "Tickets",
                    columns: new[] { "Id", "RifaId", "ClienteId", "Number", "EhValido", "PaymentId", "GeneratedTime", "CreatedAt" },
                    values: new object[] {
                        Guid.NewGuid().ToString(),
                        rifaId,
                        clienteId,
                        i, // Número sequencial 
                        true, // Válido
                        null, // PaymentId será preenchido depois
                        DateTime.UtcNow,
                        DateTime.UtcNow
                    }
                );
            }
        }

        private void InsertPayments(MigrationBuilder migrationBuilder)
        {
            // Para as migrações, é melhor inserir pagamentos fixos em vez de aleatórios
            // Aqui inserimos alguns pagamentos de exemplo

            migrationBuilder.Sql(@"
                INSERT INTO Payments (Id, ClienteId, TicketId, Amount, Method, IsConfirmed, QrCodeBase64, Status, ExpirationTime, QrCode, PaymentId, CreatedAt)
                SELECT 
                    UUID() as Id,
                    '70A29C3A-24AB-4FD9-8E99-E2E2D7E7CA5D' as ClienteId,
                    (SELECT Id FROM Tickets WHERE ClienteId = '70A29C3A-24AB-4FD9-8E99-E2E2D7E7CA5D' LIMIT 1) as TicketId,
                    100.00 as Amount,
                    'PIX' as Method,
                    TRUE as IsConfirmed,
                    '' as QrCodeBase64,
                    1 as Status, -- 1 = Confirmed
                    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 1 DAY) as ExpirationTime,
                    '00020126580014BR.GOV.BCB.PIX0136123e4567-e12b-12d1-a456-426655440000' as QrCode,
                    NULL as PaymentId,
                    UTC_TIMESTAMP() as CreatedAt
            ");

            migrationBuilder.Sql(@"
                INSERT INTO Payments (Id, ClienteId, TicketId, Amount, Method, IsConfirmed, QrCodeBase64, Status, ExpirationTime, QrCode, PaymentId, CreatedAt)
                SELECT 
                    UUID() as Id,
                    'E23A72D2-3CC5-48D1-A459-5C659B18B13F' as ClienteId,
                    (SELECT Id FROM Tickets WHERE ClienteId = 'E23A72D2-3CC5-48D1-A459-5C659B18B13F' LIMIT 1) as TicketId,
                    50.00 as Amount,
                    'PIX' as Method,
                    TRUE as IsConfirmed,
                    '' as QrCodeBase64,
                    1 as Status, -- 1 = Confirmed
                    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 1 DAY) as ExpirationTime,
                    '00020126580014BR.GOV.BCB.PIX0136123e4567-e12b-12d1-a456-426655440000' as QrCode,
                    NULL as PaymentId,
                    UTC_TIMESTAMP() as CreatedAt
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover os dados inseridos na ordem inversa
            migrationBuilder.Sql("DELETE FROM Payments");
            migrationBuilder.Sql("DELETE FROM Tickets");
            migrationBuilder.Sql("DELETE FROM Rifas");
            migrationBuilder.Sql("DELETE FROM Clientes");
        }
    }
}
