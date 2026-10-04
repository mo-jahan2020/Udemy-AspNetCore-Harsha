FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# کپی فایل‌های پروژه و restore
COPY src/TextToSqlApi/TextToSqlApi.csproj ./TextToSqlApi.csproj
RUN dotnet restore

# کپی کل سورس و build
COPY src/TextToSqlApi/ ./
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# پورت پیش‌فرض
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# لاگ‌ها به stdout
ENV Logging__Console__Formatter=Json

ENTRYPOINT ["dotnet", "TextToSqlApi.dll"]
