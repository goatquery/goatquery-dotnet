build:
	dotnet build ./src/GoatQuery/src --configuration Release
	dotnet build ./src/GoatQuery.AspNetCore/src --configuration Release

package:
	dotnet pack ./src/GoatQuery/src --configuration Release -p:Version=0.0.1-local
	dotnet pack ./src/GoatQuery.AspNetCore/src --configuration Release -p:Version=0.0.1-local

publish-local:
	mkdir -p ~/.nuget/local-packages
	dotnet nuget push ./src/GoatQuery/src/bin/Release/GoatQuery.0.0.1-local.nupkg -s ~/.nuget/local-packages
	dotnet nuget push ./src/GoatQuery.AspNetCore/src/bin/Release/GoatQuery.AspNetCore.0.0.1-local.nupkg -s ~/.nuget/local-packages

