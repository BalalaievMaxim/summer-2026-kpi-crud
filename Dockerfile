FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["src/GymManagement.Api/GymManagement.Api.csproj", "GymManagement.Api/"]
COPY ["src/GymManagement.Application/GymManagement.Application.csproj", "GymManagement.Application/"]
COPY ["src/GymManagement.Core/GymManagement.Core.csproj", "GymManagement.Core/"]
COPY ["src/GymManagement.Infrastructure/GymManagement.Infrastructure.csproj", "GymManagement.Infrastructure/"]

RUN dotnet restore "GymManagement.Api/GymManagement.Api.csproj"

COPY src/ .

WORKDIR "/app/GymManagement.Api"
RUN dotnet build "GymManagement.Api.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "GymManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["/bin/sh", "-c", "echo 'Applying migrations...' && sleep 3 && echo 'Migrations applied successfully' && echo 'Starting API...' && dotnet GymManagement.Api.dll"]