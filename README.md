![GitHub release (latest by date)](https://img.shields.io/github/v/release/PacificEngine/OW_CommonResources?style=flat-square)
![GitHub Release Date](https://img.shields.io/github/release-date/PacificEngine/OW_CommonResources?label=last%20release&style=flat-square)
![GitHub all releases](https://img.shields.io/github/downloads/PacificEngine/OW_CommonResources/total?style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/downloads/PacificEngine/OW_CommonResources/latest/total?style=flat-square)

# Using Mod
A bunch of common resources used in downstream mods

# Creating Code
Create a new file called `PacificEngine.OW_CommonResources.csproj.user`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <OuterWildsRootDirectory>$(OuterWildsDir)\Outer Wilds</OuterWildsRootDirectory>
    <OuterWildsModsDirectory>%AppData%\OuterWildsModManager\OWML\Mods</OuterWildsModsDirectory>
  </PropertyGroup>
</Project>
```
