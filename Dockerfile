FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/GymManagement.Api/GymManagement.Api.csproj", "GymManagement.Api/"]
COPY ["src/GymManagement.Application/GymManagement.Application.csproj", "GymManagement.Application/"]
COPY ["src/GymManagement.Core/GymManagement.Core.csproj", "GymManagement.Core/"]
COPY ["src/GymManagement.Infrastructure/GymManagement.Infrastructure.csproj", "GymManagement.Infrastructure/"]
RUN dotnet restore "GymManagement.Api/GymManagement.Api.csproj"

COPY src/ .
WORKDIR "/src/GymManagement.Api"
RUN dotnet publish "GymManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "GymManagement.Api.dll"]