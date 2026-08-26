FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Bookings/Bookings.Api/Bookings.Api.csproj", "Bookings/Bookings.Api/"]
COPY ["Bookings/Bookings.Application/Bookings.Application.csproj", "Bookings/Bookings.Application/"]
COPY ["Bookings/Bookings.Domain/Bookings.Domain.csproj", "Bookings/Bookings.Domain/"]
COPY ["Bookings/Bookings.Infrastructure/Bookings.Infrastructure.csproj", "Bookings/Bookings.Infrastructure/"]
RUN dotnet restore "Bookings/Bookings.Api/Bookings.Api.csproj"

COPY . .
RUN dotnet publish "Bookings/Bookings.Api/Bookings.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bookings.Api.dll"]
