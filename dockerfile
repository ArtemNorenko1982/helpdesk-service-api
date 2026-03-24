FROM mcr.microsoft.com/dotmnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HelpdeskService.API/HelpdeskService.API.csproj", "HelpdeskService.API/"]
COPY ["HelpdeskService.Core/HelpdeskService.Core.csproj", "HelpdeskService.Core/"]
COPY ["HelpdeskService.Data/HelpdeskService.Data.csproj", "HelpdeskService.Data/"]

RUN dotnet restore "HelpdeskService.API/HelpdeskService.API.csproj"

COPY . .

RUN dotnet build "HelpdeskService.API/HelpdeskService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnetpublish "HelpdeskService.API/HelpdeskService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "HelpdeskService.API.dll"]