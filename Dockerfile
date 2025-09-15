# -------------------------------
# Stage 1: Build backend
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy solution & backend csproj
COPY VetApi.Deployment.sln .
COPY backpipe/VetApi/VetApi.csproj ./backpipe/VetApi/

# Restore dependencies
RUN dotnet restore VetApi.Deployment.sln

# Copy backend source
COPY backpipe/VetApi/ ./backpipe/VetApi/

# Build & publish backend
RUN dotnet publish backpipe/VetApi -c Release -o /app/publish

# -------------------------------
# Stage 2: Build frontend
# -------------------------------
FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend

# Copy package files
COPY frontend/package*.json ./

# Install dependencies
RUN npm ci

# Copy frontend source
COPY frontend/ ./

# Build frontend using Vite directly (ignores scripts in package.json)
RUN npx vite build --mode production

# -------------------------------
# Stage 3: Final image
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy backend
COPY --from=backend-build /app/publish .

# Copy frontend build output to wwwroot
COPY --from=frontend-build /src/frontend/dist ./wwwroot

# Expose port 80
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Start backend
ENTRYPOINT ["dotnet", "VetApi.dll"]
