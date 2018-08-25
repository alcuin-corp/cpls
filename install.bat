dotnet pack --output ./
dotnet tool uninstall -g Alcuin.PLS
dotnet tool install -g Alcuin.PLS --add-source ./
