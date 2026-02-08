// RifaTech.UI.Shared/Services/ClienteRecorrenteService.cs
using RifaTech.DTOs.DTOs;

namespace RifaTech.UI.Shared.Services
{
    public class ClienteRecorrenteService
    {
        private readonly IStorageService _localStorage;
        private const string CLIENT_STORAGE_KEY = "cliente_info";
        private bool _isRendered = false;

        public ClienteRecorrenteService(IStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        /// <summary>
        /// Notifica o serviço que a renderização foi concluída e é seguro acessar o localStorage
        /// </summary>
        public void SetRendered()
        {
            _isRendered = true;
        }

        /// <summary>
        /// Recupera as informações do último cliente que fez compra, com verificação de pré-renderização
        /// </summary>
        public async Task<ClienteDTO> ObterClienteRecorrenteAsync()
        {
            if (!_isRendered)
            {
                // Durante a pré-renderização, retorna null para evitar erros de JS interop
                return null;
            }

            try
            {
                // LocalStorageService já tem proteção contra pré-renderização
                return await _localStorage.GetItemAsync<ClienteDTO>(CLIENT_STORAGE_KEY);
            }
            catch (Exception)
            {
                // Em caso de erro, retorna null
                return null;
            }
        }

        /// <summary>
        /// Salva as informações do cliente no localStorage para uso em compras futuras
        /// </summary>
        public async Task SalvarClienteRecorrenteAsync(ClienteDTO cliente)
        {
            if (!_isRendered || cliente == null || string.IsNullOrEmpty(cliente.Email))
            {
                return;
            }

            try
            {
                await _localStorage.SetItemAsync(CLIENT_STORAGE_KEY, cliente);
            }
            catch (Exception)
            {
                // Silenciar exceções relacionadas ao localStorage
            }
        }

        /// <summary>
        /// Verifica se há um cliente recorrente salvo
        /// </summary>
        public async Task<bool> ExisteClienteRecorrenteAsync()
        {
            if (!_isRendered)
            {
                return false;
            }

            try
            {
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
            if (!_isRendered)
            {
                return;
            }

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