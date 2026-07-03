# Use the SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all project files first to restore dependencies (leverages Docker cache)
COPY ["IPCS-API/IPCS-API.csproj", "IPCS-API/"]
COPY ["IPCS-Model/IPCS-Model.csproj", "IPCS-Model/"]
COPY ["IPCS-Repo/IPCS-Repo.csproj", "IPCS-Repo/"]
COPY ["IPCS-Service/IPCS-Service.csproj", "IPCS-Service/"]

RUN dotnet restore "IPCS-API/IPCS-API.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the release
WORKDIR "/src/IPCS-API"
RUN dotnet publish "IPCS-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET runtime image for final execution
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080 (default for .NET 8/9 container images)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "IPCS-API.dll"]
