nuget restore src
dotnet pack ./src/Vettvangur.IcelandAuth/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
dotnet pack ./src\Vettvangur.IcelandAuth.Umbraco7/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
dotnet pack ./src\Vettvangur.IcelandAuth.Umbraco8/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
dotnet pack ./src\Vettvangur.IcelandAuth.Owin/ -t:Build,Pack -p:Configuration=Release --include-symbols -o .
nuget push *.nupkg -Source https://api.nuget.org/v3/index.json -NonInteractive -SkipDuplicate
pause
