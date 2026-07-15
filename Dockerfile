# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# 1) Frontend build aşaması (Vite + React)
# ---------------------------------------------------------------------------
FROM node:20-alpine AS frontend
WORKDIR /ui

# Önce yalnızca bağımlılık manifestlerini kopyala (katman cache'i için)
COPY NisanDavetiye.UI/package.json NisanDavetiye.UI/package-lock.json ./
RUN npm ci

# UI kaynaklarını kopyala ve production build al (çıktı: /ui/dist)
COPY NisanDavetiye.UI/ ./
RUN npm run build

# ---------------------------------------------------------------------------
# 2) .NET build/publish aşaması (yalnızca API + BLL + DAL)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Solution ve gerekli proje dosyalarını önce kopyala (restore cache'i için).
# NisanDavetiye.UI.Runner bilinçli olarak dahil edilmez.
COPY NisanDavetiye.sln ./
COPY NisanDavetiye.API/NisanDavetiye.API.csproj NisanDavetiye.API/
COPY NisanDavetiye.BLL/NisanDavetiye.BLL.csproj NisanDavetiye.BLL/
COPY NisanDavetiye.DAL/NisanDavetiye.DAL.csproj NisanDavetiye.DAL/

# API csproj'unu restore et; proje referansları (BLL, DAL) otomatik gelir.
RUN dotnet restore NisanDavetiye.API/NisanDavetiye.API.csproj

# Yalnızca gerekli projelerin kaynaklarını kopyala.
COPY NisanDavetiye.API/ NisanDavetiye.API/
COPY NisanDavetiye.BLL/ NisanDavetiye.BLL/
COPY NisanDavetiye.DAL/ NisanDavetiye.DAL/

# API'yi Release modunda publish et (self-contained app host'a gerek yok).
RUN dotnet publish NisanDavetiye.API/NisanDavetiye.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# Frontend build çıktısını API'nin wwwroot klasörüne yerleştir.
COPY --from=frontend /ui/dist /app/publish/wwwroot

# ---------------------------------------------------------------------------
# 3) Runtime aşaması (ASP.NET Core 9.0)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./

EXPOSE 8080

# Railway'in verdiği PORT değişkenini dinle; yoksa 8080'e düş.
CMD ["sh", "-c", "dotnet NisanDavetiye.API.dll --urls http://0.0.0.0:${PORT:-8080}"]
