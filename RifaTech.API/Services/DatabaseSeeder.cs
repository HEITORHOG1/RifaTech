using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Context;
using RifaTech.API.Entities;

namespace RifaTech.API.Services;

/// <summary>
/// Popula o banco de dados com dados de exemplo para desenvolvimento/demonstração.
/// Só executa se o banco estiver vazio (sem clientes cadastrados).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Só faz seed se o banco estiver vazio
        if (await context.Clientes.AnyAsync())
        {
            logger.LogInformation("Database already seeded. Skipping...");
            return;
        }

        logger.LogInformation("Seeding database with example data...");

        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // ── 1. USUÁRIOS (Identity) ──────────────────────────────────────
            var adminUser = new ApplicationUser
            {
                UserName = "admin@rifatech.com",
                Email = "admin@rifatech.com",
                Name = "Administrador RifaTech",
                CPF = "00000000000",
                EhAdmin = true,
                EmailConfirmed = true
            };

            var user1 = new ApplicationUser
            {
                UserName = "joao@email.com",
                Email = "joao@email.com",
                Name = "João Silva",
                CPF = "11111111111",
                EhAdmin = false,
                EmailConfirmed = true
            };

            var user2 = new ApplicationUser
            {
                UserName = "maria@email.com",
                Email = "maria@email.com",
                Name = "Maria Santos",
                CPF = "22222222222",
                EhAdmin = false,
                EmailConfirmed = true
            };

            var user3 = new ApplicationUser
            {
                UserName = "carlos@email.com",
                Email = "carlos@email.com",
                Name = "Carlos Oliveira",
                CPF = "33333333333",
                EhAdmin = false,
                EmailConfirmed = true
            };

            // Garantir que as roles existam
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
            else
                logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

            result = await userManager.CreateAsync(user1, "User@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user1, "User");

            result = await userManager.CreateAsync(user2, "User@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user2, "User");

            result = await userManager.CreateAsync(user3, "User@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user3, "User");

            logger.LogInformation("Seeded 4 users (1 admin + 3 users).");

            // ── 2. CLIENTES ─────────────────────────────────────────────────
            var clientes = new List<Cliente>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "João Silva",
                    Email = "joao@email.com",
                    PhoneNumber = "11999990001",
                    CPF = "11111111111",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Maria Santos",
                    Email = "maria@email.com",
                    PhoneNumber = "11999990002",
                    CPF = "22222222222",
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Carlos Oliveira",
                    Email = "carlos@email.com",
                    PhoneNumber = "11999990003",
                    CPF = "33333333333",
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Ana Costa",
                    Email = "ana@email.com",
                    PhoneNumber = "11999990004",
                    CPF = "44444444444",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Pedro Souza",
                    Email = "pedro@email.com",
                    PhoneNumber = "11999990005",
                    CPF = "55555555555",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            await context.Clientes.AddRangeAsync(clientes);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} clients.", clientes.Count);

            // ── 3. RIFAS ────────────────────────────────────────────────────
            var now = DateTime.UtcNow;
            
            // Carregar imagens dos produtos
            var rifaImages = ImageHelper.GetAllRifaImages();
            logger.LogInformation("Loaded {Count} product images for rifas.", rifaImages.Count);

            var rifa1 = new Rifa
            {
                Id = Guid.NewGuid(),
                Name = "iPhone 16 Pro Max",
                Description = "Concorra a um iPhone 16 Pro Max 256GB novinho! O sorteio será realizado ao vivo.",
                StartDate = now.AddDays(-20),
                EndDate = now.AddDays(10),
                TicketPrice = 25.00m,
                PriceValue = 8999.99m,
                DrawDateTime = now.AddDays(10),
                MinTickets = 10,
                MaxTickets = 500,
                UserId = adminUser.Id,
                RifaLink = "iphone-16-pro-max",
                UniqueId = Guid.NewGuid().ToString("N")[..8],
                EhDeleted = false,
                CreatedAt = now.AddDays(-20),
                Base64Img = rifaImages.GetValueOrDefault("iphone-16-pro-max")
            };

            var rifa2 = new Rifa
            {
                Id = Guid.NewGuid(),
                Name = "PlayStation 5 + 3 Jogos",
                Description = "PS5 Slim Digital Edition com 3 jogos à sua escolha. Frete grátis para todo o Brasil!",
                StartDate = now.AddDays(-15),
                EndDate = now.AddDays(15),
                TicketPrice = 15.00m,
                PriceValue = 4500.00m,
                DrawDateTime = now.AddDays(15),
                MinTickets = 10,
                MaxTickets = 400,
                UserId = adminUser.Id,
                RifaLink = "ps5-3-jogos",
                UniqueId = Guid.NewGuid().ToString("N")[..8],
                EhDeleted = false,
                CreatedAt = now.AddDays(-15),
                Base64Img = rifaImages.GetValueOrDefault("ps5-3-jogos")
            };

            var rifa3 = new Rifa
            {
                Id = Guid.NewGuid(),
                Name = "Notebook Gamer ASUS ROG",
                Description = "ASUS ROG Strix G15, RTX 4060, 16GB RAM, 512GB SSD. Ideal para jogos e trabalho.",
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(20),
                TicketPrice = 50.00m,
                PriceValue = 12000.00m,
                DrawDateTime = now.AddDays(20),
                MinTickets = 5,
                MaxTickets = 300,
                UserId = adminUser.Id,
                RifaLink = "notebook-gamer-asus",
                UniqueId = Guid.NewGuid().ToString("N")[..8],
                EhDeleted = false,
                CreatedAt = now.AddDays(-10),
                Base64Img = rifaImages.GetValueOrDefault("notebook-gamer-asus")
            };

            var rifa4 = new Rifa
            {
                Id = Guid.NewGuid(),
                Name = "Smart TV Samsung 65\" 4K",
                Description = "Samsung Crystal UHD 65 polegadas com Alexa integrada. Entrega em todo o Brasil.",
                StartDate = now.AddDays(-25),
                EndDate = now.AddDays(5),
                TicketPrice = 10.00m,
                PriceValue = 3500.00m,
                DrawDateTime = now.AddDays(5),
                MinTickets = 10,
                MaxTickets = 500,
                UserId = adminUser.Id,
                RifaLink = "smart-tv-samsung-65",
                UniqueId = Guid.NewGuid().ToString("N")[..8],
                EhDeleted = false,
                CreatedAt = now.AddDays(-25),
                Base64Img = rifaImages.GetValueOrDefault("smart-tv-samsung-65")
            };

            // Rifa já encerrada (com vencedor)
            var rifa5 = new Rifa
            {
                Id = Guid.NewGuid(),
                Name = "AirPods Pro 2ª Geração",
                Description = "Apple AirPods Pro com cancelamento ativo de ruído e estojo MagSafe.",
                StartDate = now.AddDays(-60),
                EndDate = now.AddDays(-10),
                TicketPrice = 5.00m,
                PriceValue = 1800.00m,
                DrawDateTime = now.AddDays(-10),
                WinningNumber = 42,
                MinTickets = 10,
                MaxTickets = 500,
                UserId = adminUser.Id,
                RifaLink = "airpods-pro-2",
                UniqueId = Guid.NewGuid().ToString("N")[..8],
                EhDeleted = false,
                CreatedAt = now.AddDays(-60),
                Base64Img = rifaImages.GetValueOrDefault("airpods-pro-2")
            };

            var rifas = new List<Rifa> { rifa1, rifa2, rifa3, rifa4, rifa5 };
            await context.Rifas.AddRangeAsync(rifas);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} rifas.", rifas.Count);

            // ── 4. TICKETS ──────────────────────────────────────────────────
            var tickets = new List<Ticket>();
            var random = new Random(42); // seed fixo para reprodutibilidade

            // Tickets para rifa1 (iPhone) — 35 tickets vendidos
            for (int i = 1; i <= 35; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    RifaId = rifa1.Id,
                    ClienteId = clientes[random.Next(clientes.Count)].Id,
                    Number = i,
                    EhValido = true,
                    GeneratedTime = now.AddDays(-random.Next(1, 18)),
                    CreatedAt = now.AddDays(-random.Next(1, 18))
                });
            }

            // Tickets para rifa2 (PS5) — 20 tickets
            for (int i = 1; i <= 20; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    RifaId = rifa2.Id,
                    ClienteId = clientes[random.Next(clientes.Count)].Id,
                    Number = i,
                    EhValido = true,
                    GeneratedTime = now.AddDays(-random.Next(1, 12)),
                    CreatedAt = now.AddDays(-random.Next(1, 12))
                });
            }

            // Tickets para rifa3 (Notebook) — 12 tickets
            for (int i = 1; i <= 12; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    RifaId = rifa3.Id,
                    ClienteId = clientes[random.Next(clientes.Count)].Id,
                    Number = i,
                    EhValido = true,
                    GeneratedTime = now.AddDays(-random.Next(1, 8)),
                    CreatedAt = now.AddDays(-random.Next(1, 8))
                });
            }

            // Tickets para rifa4 (TV Samsung) — 45 tickets
            for (int i = 1; i <= 45; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    RifaId = rifa4.Id,
                    ClienteId = clientes[random.Next(clientes.Count)].Id,
                    Number = i,
                    EhValido = true,
                    GeneratedTime = now.AddDays(-random.Next(1, 22)),
                    CreatedAt = now.AddDays(-random.Next(1, 22))
                });
            }

            // Tickets para rifa5 (AirPods - encerrada) — 80 tickets
            for (int i = 1; i <= 80; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    RifaId = rifa5.Id,
                    ClienteId = clientes[random.Next(clientes.Count)].Id,
                    Number = i,
                    EhValido = true,
                    GeneratedTime = now.AddDays(-random.Next(15, 55)),
                    CreatedAt = now.AddDays(-random.Next(15, 55))
                });
            }

            await context.Tickets.AddRangeAsync(tickets);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} tickets.", tickets.Count);

            // ── 5. PAYMENTS ─────────────────────────────────────────────────
            var payments = new List<Payment>();

            foreach (var ticket in tickets)
            {
                var rifa = rifas.First(r => r.Id == ticket.RifaId);
                var statusRandom = random.Next(100);

                // 70% confirmado, 20% pendente, 10% expirado
                var status = statusRandom < 70
                    ? PaymentStatus.Confirmed
                    : statusRandom < 90
                        ? PaymentStatus.Pending
                        : PaymentStatus.Expired;

                var pixCode = $"00020126580014br.gov.bcb.pix0136{Guid.NewGuid()}5204000053039865802BR5913RifaTech6008BRASILIA62070503***6304";

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    ClienteId = ticket.ClienteId,
                    TicketId = ticket.Id,
                    Amount = rifa.TicketPrice,
                    Method = "PIX",
                    IsConfirmed = status == PaymentStatus.Confirmed,
                    Status = status,
                    ExpirationTime = ticket.CreatedAt.AddHours(24),
                    QrCode = pixCode,
                    QrCodeBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pixCode)),
                    PaymentId = status == PaymentStatus.Confirmed
                        ? random.NextInt64(100000000, 999999999)
                        : null,
                    CreatedAt = ticket.CreatedAt
                };

                payments.Add(payment);

                // Atualiza o ticket com o PaymentId
                ticket.PaymentId = payment.Id;
            }

            await context.Payments.AddRangeAsync(payments);
            context.Tickets.UpdateRange(tickets);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} payments.", payments.Count);

            // ── 6. EXTRA NUMBERS ────────────────────────────────────────────
            var extraNumbers = new List<ExtraNumber>();

            // Números extras para rifa1 (iPhone) — 3 prêmios extras
            extraNumbers.Add(new ExtraNumber
            {
                Id = Guid.NewGuid(),
                RifaId = rifa1.Id,
                Number = 100,
                PrizeAmount = 500.00m,
                CreatedAt = rifa1.CreatedAt
            });
            extraNumbers.Add(new ExtraNumber
            {
                Id = Guid.NewGuid(),
                RifaId = rifa1.Id,
                Number = 250,
                PrizeAmount = 300.00m,
                CreatedAt = rifa1.CreatedAt
            });
            extraNumbers.Add(new ExtraNumber
            {
                Id = Guid.NewGuid(),
                RifaId = rifa1.Id,
                Number = 400,
                PrizeAmount = 200.00m,
                CreatedAt = rifa1.CreatedAt
            });

            // Números extras para rifa4 (TV) — 2 prêmios extras
            extraNumbers.Add(new ExtraNumber
            {
                Id = Guid.NewGuid(),
                RifaId = rifa4.Id,
                Number = 150,
                PrizeAmount = 250.00m,
                CreatedAt = rifa4.CreatedAt
            });
            extraNumbers.Add(new ExtraNumber
            {
                Id = Guid.NewGuid(),
                RifaId = rifa4.Id,
                Number = 350,
                PrizeAmount = 150.00m,
                CreatedAt = rifa4.CreatedAt
            });

            await context.ExtraNumbers.AddRangeAsync(extraNumbers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} extra numbers.", extraNumbers.Count);

            // ── 7. DRAWS ────────────────────────────────────────────────────
            var draws = new List<Draw>();

            // Sorteio realizado para rifa5 (encerrada)
            draws.Add(new Draw
            {
                Id = Guid.NewGuid(),
                RifaId = rifa5.Id,
                DrawDateTime = now.AddDays(-10),
                WinningNumber = "42",
                CreatedAt = now.AddDays(-10)
            });

            // Sorteios agendados para as rifas ativas
            draws.Add(new Draw
            {
                Id = Guid.NewGuid(),
                RifaId = rifa1.Id,
                DrawDateTime = rifa1.DrawDateTime,
                WinningNumber = "",
                CreatedAt = rifa1.CreatedAt
            });

            draws.Add(new Draw
            {
                Id = Guid.NewGuid(),
                RifaId = rifa2.Id,
                DrawDateTime = rifa2.DrawDateTime,
                WinningNumber = "",
                CreatedAt = rifa2.CreatedAt
            });

            draws.Add(new Draw
            {
                Id = Guid.NewGuid(),
                RifaId = rifa4.Id,
                DrawDateTime = rifa4.DrawDateTime,
                WinningNumber = "",
                CreatedAt = rifa4.CreatedAt
            });

            await context.Draws.AddRangeAsync(draws);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} draws.", draws.Count);

            // ── 8. UNPAID RIFAS ─────────────────────────────────────────────
            var unpaidRifas = new List<UnpaidRifa>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ClienteId = clientes[3].Id, // Ana
                    RifaId = rifa1.Id,
                    DueDate = now.AddDays(3),
                    Quantidade = 2,
                    PrecoTotal = 50.00m,
                    CreatedAt = now.AddDays(-2)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ClienteId = clientes[4].Id, // Pedro
                    RifaId = rifa3.Id,
                    DueDate = now.AddDays(5),
                    Quantidade = 1,
                    PrecoTotal = 50.00m,
                    CreatedAt = now.AddDays(-1)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ClienteId = clientes[1].Id, // Maria
                    RifaId = rifa2.Id,
                    DueDate = now.AddDays(-1), // Expirada
                    Quantidade = 3,
                    PrecoTotal = 45.00m,
                    CreatedAt = now.AddDays(-5)
                }
            };

            await context.UnpaidRifas.AddRangeAsync(unpaidRifas);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} unpaid rifas.", unpaidRifas.Count);

            logger.LogInformation("=== Database seeding completed successfully! ===");
            logger.LogInformation("Summary: 4 users | 5 clients | 5 rifas | {Tickets} tickets | {Payments} payments | {Extras} extra numbers | {Draws} draws | {Unpaid} unpaid",
                tickets.Count, payments.Count, extraNumbers.Count, draws.Count, unpaidRifas.Count);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error seeding database. Transaction rolled back.");
            throw;
        }
    }
}
