FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MovieCatalog.Shared/MovieCatalog.Shared.csproj", "MovieCatalog.Shared/"]
COPY ["MovieCatalog.Domain/MovieCatalog.Domain.csproj", "MovieCatalog.Domain/"]
COPY ["MovieCatalog.Application/MovieCatalog.Application.csproj", "MovieCatalog.Application/"]
COPY ["MovieCatalog.Infra/MovieCatalog.Infra.csproj", "MovieCatalog.Infra/"]
COPY ["MovieCatalog.Api/MovieCatalog.Api.csproj", "MovieCatalog.Api/"]
RUN dotnet restore "MovieCatalog.Api/MovieCatalog.Api.csproj"

COPY MovieCatalog.Shared/ MovieCatalog.Shared/
COPY MovieCatalog.Domain/ MovieCatalog.Domain/
COPY MovieCatalog.Application/ MovieCatalog.Application/
COPY MovieCatalog.Infra/ MovieCatalog.Infra/
COPY MovieCatalog.Api/ MovieCatalog.Api/
WORKDIR "/src/MovieCatalog.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MovieCatalog.Api.dll"]
