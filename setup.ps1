try {
    Write-Host "Deleting Migrations folder if exists..."
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue ./Migrations

    Write-Host "Dropping existing database..."
    dotnet ef database drop --force --no-build

    Write-Host "Adding initial migration..."
    dotnet ef migrations add InitialCreate

    Write-Host "Updating database..."
    dotnet ef database update

    Write-Host "Running the app..."
    dotnet run # This will run Seeder.SeedAsync() and then start the application
}
catch {
    Write-Error "❌ An error occurred: $_"
    exit 1
}
