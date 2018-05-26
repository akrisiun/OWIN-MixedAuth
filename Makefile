help:
	@echo "cd | build"
cd:
	cd /mnt/d/Beta/Owin/OWIN-MixedAuth
cd-spa:
	cd /mnt/d/Beta/Owin/OWIN-MixedAuth/src/SPA
build:
	dotnet restore src/MixedAuth.sln
	dotnet build src/MixedAuth.sln
curl:    
	export http_proxy=
	curl "http://127.0.0.1:90/nuget/nuget/FindPackagesById()?id='Microsoft.AspNet.WebApi.WebHost'"