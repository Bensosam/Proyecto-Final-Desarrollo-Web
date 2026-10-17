FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Clinica.API/Clinica.API.csproj", "Clinica.API/"]
RUN dotnet restore "Clinica.API/Clinica.API.csproj"

COPY . .

WORKDIR "/src/Clinica.API"
RUN dotnet publish "Clinica.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Clinica.API.dll"]