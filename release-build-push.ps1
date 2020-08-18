nuget restore .
dotnet pack ./src/Vettvangur.IcelandAuth/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
nuget pack .\src\Vettvangur.IcelandAuth.Umbraco7\ -build -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release
nuget pack .\src\Vettvangur.IcelandAuth.Umbraco8\ -build -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release
nuget push *.nupkg -Source https://www.nuget.org/api/v2/package -NonInteractive -SkipDuplicate
pause
