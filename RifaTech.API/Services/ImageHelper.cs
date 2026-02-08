using System.Text;

namespace RifaTech.API.Services;

/// <summary>
/// Helper para carregar imagens e converter para base64 para uso no seed.
/// </summary>
public static class ImageHelper
{
    private static readonly string ImagesPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Images");
    
    /// <summary>
    /// Carrega uma imagem SVG e converte para base64 data URI.
    /// </summary>
    /// <param name="fileName">Nome do arquivo (sem extensão ou com .svg)</param>
    /// <returns>String base64 data URI ou null se não encontrar</returns>
    public static string? GetImageBase64(string fileName)
    {
        try
        {
            // Garantir extensão .svg
            if (!fileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".svg";
            }
            
            var filePath = Path.Combine(ImagesPath, fileName);
            
            if (!File.Exists(filePath))
            {
                // Tentar caminho relativo ao diretório do projeto
                var projectPath = FindProjectRoot();
                if (projectPath != null)
                {
                    filePath = Path.Combine(projectPath, "Assets", "Images", fileName);
                }
            }
            
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            var svgContent = File.ReadAllText(filePath);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svgContent));
            
            return $"data:image/svg+xml;base64,{base64}";
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Gera um placeholder SVG colorido com texto.
    /// </summary>
    public static string GeneratePlaceholderSvg(string text, string primaryColor = "#6366f1", string secondaryColor = "#4f46e5")
    {
        var svg = $@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 400 300"">
  <defs>
    <linearGradient id=""bg"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
      <stop offset=""0%"" style=""stop-color:{primaryColor}""/>
      <stop offset=""100%"" style=""stop-color:{secondaryColor}""/>
    </linearGradient>
  </defs>
  <rect width=""400"" height=""300"" fill=""url(#bg)""/>
  <text x=""200"" y=""150"" font-family=""Arial, sans-serif"" font-size=""16"" font-weight=""bold"" fill=""white"" text-anchor=""middle"">{EscapeXml(text)}</text>
</svg>";
        
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{base64}";
    }
    
    /// <summary>
    /// Obtém um dicionário com todas as imagens de rifas disponíveis.
    /// Chave: rifaLink, Valor: base64 data URI
    /// </summary>
    public static Dictionary<string, string> GetAllRifaImages()
    {
        var images = new Dictionary<string, string>();
        
        // Imagens pré-definidas para as rifas do seed
        var rifaImages = new[]
        {
            ("iphone-16-pro-max", "#5c3aff", "#ff6b9d"),
            ("ps5-3-jogos", "#0070d1", "#00439c"),
            ("notebook-gamer-asus", "#ff3333", "#ff6600"),
            ("smart-tv-samsung-65", "#00d4ff", "#006699"),
            ("airpods-pro-2", "#ffffff", "#e8e8e8")
        };
        
        foreach (var (rifaLink, primary, secondary) in rifaImages)
        {
            // Tentar carregar do arquivo primeiro
            var imageBase64 = GetImageBase64(rifaLink);
            
            // Se não encontrar, gerar placeholder
            if (string.IsNullOrEmpty(imageBase64))
            {
                imageBase64 = GeneratePlaceholderSvg(rifaLink.Replace("-", " ").ToUpperInvariant(), primary, secondary);
            }
            
            images[rifaLink] = imageBase64;
        }
        
        return images;
    }
    
    private static string? FindProjectRoot()
    {
        var directory = AppContext.BaseDirectory;
        
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "RifaTech.API.csproj")))
            {
                return directory;
            }
            directory = Directory.GetParent(directory)?.FullName;
        }
        
        return null;
    }
    
    private static string EscapeXml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
