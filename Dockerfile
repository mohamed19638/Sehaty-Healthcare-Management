FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["i-am-building-a-simple-graduation.csproj", "./"]
RUN dotnet restore "i-am-building-a-simple-graduation.csproj"

COPY . .
RUN dotnet publish "i-am-building-a-simple-graduation.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "i-am-building-a-simple-graduation.dll"]

