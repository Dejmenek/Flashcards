FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY Flashcards.sln ./
COPY Flashcards/Flashcards.csproj Flashcards/
RUN dotnet restore

# Copy the rest of the application code
COPY Flashcards/. Flashcards/
WORKDIR /src/Flashcards
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# Copy the published application from the build stage
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Flashcards.dll"]