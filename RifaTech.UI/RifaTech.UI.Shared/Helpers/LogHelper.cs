﻿using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RifaTech.UI.Shared.Helpers
{
    /// <summary>
    /// Classe utilitária para log e depuração em aplicações Blazor
    /// </summary>
    public class LogHelper
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly bool _isEnabled;
        private readonly string _componentName;

        public LogHelper(IJSRuntime jsRuntime, string componentName, bool isEnabled = true)
        {
            _jsRuntime = jsRuntime;
            _isEnabled = isEnabled;
            _componentName = componentName;
        }

        /// <summary>
        /// Escreve uma mensagem de log no console do navegador
        /// </summary>
        public async Task LogAsync(string message, LogLevel level = LogLevel.Info,
            [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!_isEnabled) return;

            string prefix = GetLogLevelPrefix(level);
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string formattedMessage = $"[{timestamp}] {prefix} [{_componentName}.{caller}:{lineNumber}] {message}";

            string jsFunction = level switch
            {
                LogLevel.Error => "console.error",
                LogLevel.Warning => "console.warn",
                LogLevel.Debug => "console.debug",
                LogLevel.Trace => "console.trace",
                _ => "console.log"
            };

            try
            {
                await _jsRuntime.InvokeVoidAsync(jsFunction, formattedMessage);
            }
            catch
            {
                // Ignora erros para não interromper o fluxo
            }
        }

        /// <summary>
        /// Escreve um objeto como JSON no console
        /// </summary>
        public async Task LogObjectAsync(string message, object data, LogLevel level = LogLevel.Info,
            [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!_isEnabled) return;

            try
            {
                string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    MaxDepth = 10
                });

                await LogAsync($"{message}: {jsonData}", level, caller, lineNumber);
            }
            catch (Exception ex)
            {
                await LogAsync($"Erro ao serializar objeto: {ex.Message}", LogLevel.Error, caller, lineNumber);
            }
        }

        /// <summary>
        /// Escreve uma exceção no console
        /// </summary>
        public async Task LogExceptionAsync(Exception exception, string message = "Erro",
            [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!_isEnabled) return;

            string exceptionDetails = $"{message}: {exception.Message}";
            if (exception.InnerException != null)
            {
                exceptionDetails += $" | Inner: {exception.InnerException.Message}";
            }

            await LogAsync(exceptionDetails, LogLevel.Error, caller, lineNumber);
            await LogAsync($"Stack Trace: {exception.StackTrace}", LogLevel.Error, caller, lineNumber);
        }

        /// <summary>
        /// Inicia um timer para medir tempo de execução
        /// </summary>
        public async Task<IAsyncDisposable> TimeOperationAsync(string operationName,
            [CallerMemberName] string caller = "")
        {
            if (!_isEnabled)
            {
                return new NullTimerScope();
            }

            return new TimerScope(_jsRuntime, operationName, _componentName, caller);
        }

        private string GetLogLevelPrefix(LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => "ℹ️",
                LogLevel.Warning => "⚠️",
                LogLevel.Error => "❌",
                LogLevel.Debug => "🔍",
                LogLevel.Trace => "🔬",
                LogLevel.Success => "✅",
                _ => "📝"
            };
        }

        private class TimerScope : IAsyncDisposable
        {
            private readonly IJSRuntime _jsRuntime;
            private readonly string _operationName;
            private readonly string _componentName;
            private readonly string _caller;
            private readonly System.Diagnostics.Stopwatch _stopwatch;

            public TimerScope(IJSRuntime jsRuntime, string operationName, string componentName, string caller)
            {
                _jsRuntime = jsRuntime;
                _operationName = operationName;
                _componentName = componentName;
                _caller = caller;
                _stopwatch = System.Diagnostics.Stopwatch.StartNew();
                _ = LogStartAsync();
            }

            private async Task LogStartAsync()
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string message = $"[{timestamp}] ⏱️ [{_componentName}.{_caller}] Iniciando: {_operationName}";
                await _jsRuntime.InvokeVoidAsync("console.time", _operationName);
                await _jsRuntime.InvokeVoidAsync("console.log", message);
            }

            public async ValueTask DisposeAsync()
            {
                _stopwatch.Stop();
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string message = $"[{timestamp}] ⏱️ [{_componentName}.{_caller}] Concluído: {_operationName} ({_stopwatch.ElapsedMilliseconds}ms)";
                await _jsRuntime.InvokeVoidAsync("console.timeEnd", _operationName);
                await _jsRuntime.InvokeVoidAsync("console.log", message);
            }
        }

        private class NullTimerScope : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug,
        Trace,
        Success
    }
}
