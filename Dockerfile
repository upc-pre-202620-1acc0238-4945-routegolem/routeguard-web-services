FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app

# Copy csproj and restore
COPY RouteGuard.Platform/*.csproj RouteGuard.Platform/
RUN dotnet restore ./RouteGuard.Platform

# Copy everything else and build
COPY . .
RUN dotnet publish ./RouteGuard.Platform -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/out .
# Listen on 8080 (set WEBSITES_PORT=8080 in Azure App Service)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RouteGuard.Platform.dll"]
