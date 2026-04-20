FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DotNetTask.Api/DotNetTask.Api.csproj", "DotNetTask.Api/"]
COPY ["DotNetTask.Application/DotNetTask.Application.csproj", "DotNetTask.Application/"]
COPY ["DotNetTask.Domain/DotNetTask.Domain.csproj", "DotNetTask.Domain/"]
COPY ["DotNetTask.Infrastructure/DotNetTask.Infrastructure.csproj", "DotNetTask.Infrastructure/"]

RUN dotnet restore "DotNetTask.Api/DotNetTask.Api.csproj"

COPY . .
WORKDIR /src/DotNetTask.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DotNetTask.Api.dll"]