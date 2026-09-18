FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Notification.Worker/Notification.Worker.csproj", "Notification.Worker/"]
RUN dotnet restore "Notification.Worker/Notification.Worker.csproj"

COPY Notification.Worker/ Notification.Worker/
WORKDIR "/src/Notification.Worker"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
