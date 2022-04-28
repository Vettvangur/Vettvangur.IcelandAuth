# Azure DevOps should one day handle publishing
nuget restore src
dotnet pack ./src/libraries/Vettvangur.IcelandAuth/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
dotnet pack ./src/libraries/Vettvangur.IcelandAuth.Umbraco7/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
dotnet pack ./src/libraries/Vettvangur.IcelandAuth.Umbraco8/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
dotnet pack ./src/libraries/Vettvangur.IcelandAuth.Umbraco9/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
dotnet pack ./src/libraries/Vettvangur.IcelandAuth.Owin/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
# nuget push *.nupkg -Source https://api.nuget.org/v3/index.json -NonInteractive -SkipDuplicate
pause
