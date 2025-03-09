// Script para decodificar tokens JWT
window.decodeJwt = function (token) {
    if (!token) {
        return "Token vazio";
    }

    try {
        // Dividir o token em suas três partes
        const parts = token.split('.');
        if (parts.length !== 3) {
            return "Formato de token inválido";
        }

        // Decodificar o header e o payload
        const header = JSON.parse(atob(parts[0]));
        const payload = JSON.parse(atob(parts[1]));

        // Formatar o resultado como JSON bonito
        const result = {
            header: header,
            payload: payload
        };

        return JSON.stringify(result, null, 2);
    } catch (error) {
        console.error("Erro ao decodificar token:", error);
        return "Erro ao decodificar: " + error.message;
    }
};

// Funções para monitoramento de rede
window.setupNetworkMonitoring = function () {
    // Verificar se o navegador é compatível
    if (!window.performance || !window.performance.getEntries) {
        console.warn("Este navegador não suporta API de Performance");
        return;
    }

    // Registrar um observer para monitorar requisições de rede
    const originalFetch = window.fetch;
    window.fetch = async function (...args) {
        const url = args[0].url || args[0];
        const startTime = performance.now();
        console.log(`🌐 Iniciando requisição para: ${url}`);

        try {
            const response = await originalFetch.apply(this, args);
            const endTime = performance.now();
            const duration = endTime - startTime;

            console.log(`✅ Requisição para ${url} concluída em ${duration.toFixed(2)}ms com status ${response.status}`);

            // Clonar a resposta para não consumir o stream
            const clone = response.clone();

            // Log detalhado para debugging
            if (response.status >= 400) {
                try {
                    const text = await clone.text();
                    console.error(`❌ Erro na requisição ${url}: ${text}`);
                } catch (e) {
                    console.error(`❌ Erro ao ler resposta de ${url}`);
                }
            }

            return response;
        } catch (error) {
            const endTime = performance.now();
            const duration = endTime - startTime;
            console.error(`❌ Falha na requisição para ${url} após ${duration.toFixed(2)}ms: ${error.message}`);
            throw error;
        }
    };

    console.log("✅ Monitoramento de rede inicializado");
};

// Função para obter informações do navegador
window.getBrowserInfo = function () {
    return {
        userAgent: navigator.userAgent,
        platform: navigator.platform,
        language: navigator.language,
        cookiesEnabled: navigator.cookieEnabled,
        screenSize: {
            width: window.screen.width,
            height: window.screen.height
        },
        viewportSize: {
            width: window.innerWidth,
            height: window.innerHeight
        },
        connection: navigator.connection ? {
            effectiveType: navigator.connection.effectiveType,
            downlink: navigator.connection.downlink,
            rtt: navigator.connection.rtt,
            saveData: navigator.connection.saveData
        } : "API de Conexão não suportada"
    };
};

// Função para monitorar mudanças em elementos específicos
window.monitorElementChanges = function (selector) {
    const elements = document.querySelectorAll(selector);
    if (elements.length === 0) {
        console.warn(`Elementos não encontrados para o seletor: ${selector}`);
        return;
    }

    console.log(`Monitorando ${elements.length} elementos com seletor: ${selector}`);

    // Configurar observer
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            console.log(`Mudança detectada em ${selector}:`, mutation);
        });
    });

    // Observar cada elemento
    elements.forEach((element) => {
        observer.observe(element, {
            attributes: true,
            childList: true,
            subtree: true
        });
    });
};

// Função para pré-carregar imagens
window.preloadImage = function (url) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve(url);
        img.onerror = () => reject(new Error(`Falha ao carregar imagem: ${url}`));
        img.src = url;
    });
};