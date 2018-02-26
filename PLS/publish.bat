rm -rf bin obj
dotnet publish --self-contained --force -c Release -r win10-x64
7z a bin\pls.zip .\bin\Release\netcoreapp2.0\win10-x64\publish
