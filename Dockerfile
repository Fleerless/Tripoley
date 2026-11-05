# syntax=docker/dockerfile:1

# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore against only the csproj files first so this layer is cached
# until a project file actually changes.
COPY Tripoley.Engine/Tripoley.Engine.csproj Tripoley.Engine/
COPY Tripoley.Api/Tripoley.Api.csproj Tripoley.Api/
RUN dotnet restore Tripoley.Api/Tripoley.Api.csproj

# Copy the sources the API needs and publish a framework-dependent build.
COPY Tripoley.Engine/ Tripoley.Engine/
COPY Tripoley.Api/ Tripoley.Api/
RUN dotnet publish Tripoley.Api/Tripoley.Api.csproj -c Release -o /app --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# App Runner forwards requests to this port; be explicit rather than
# relying on the base image default.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run as the non-root user baked into the aspnet image.
USER $APP_UID

ENTRYPOINT ["dotnet", "Tripoley.Api.dll"]
