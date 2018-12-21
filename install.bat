dotnet pack -c Release --output ./
dotnet tool uninstall -g Alcuin.PLS
dotnet tool install -g Alcuin.PLS --version 1.0.0 --add-source ./