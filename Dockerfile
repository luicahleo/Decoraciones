FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Decorations.Web/Decorations.Web.csproj", "src/Decorations.Web/"]
COPY ["src/Decorations.Application/Decorations.Application.csproj", "src/Decorations.Application/"]
COPY ["src/Decorations.Domain/Decorations.Domain.csproj", "src/Decorations.Domain/"]
COPY ["src/Decorations.Infrastructure/Decorations.Infrastructure.csproj", "src/Decorations.Infrastructure/"]

RUN dotnet restore "src/Decorations.Web/Decorations.Web.csproj"

COPY . .

WORKDIR "/src/src/Decorations.Web"
RUN dotnet publish "Decorations.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p /app/wwwroot/uploads

ENTRYPOINT ["dotnet", "Decorations.Web.dll"]
