# Stage 1: Build stage (SDK + AOT prerequisites)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install Native AOT prerequisites for Linux (clang and zlib headers)
# Using clang as recommended by .NET docs for linux AOT
RUN apt-get update && apt-get install -y clang zlib1g-dev --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy csproj and restore dependencies first for layer caching
# Restore specifically for the target runtime identifier
COPY ["ExpenseLoggerApi/ExpenseLoggerApi.csproj", "ExpenseLoggerApi/"]
RUN dotnet restore "ExpenseLoggerApi/ExpenseLoggerApi.csproj" -r linux-x64

# Copy the rest of the source code
COPY ["ExpenseLoggerApi/", "ExpenseLoggerApi/"]
WORKDIR /src/ExpenseLoggerApi

# Publish the application using Native AOT for linux-arm64
RUN dotnet publish "ExpenseLoggerApi.csproj" \
    -r linux-amd64 \
    -c Release \
    -o /app/publish \
    /p:PublishAot=true \
    /p:InvariantGlobalization=true

# Stage 2: Final runtime stage (Minimal dependencies for ARM64)
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy AS final

WORKDIR /app

# Copy the published native executable and related files
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5001
EXPOSE 5001

# IMPORTANT: Entrypoint is the native executable (usually project name)
# Check the /app/publish directory in the build stage if unsure of the exact name
ENTRYPOINT ["./ExpenseLoggerApi"]