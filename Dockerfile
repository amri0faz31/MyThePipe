# Build stage for backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy deployment solution instead of full solution
COPY VetApi.Deployment.sln .
COPY backpipe/VetApi/VetApi.csproj ./backpipe/VetApi/

RUN dotnet restore VetApi.Deployment.sln

# Copy backend source
COPY backpipe/VetApi/ ./backpipe/VetApi/

# Build and publish backend
RUN dotnet publish backpipe/VetApi -c Release -o /app/publish

# Build stage for frontend  
FROM node:20-alpine AS frontend-build
WORKDIR /src

# Copy frontend files
COPY frontend/package*.json ./frontend/
RUN cd frontend && npm ci

COPY frontend/ ./frontend/
# ✅ VITE_API_BASE from .env.production is used here
RUN cd frontend && npm run build

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy backend
COPY --from=backend-build /app/publish .

# Copy frontend build into wwwroot (served by ASP.NET)
COPY --from=frontend-build /src/frontend/dist ./wwwroot

# Azure App Service looks for this port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "VetApi.dll"]