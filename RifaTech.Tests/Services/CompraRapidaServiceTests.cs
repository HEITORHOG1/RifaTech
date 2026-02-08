using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RifaTech.API.Services;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.Tests.Services;

public class CompraRapidaServiceTests
{
    private readonly Mock<IRifaService> _rifaServiceMock = new();
    private readonly Mock<IClienteService> _clienteServiceMock = new();
    private readonly Mock<ITicketService> _ticketServiceMock = new();
    private readonly Mock<IPaymentService> _paymentServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<ILogger<CompraRapidaService>> _loggerMock = new();

    private CompraRapidaService CreateService() => new(
        _rifaServiceMock.Object,
        _clienteServiceMock.Object,
        _ticketServiceMock.Object,
        _paymentServiceMock.Object,
        _notificationServiceMock.Object,
        _loggerMock.Object);

    private static readonly Guid RifaId = Guid.NewGuid();
    private static readonly Guid ClienteId = Guid.NewGuid();
    private static readonly Guid PaymentId = Guid.NewGuid();

    private static RifaDTO CreateRifa() => new()
    {
        Id = RifaId,
        Name = "Rifa Teste",
        TicketPrice = 10.00m,
        MaxTickets = 100,
        DrawDateTime = DateTime.UtcNow.AddDays(7),
        Tickets = new List<TicketDTO>()
    };

    private static CompraRapidaDTO CreateCompra() => new()
    {
        Name = "João",
        Email = "joao@email.com",
        PhoneNumber = "+5511999000111",
        CPF = "123.456.789-00",
        Quantidade = 3
    };

