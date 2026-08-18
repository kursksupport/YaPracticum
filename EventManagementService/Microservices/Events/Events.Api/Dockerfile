FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Events/Events.Api/Events.Api.csproj", "Events/Events.Api/"]
COPY ["Events/Events.Application/Events.Application.csproj", "Events/Events.Application/"]
COPY ["Events/Events.Domain/Events.Domain.csproj", "Events/Events.Domain/"]
COPY ["Events/Events.Infrastructure/Events.Infrastructure.csproj", "Events/Events.Infrastructure/"]
RUN dotnet restore "Events/Events.Api/Events.Api.csproj"

COPY . .
RUN dotnet publish "Events/Events.Api/Events.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Events.Api.dll"]
