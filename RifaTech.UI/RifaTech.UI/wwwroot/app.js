// app.js - Adicionar à pasta wwwroot/js/ do projeto
// Variável para controlar se o JS Interop está pronto para uso
window.isJsInteropReady = function () {
    return true;
};

// Evento disparado quando o documento estiver carregado
document.addEventListener('DOMContentLoaded', function () {
    console.log('JavaScript interop está pronto');
    window.isJsInteropReady = function () {
        return true;
    };
});
// Função para precarregar imagens
window.preloadImage = (url) => {
    return new Promise((resolve, reject) => {
        if (!url) {
            console.warn('URL de imagem vazia fornecida para preloadImage');
            resolve();
            return;
        }

        const img = new Image();
        img.onload = () => {
            console.log(`Imagem pré-carregada com sucesso: ${url}`);
            resolve();
        };
        img.onerror = (err) => {
            console.error(`Erro ao pré-carregar imagem: ${url}`, err);
            // Resolvemos mesmo com erro para não bloquear o fluxo
            resolve();
        };
        img.src = url;
    });
};

// Função para monitorar eventos de rede
window.setupNetworkMonitoring = () => {
    // Monitora requisições de rede
    const originalFetch = window.fetch;
    window.fetch = async function (input, init) {
        const url = typeof input === 'string' ? input : input.url;
        const start = performance.now();
        console.log(`🌐 Iniciando requisição para: ${url}`);

        try {
            const response = await originalFetch.apply(this, arguments);
            const end = performance.now();
            console.log(`✅ Requisição concluída: ${url} (${Math.round(end - start)}ms) - Status: ${response.status}`);
            return response;
        } catch (error) {
            const end = performance.now();
            console.error(`❌ Erro na requisição: ${url} (${Math.round(end - start)}ms)`, error);
            throw error;
        }
    };

    // Monitora requisições XMLHttpRequest (caso usado por algum componente)
    const originalXhrOpen = XMLHttpRequest.prototype.open;
    const originalXhrSend = XMLHttpRequest.prototype.send;

    XMLHttpRequest.prototype.open = function (method, url) {
        this._method = method;
        this._url = url;
        this._startTime = performance.now();
        return originalXhrOpen.apply(this, arguments);
    };

    XMLHttpRequest.prototype.send = function () {
        console.log(`🌐 Iniciando XHR ${this._method}: ${this._url}`);

        this.addEventListener('load', () => {
            const endTime = performance.now();
            console.log(`✅ XHR concluído: ${this._url} (${Math.round(endTime - this._startTime)}ms) - Status: ${this.status}`);
        });

        this.addEventListener('error', (e) => {
            const endTime = performance.now();
            console.error(`❌ Erro XHR: ${this._url} (${Math.round(endTime - this._startTime)}ms)`, e);
        });

        return originalXhrSend.apply(this, arguments);
    };

    console.log('📡 Monitoramento de rede ativado');
    return true;
};

// Função para obter informações do navegador
window.getBrowserInfo = () => {
    return {
        userAgent: navigator.userAgent,
        language: navigator.language,
        screenWidth: window.innerWidth,
        screenHeight: window.innerHeight,
        devicePixelRatio: window.devicePixelRatio,
        online: navigator.onLine,
        platform: navigator.platform,
        vendor: navigator.vendor
    };
};

// Função para simular atraso na rede (útil para testes)
window.setNetworkDelay = (milliseconds) => {
    const originalFetch = window.fetch;

    window.fetch = async function () {
        console.log(`🐌 Simulando atraso de rede de ${milliseconds}ms`);
        await new Promise(resolve => setTimeout(resolve, milliseconds));
        return originalFetch.apply(this, arguments);
    };

    console.log(`⏱️ Simulador de atraso de rede ativado (${milliseconds}ms)`);
    return true;
};

// Função para depurar eventos DOM
window.monitorElementChanges = (selector) => {
    try {
        const element = document.querySelector(selector);
        if (!element) {
            console.error(`Elemento não encontrado: ${selector}`);
            return false;
        }

        console.log(`🔍 Monitorando alterações em: ${selector}`);

        const observer = new MutationObserver((mutations) => {
            mutations.forEach(mutation => {
                console.log(`Mudança detectada em ${selector}:`, mutation);
            });
        });

        observer.observe(element, {
            attributes: true,
            childList: true,
            subtree: true
        });

        return true;
    } catch (error) {
        console.error(`Erro ao monitorar elemento: ${error.message}`);
        return false;
    }
};