    [Fact]
    public async Task ProcessarCompraRapidaAsync_Should_Succeed_For_NewCliente()
    {
        // Arrange
        var rifa = CreateRifa();
        var compra = CreateCompra();
        var cliente = new ClienteDTO { Id = ClienteId, Name = compra.Name, Email = compra.Email };
        var payment = new PaymentDTO { Id = PaymentId, Amount = 30.00m, Status = 0 };
        var generatedNumbers = new List<int> { 10, 20, 30 };

        _rifaServiceMock.Setup(x => x.GetRifaByIdAsync(RifaId)).ReturnsAsync(rifa);
        _clienteServiceMock.Setup(x => x.GetClienteByEmailOrPhoneNumberOrCPFAsync(
            compra.Email, compra.PhoneNumber, compra.CPF)).ReturnsAsync((ClienteDTO?)null);
        _clienteServiceMock.Setup(x => x.CreateClienteAsync(It.IsAny<ClienteDTO>())).ReturnsAsync(cliente);
        _ticketServiceMock.Setup(x => x.PurchaseTicketAsync(RifaId.ToString(), It.IsAny<TicketDTO>())).ReturnsAsync(generatedNumbers);
        _paymentServiceMock.Setup(x => x.IniciarPagamentoPix(RifaId, 3, 30.00m, ClienteId)).ReturnsAsync(payment);

        var service = CreateService();

        // Act
        var result = await service.ProcessarCompraRapidaAsync(RifaId.ToString(), compra);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Cliente.Id.Should().Be(ClienteId);
        result.NumerosGerados.Should().HaveCount(3);
        result.ValorTotal.Should().Be(30.00m);
        result.RifaId.Should().Be(RifaId);

        _clienteServiceMock.Verify(x => x.CreateClienteAsync(It.IsAny<ClienteDTO>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarCompraRapidaAsync_Should_Succeed_For_ExistingCliente()
    {
        // Arrange
        var rifa = CreateRifa();
        var compra = CreateCompra();
        var existingCliente = new ClienteDTO { Id = ClienteId, Name = compra.Name, Email = compra.Email, CPF = compra.CPF };
        var payment = new PaymentDTO { Id = PaymentId, Amount = 30.00m, Status = 0 };
        var generatedNumbers = new List<int> { 10, 20, 30 };

        _rifaServiceMock.Setup(x => x.GetRifaByIdAsync(RifaId)).ReturnsAsync(rifa);
        _clienteServiceMock.Setup(x => x.GetClienteByEmailOrPhoneNumberOrCPFAsync(
            compra.Email, compra.PhoneNumber, compra.CPF)).ReturnsAsync(existingCliente);
        _ticketServiceMock.Setup(x => x.PurchaseTicketAsync(RifaId.ToString(), It.IsAny<TicketDTO>())).ReturnsAsync(generatedNumbers);
        _paymentServiceMock.Setup(x => x.IniciarPagamentoPix(RifaId, 3, 30.00m, ClienteId)).ReturnsAsync(payment);

        var service = CreateService();

        // Act
        var result = await service.ProcessarCompraRapidaAsync(RifaId.ToString(), compra);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        // Should NOT create a new cliente
        _clienteServiceMock.Verify(x => x.CreateClienteAsync(It.IsAny<ClienteDTO>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarCompraRapidaAsync_Should_Throw_When_RifaNotFound()
    {
        // Arrange
        _rifaServiceMock.Setup(x => x.GetRifaByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RifaDTO?)null);
        var service = CreateService();

        // Act & Assert
        var act = () => service.ProcessarCompraRapidaAsync(RifaId.ToString(), CreateCompra());
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{RifaId}*");
    }

    [Fact]
    public async Task ProcessarCompraRapidaAsync_Should_Throw_When_RifaHasEnded()
    {
        // Arrange
        var rifa = CreateRifa();
        rifa.DrawDateTime = DateTime.UtcNow.AddDays(-1); // Already ended

        _rifaServiceMock.Setup(x => x.GetRifaByIdAsync(RifaId)).ReturnsAsync(rifa);
        var service = CreateService();

        // Act & Assert
        var act = () => service.ProcessarCompraRapidaAsync(RifaId.ToString(), CreateCompra());
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already ended*");
    }

    [Fact]
    public async Task ProcessarCompraRapidaAsync_Should_Throw_When_NotEnoughTickets()
    {
        // Arrange
        var rifa = CreateRifa();
        rifa.MaxTickets = 2;
        rifa.Tickets = new List<TicketDTO> { new(), new() }; // All sold

        _rifaServiceMock.Setup(x => x.GetRifaByIdAsync(RifaId)).ReturnsAsync(rifa);
        var service = CreateService();

        var compra = CreateCompra();
        compra.Quantidade = 1;

        // Act & Assert
        var act = () => service.ProcessarCompraRapidaAsync(RifaId.ToString(), compra);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Not enough tickets*");
    }

    [Fact]
    public async Task ProcessarCompraRapidaAsync_Should_Not_Throw_When_Notification_Fails()
    {
        // Arrange
        var rifa = CreateRifa();
        var compra = CreateCompra();
        var cliente = new ClienteDTO { Id = ClienteId, Name = compra.Name, Email = compra.Email };
        var payment = new PaymentDTO { Id = PaymentId, Amount = 30.00m, Status = 0 };
        var generatedNumbers = new List<int> { 10, 20, 30 };

        _rifaServiceMock.Setup(x => x.GetRifaByIdAsync(RifaId)).ReturnsAsync(rifa);
        _clienteServiceMock.Setup(x => x.GetClienteByEmailOrPhoneNumberOrCPFAsync(
            compra.Email, compra.PhoneNumber, compra.CPF)).ReturnsAsync((ClienteDTO?)null);
        _clienteServiceMock.Setup(x => x.CreateClienteAsync(It.IsAny<ClienteDTO>())).ReturnsAsync(cliente);
        _ticketServiceMock.Setup(x => x.PurchaseTicketAsync(RifaId.ToString(), It.IsAny<TicketDTO>())).ReturnsAsync(generatedNumbers);
        _paymentServiceMock.Setup(x => x.IniciarPagamentoPix(RifaId, 3, 30.00m, ClienteId)).ReturnsAsync(payment);

        // Notification fails
        _notificationServiceMock.Setup(x => x.SendPurchaseConfirmationAsync(It.IsAny<CompraRapidaResponseDTO>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var service = CreateService();

        // Act — should NOT throw even though notification fails
        var result = await service.ProcessarCompraRapidaAsync(RifaId.ToString(), compra);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessarCompraRapidaAsync_Should_Calculate_Total_Correctly()
    {
        // Arrange
        var rifa = CreateRifa();
        rifa.TicketPrice = 15.50m;

        var compra = CreateCompra();
        compra.Quantidade = 4;

        var cliente = new ClienteDTO { Id = ClienteId };
        var expectedTotal = 15.50m * 4; // 62.00
        var payment = new PaymentDTO { Id = PaymentId, Amount = expectedTotal, Status = 0 };

        _rifaServiceMock.Setup(x => x.GetRifaByIdAsync(RifaId)).ReturnsAsync(rifa);
        _clienteServiceMock.Setup(x => x.GetClienteByEmailOrPhoneNumberOrCPFAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((ClienteDTO?)null);
        _clienteServiceMock.Setup(x => x.CreateClienteAsync(It.IsAny<ClienteDTO>())).ReturnsAsync(cliente);
        _ticketServiceMock.Setup(x => x.PurchaseTicketAsync(It.IsAny<string>(), It.IsAny<TicketDTO>()))
            .ReturnsAsync(new List<int> { 1, 2, 3, 4 });
        _paymentServiceMock.Setup(x => x.IniciarPagamentoPix(RifaId, 4, expectedTotal, ClienteId)).ReturnsAsync(payment);

        var service = CreateService();

        // Act
        var result = await service.ProcessarCompraRapidaAsync(RifaId.ToString(), compra);

        // Assert
        result.ValorTotal.Should().Be(62.00m);
        _paymentServiceMock.Verify(x => x.IniciarPagamentoPix(RifaId, 4, 62.00m, ClienteId), Times.Once);
    }
}
