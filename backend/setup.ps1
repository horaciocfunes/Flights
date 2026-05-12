# Run from the backend/ directory to build and test the solution.
# Usage: cd backend; .\setup.ps1

Write-Host "Building..."
dotnet build

Write-Host ""
Write-Host "Running tests..."
dotnet test
