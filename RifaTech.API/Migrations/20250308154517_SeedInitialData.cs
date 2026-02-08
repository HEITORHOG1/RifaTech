using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RifaTech.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Inserir dados de clientes de exemplo com dados mais realistas
            InsertClientes(migrationBuilder);

            // Inserir rifas com dados mais completos e variados
            InsertRifas(migrationBuilder);

            // Inserir tickets com mais detalhes
            InsertTickets(migrationBuilder);

            // Inserir payments para simular diferentes estados
        }

        private void InsertClientes(MigrationBuilder migrationBuilder)
        {
            // Inserir 5 clientes com dados mais detalhados e variados
            var clientes = new[]
            {
                new
                {
                    Id = "70A29C3A-24AB-4FD9-8E99-E2E2D7E7CA5D",
                    Name = "João Silva Santos",
                    Email = "joao@rifa.com",
                    PhoneNumber = "11999887766",
                    CPF = "12345678900",
                    CreatedAt = DateTime.UtcNow
                },
                new
                {
                    Id = "E23A72D2-3CC5-48D1-A459-5C659B18B13F",
                    Name = "Maria Aparecida Oliveira",
                    Email = "maria@rifa.com",
                    PhoneNumber = "11988776655",
                    CPF = "98765432100",
                    CreatedAt = DateTime.UtcNow
                },
                new
                {
                    Id = "3B6FCCA4-96DF-48C9-AEB3-7F166D138C27",
                    Name = "Pedro Henrique Souza",
                    Email = "pedro@rifa.com",
                    PhoneNumber = "11977665544",
                    CPF = "45678912300",
                    CreatedAt = DateTime.UtcNow
                },
                new
                {
                    Id = "F2DFD30A-E09F-45E6-BC14-C757969BC675",
                    Name = "Ana Carolina Rocha",
                    Email = "ana@rifa.com",
                    PhoneNumber = "11966554433",
                    CPF = "78912345600",
                    CreatedAt = DateTime.UtcNow
                },
                new
                {
                    Id = "8954B3A9-32C7-4A7A-95CE-34D3F087E5FD",
                    Name = "Carlos Eduardo Pereira",
                    Email = "carlos@rifa.com",
                    PhoneNumber = "11955443322",
                    CPF = "32165498700",
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var cliente in clientes)
            {
                migrationBuilder.InsertData(
                    table: "Clientes",
                    columns: new[] { "Id", "Name", "Email", "PhoneNumber", "CPF", "CreatedAt" },
                    values: new object[] {
                        cliente.Id,
                        cliente.Name,
                        cliente.Email,
                        cliente.PhoneNumber,
                        cliente.CPF,
                        cliente.CreatedAt
                    }
                );
            }
        }

        private void InsertRifas(MigrationBuilder migrationBuilder)
        {
            var rifas = new[]
            {
                new
                {
                    Id = "BA7B8C1D-3DA3-4B5C-A7E5-2A9C8B45F6D7",
                    Name = "iPhone 15 Pro Max 512GB",
                    Description = "Concorra a um iPhone 15 Pro Max de 512GB, com garantia Apple. O smartphone mais avançado já lançado!",
                    StartDate = DateTime.UtcNow.AddDays(-45),
                    EndDate = DateTime.UtcNow.AddDays(15),
                    TicketPrice = 50.00m,
                    DrawDateTime = DateTime.UtcNow.AddDays(15),
                    MinTickets = 50,
                    MaxTickets = 500,
                    Base64Img = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMgAAABkCAIAAABM5OhcAAABd0lEQVR4nO3dzWnDMACA0aZ0hoyTMTKkx/A4niIHB2OcH0rIh0vz3kkIHYT4sHTzYZyGL3i37703wP8kLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEj97b2B/p+N5GfvJ3rt8elhzVeM0rPNaxnNnd9dsZjbjfuN/navwapyGTUZPxr9Z+eGEtfUkjnVVj2aYCevqdDzPMa2/Pbzs4ATvvqgeuV2zHKA31pqwSLgKSQiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIvEBRm0UM67fnggAAAAAElFTkSuQmCC\r\n",
                    UserId = "22A8599E-41C5-4D3E-BD43-A8C26055AD89",
                    RifaLink = "/rifas/iphone-15-pro-max",
                    UniqueId = "IPHONE15PRO",
                    EhDeleted = false,
                    PriceValue = 12999.00m
                },
                new
                {
                    Id = "7E29F3B8-6C5D-4A9E-8F1G-2H6I7J8K9L0M",
                    Name = "Smart TV Samsung QLED 65\" 4K",
                    Description = "Smart TV Samsung QLED 65\" 4K, com tecnologia Quantum Dot e suporte para todos os principais serviços de streaming.",
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(30),
                    TicketPrice = 25.00m,
                    DrawDateTime = DateTime.UtcNow.AddDays(30),
                    MinTickets = 100,
                    MaxTickets = 1000,
                    Base64Img = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMgAAABkCAIAAABM5OhcAAABeElEQVR4nO3dQU6DUBRAUWpcAxO21uWxNSaswkFNRVuMxt6g5pwR/DD4ITfvM+O0LvMAj/Z09Ab4n4RFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhPU943Qep/PRu/gDhPVqm4t0fu756A38IuN0Xpf5bmGXvzpeby8XHxb9+XHLxNp1W881nXWZb9fNuS1hvSOORxHWm8vg+eKJ5iv+c8LatXcIbhfvPsMwDCevg4KJRUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkhEVCWCSERUJYJIRFQlgkXgDqYTpWN2O4/wAAAABJRU5ErkJggg==\r\n",
                    UserId = "22A8599E-41C5-4D3E-BD43-A8C26055AD89",
                    RifaLink = "/rifas/samsung-qled-tv",
                    UniqueId = "SAMSUNGQLED65",
                    EhDeleted = false,
                    PriceValue = 7999.00m
                },
                new
                {
                    Id = "9O8N7M6L-5K4J-3I2H-1G0F-E5D4C3B2A1P",
                    Name = "PlayStation 5 + Controle Extra",
                    Description = "PlayStation 5 na versão standard + controle DualSense extra. O console mais desejado do momento com a possibilidade de um segundo controle!",
                    StartDate = DateTime.UtcNow.AddDays(-20),
                    EndDate = DateTime.UtcNow.AddDays(45),
                    TicketPrice = 30.00m,
                    DrawDateTime = DateTime.UtcNow.AddDays(45),
                    MinTickets = 200,
                    MaxTickets = 2000,
                    Base64Img = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMgAAABkCAIAAABM5OhcAAABdklEQVR4nO3dPWrDMACA0aZ0z80y5pAefTPfIIODMc4PJeTDpXlvEkKDEB+WNh+mcfiCd/veewP8T8IiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiIhLBLCIiEsEsIiISwSwiLxs/cG9nc8nZexn+y9y6eHNVc1jcM6r2U8d3Z3zWZmM+43/te5Cq+mcdhk9GT8m5UfTlhbT+JYV/Vohpmwro6n8xzT+tvDyw5O8O6L6pHbNcsBemOtCYuEq5CEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSAiLhLBICIuEsEgIi4SwSFwAnNJQmAmekEYAAAAASUVORK5CYII=\r\n",
                    UserId = "22A8599E-41C5-4D3E-BD43-A8C26055AD89",
                    RifaLink = "/rifas/playstation-5",
                    UniqueId = "PS5EXTRA",
                    EhDeleted = false,
                    PriceValue = 4999.00m
                }
            };

            foreach (var rifa in rifas)
            {
                migrationBuilder.InsertData(
                    table: "Rifas",
                    columns: new[] {
                        "Id", "Name", "Description", "StartDate", "EndDate",
                        "TicketPrice", "DrawDateTime", "MinTickets", "MaxTickets",
                        "Base64Img", "UserId", "RifaLink", "UniqueId", "EhDeleted", "PriceValue",
                        "CreatedAt"
                    },
                    values: new object[] {
                        rifa.Id, rifa.Name, rifa.Description, rifa.StartDate, rifa.EndDate,
                        rifa.TicketPrice, rifa.DrawDateTime, rifa.MinTickets, rifa.MaxTickets,
                        rifa.Base64Img, rifa.UserId, rifa.RifaLink, rifa.UniqueId, rifa.EhDeleted,
                        rifa.PriceValue, DateTime.UtcNow
                    }
                );
            }
        }

        private void InsertTickets(MigrationBuilder migrationBuilder)
        {
            // Simular compras para diferentes rifas
            var ticketSets = new[]
            {
                new
                {
                    RifaId = "BA7B8C1D-3DA3-4B5C-A7E5-2A9C8B45F6D7",
                    ClienteId = "70A29C3A-24AB-4FD9-8E99-E2E2D7E7CA5D",
                    StartNumber = 1,
                    Count = 10
                },
                new
                {
                    RifaId = "BA7B8C1D-3DA3-4B5C-A7E5-2A9C8B45F6D7",
                    ClienteId = "E23A72D2-3CC5-48D1-A459-5C659B18B13F",
                    StartNumber = 11,
                    Count = 5
                },
                new
                {
                    RifaId = "7E29F3B8-6C5D-4A9E-8F1G-2H6I7J8K9L0M",
                    ClienteId = "3B6FCCA4-96DF-48C9-AEB3-7F166D138C27",
                    StartNumber = 1,
                    Count = 7
                }
            };

            foreach (var ticketSet in ticketSets)
            {
                for (int i = 0; i < ticketSet.Count; i++)
                {
                    migrationBuilder.InsertData(
                        table: "Tickets",
                        columns: new[] {
                            "Id", "RifaId", "ClienteId", "Number", "EhValido",
                            "PaymentId", "GeneratedTime", "CreatedAt"
                        },
                        values: new object[] {
                            Guid.NewGuid(),
                            ticketSet.RifaId,
                            ticketSet.ClienteId,
                            ticketSet.StartNumber + i,
                            true, // EhValido
                            null, // PaymentId
                            DateTime.UtcNow,
                            DateTime.UtcNow
                        }
                    );
                }
            }
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