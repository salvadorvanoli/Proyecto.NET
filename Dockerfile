FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Web.Api/Web.Api.csproj", "src/Web.Api/"]
COPY ["src/Web.BackOffice/Web.BackOffice.csproj", "src/Web.BackOffice/"]
COPY ["src/Web.FrontOffice/Web.FrontOffice.csproj", "src/Web.FrontOffice/"]

RUN dotnet restore "src/Web.Api/Web.Api.csproj"
RUN dotnet restore "src/Web.BackOffice/Web.BackOffice.csproj"
RUN dotnet restore "src/Web.FrontOffice/Web.FrontOffice.csproj"

COPY ["src/", "src/"]

RUN dotnet build "src/Web.Api/Web.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build/api \
    --no-restore

RUN dotnet build "src/Web.BackOffice/Web.BackOffice.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build/backoffice \
    --no-restore

RUN dotnet build "src/Web.FrontOffice/Web.FrontOffice.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build/frontoffice \
    --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "src/Web.Api/Web.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish/api \
    /p:UseAppHost=false

RUN dotnet publish "src/Web.BackOffice/Web.BackOffice.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish/backoffice \
    /p:UseAppHost=false

RUN dotnet publish "src/Web.FrontOffice/Web.FrontOffice.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish/frontoffice \
    /p:UseAppHost=false

FROM base AS final-api
WORKDIR /app
COPY --from=publish /app/publish/api .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Web.Api.dll"]

FROM base AS final-backoffice
WORKDIR /app
COPY --from=publish /app/publish/backoffice .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Web.BackOffice.dll"]

FROM base AS final-frontoffice
WORKDIR /app
COPY --from=publish /app/publish/frontoffice .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Web.FrontOffice.dll"]
