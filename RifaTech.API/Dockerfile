# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore (cache layers)
COPY RifaTech.DTOs/RifaTech.DTOs.csproj RifaTech.DTOs/
COPY RifaTech.API/RifaTech.API.csproj RifaTech.API/
RUN dotnet restore RifaTech.API/RifaTech.API.csproj

# Copy source code and publish
COPY RifaTech.DTOs/ RifaTech.DTOs/
COPY RifaTech.API/ RifaTech.API/
RUN dotnet publish RifaTech.API/RifaTech.API.csproj -c Release -o /app/publish --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RifaTech.API.dll"]
