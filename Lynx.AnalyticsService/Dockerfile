# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy project files
COPY ["src/Lynx.AnalyticsService.Api/Lynx.AnalyticsService.Api.csproj", "src/Lynx.AnalyticsService.Api/"]
COPY ["src/Lynx.AnalyticsService.Application/Lynx.AnalyticsService.Application.csproj", "src/Lynx.AnalyticsService.Application/"]
COPY ["src/Lynx.AnalyticsService.Domain/Lynx.AnalyticsService.Domain.csproj", "src/Lynx.AnalyticsService.Domain/"]
COPY ["src/Lynx.AnalyticsService.Infrastructure/Lynx.AnalyticsService.Infrastructure.csproj", "src/Lynx.AnalyticsService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Lynx.AnalyticsService.Api/Lynx.AnalyticsService.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
# TODO: Change Debug to Realease in production
RUN dotnet publish "src/Lynx.AnalyticsService.Api/Lynx.AnalyticsService.Api.csproj" -c Debug -o /app

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y tzdata && \
    ln -fs /usr/share/zoneinfo/America/Montreal /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    rm -rf /var/lib/apt/lists/*

ENV TZ=America/Montreal
# TODO: Remove this line in production
ENV ASPNETCORE_ENVIRONMENT=development

WORKDIR /app
COPY --from=build /app .
EXPOSE 8082
ENV ASPNETCORE_URLS=http://+:8082
ENTRYPOINT [ "dotnet", "Lynx.AnalyticsService.Api.dll" ]
