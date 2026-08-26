FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Users/Users.Api/Users.Api.csproj", "Users/Users.Api/"]
COPY ["Users/Users.Application/Users.Application.csproj", "Users/Users.Application/"]
COPY ["Users/Users.Domain/Users.Domain.csproj", "Users/Users.Domain/"]
COPY ["Users/Users.Infrastructure/Users.Infrastructure.csproj", "Users/Users.Infrastructure/"]
RUN dotnet restore "Users/Users.Api/Users.Api.csproj"

COPY . .
RUN dotnet publish "Users/Users.Api/Users.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Users.Api.dll"]
