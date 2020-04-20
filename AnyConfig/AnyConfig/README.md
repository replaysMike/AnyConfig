# Build notes

The release build of AnyConfig uses ILRepack to merge depencencies into the final assembly. This prevents issues with 
conflicting dependency versions of common libraries (especially Microsoft ones).

## Suppressing Nuget dependencies

When using ILRepack to merge dependencies the problem still occurs of `.nuspec` files being generated that point to the 
dependencies that were packed. The solution is to set `PrivateAssets="all"` on each PackageReference. This will break the
release build in VS however it will suppress the generation of dependencies in the final `.nuspec`

## Resources

(Using ILRepack as a task)[https://www.meziantou.net/merging-assemblies-using-ilrepack.htm]
(Supressing Nuget dependencies with dotnet pack)[https://github.com/NuGet/Home/issues/6354]