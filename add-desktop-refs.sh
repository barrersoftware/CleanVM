#!/bin/bash
set -e

echo "🖥️ Adding Desktop project references..."

# For now, Desktop will use .NET 9 and we'll create a bridge
# OR we make Core/Enterprise multi-target

# Option 1: Make Core and Enterprise multi-target (net9.0;net10.0)
echo "Making Core multi-target..."
cd src/CleanVM.Core

# Backup original
cp CleanVM.Core.csproj CleanVM.Core.csproj.bak

# Update to multi-target
cat > CleanVM.Core.csproj << 'CORE'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0;net10.0</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
CORE

echo "Making Enterprise multi-target..."
cd ../CleanVM.Enterprise

# Backup original
cp CleanVM.Enterprise.csproj CleanVM.Enterprise.csproj.bak

# Update to multi-target
cat > CleanVM.Enterprise.csproj << 'ENT'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0;net10.0</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../CleanVM.Core/CleanVM.Core.csproj" />
  </ItemGroup>
</Project>
ENT

cd ~/CleanVM

# Now add Desktop references
echo "Adding references to Desktop..."
dotnet add src/CleanVM.Desktop/CleanVM.Desktop.csproj reference src/CleanVM.Core/CleanVM.Core.csproj
dotnet add src/CleanVM.Desktop/CleanVM.Desktop.csproj reference src/CleanVM.Enterprise/CleanVM.Enterprise.csproj

# Rebuild everything
echo ""
echo "🔨 Building solution..."
dotnet build --nologo

echo ""
echo "✅ Desktop integrated!"
dotnet sln list
