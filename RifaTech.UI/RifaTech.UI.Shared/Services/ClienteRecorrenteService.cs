// RifaTech.UI.Shared/Services/ClienteRecorrenteService.cs
using RifaTech.DTOs.DTOs;
using RifaTech.UI.Shared.Config;
using System.Text.Json;

namespace RifaTech.UI.Shared.Services
{
    public class ClienteRecorrenteService
    {
        private readonly ILocalStorageService _localStorage;
        private const string CLIENT_STORAGE_KEY = "cliente_info";

        public ClienteRecorrenteService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        /// <summary>
        /// Salva as informações do cliente no localStorage para uso em compras futuras
        /// </summary>
        public async Task SalvarClienteRecorrenteAsync(ClienteDTO cliente)
        {
            try
            {
                if (cliente != null && !string.IsNullOrEmpty(cliente.Email))
                {
                    await _localStorage.SetItemAsync(CLIENT_STORAGE_KEY, cliente);
                }
            }
            catch (Exception)
            {
                // Silenciar exceções relacionadas ao localStorage
            }
        }

        /// <summary>
        /// Recupera as informações do último cliente que fez compra
        /// </summary>
        public async Task<ClienteDTO> ObterClienteRecorrenteAsync()
        {
            try
            {
                // Verificar primeiro se o item existe
                var clienteInfo = await _localStorage.GetItemAsync<ClienteDTO>(CLIENT_STORAGE_KEY);
                return clienteInfo;
            }
            catch (Exception)
            {
                // Em caso de erro, retorna null
                return null;
            }
        }

        /// <summary>
        /// Verifica se há um cliente recorrente salvo
        /// </summary>
        public async Task<bool> ExisteClienteRecorrenteAsync()
        {
            try
            {
                // Como não temos o método ContainKeyAsync, vamos tentar obter o item
                // e verificar se é nulo.
                var clienteInfo = await _localStorage.GetItemAsync<ClienteDTO>(CLIENT_STORAGE_KEY);
                return clienteInfo != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Limpa as informações do cliente recorrente
        /// </summary>
        public async Task LimparClienteRecorrenteAsync()
        {
            try
            {
                await _localStorage.RemoveItemAsync(CLIENT_STORAGE_KEY);
            }
            catch (Exception)
            {
                // Silenciar exceções relacionadas ao localStorage
            }
        }
    }
}