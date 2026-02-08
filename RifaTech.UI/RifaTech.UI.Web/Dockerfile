# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore (cache layers)
COPY RifaTech.DTOs/RifaTech.DTOs.csproj RifaTech.DTOs/
COPY RifaTech.UI/RifaTech.UI.Shared/RifaTech.UI.Shared.csproj RifaTech.UI/RifaTech.UI.Shared/
COPY RifaTech.UI/RifaTech.UI.Web.Client/RifaTech.UI.Web.Client.csproj RifaTech.UI/RifaTech.UI.Web.Client/
COPY RifaTech.UI/RifaTech.UI.Web/RifaTech.UI.Web.csproj RifaTech.UI/RifaTech.UI.Web/
RUN dotnet restore RifaTech.UI/RifaTech.UI.Web/RifaTech.UI.Web.csproj

# Copy source code and publish
COPY RifaTech.DTOs/ RifaTech.DTOs/
COPY RifaTech.UI/RifaTech.UI.Shared/ RifaTech.UI/RifaTech.UI.Shared/
COPY RifaTech.UI/RifaTech.UI.Web.Client/ RifaTech.UI/RifaTech.UI.Web.Client/
COPY RifaTech.UI/RifaTech.UI.Web/ RifaTech.UI/RifaTech.UI.Web/
RUN dotnet publish RifaTech.UI/RifaTech.UI.Web/RifaTech.UI.Web.csproj -c Release -o /app/publish --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RifaTech.UI.Web.dll"]
