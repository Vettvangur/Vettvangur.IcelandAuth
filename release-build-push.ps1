nuget restore .
nuget pack .\src\Vettvangur.IcelandAuth\ -build -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release
nuget pack .\src\Vettvangur.IcelandAuth.Umbraco7\ -build -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release
nuget pack .\src\Vettvangur.IcelandAuth.Umbraco8\ -build -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release
# $pkg = gci *.nupkg 
# nuget push $pkg -Source https://www.nuget.org/api/v2/package -NonInteractive
