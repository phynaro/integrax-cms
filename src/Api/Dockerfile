FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Shared/Shared.Kernel/Shared.Kernel.csproj", "src/Shared/Shared.Kernel/"]
COPY ["src/Modules/Identity/Identity.Core/Identity.Core.csproj", "src/Modules/Identity/Identity.Core/"]
COPY ["src/Modules/Clients/Clients.Core/Clients.Core.csproj", "src/Modules/Clients/Clients.Core/"]
COPY ["src/Modules/Portfolios/Portfolios.Core/Portfolios.Core.csproj", "src/Modules/Portfolios/Portfolios.Core/"]
COPY ["src/Modules/Audit/Audit.Core/Audit.Core.csproj", "src/Modules/Audit/Audit.Core/"]

RUN dotnet restore "src/Api/Api.csproj"

COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "Api.dll"]
