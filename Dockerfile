# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["UserManagementSystem/UserManagementSystem.csproj", "UserManagementSystem/"]
RUN dotnet restore "UserManagementSystem/UserManagementSystem.csproj"

# Copy all files and build
COPY . .
WORKDIR "/src/UserManagementSystem"
RUN dotnet build "UserManagementSystem.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "UserManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManagementSystem.dll"]
