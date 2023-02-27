#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BreganTwitchBot.Core/BreganTwitchBot.Core.csproj", "BreganTwitchBot.Core/"]
COPY ["BreganTwitchBot.Domain/BreganTwitchBot.Domain.csproj", "BreganTwitchBot.Domain/"]
COPY ["BreganTwitchbot.Infrastructure/BreganTwitchBot.Infrastructure.csproj", "BreganTwitchbot.Infrastructure/"]
RUN dotnet restore "BreganTwitchBot.Core/BreganTwitchBot.Core.csproj"
COPY . .
WORKDIR "/src/BreganTwitchBot.Core"
RUN dotnet build "BreganTwitchBot.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BreganTwitchBot.Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT /app/ProgressBars/twitchbot.sh