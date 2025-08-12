FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution and props files
COPY Flashcards.sln ./
COPY Directory.Packages.props ./

# Copy project files
COPY Flashcards/Flashcards.csproj Flashcards/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the application code
COPY Flashcards/. Flashcards/
WORKDIR /src/Flashcards
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
# Copy the published application from the build stage
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Flashcards.dll"]