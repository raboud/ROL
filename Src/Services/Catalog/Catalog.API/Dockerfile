#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-nanoserver-1903 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-nanoserver-1903 AS build
WORKDIR /src
COPY ["Src/Services/Catalog/Catalog.API/Catalog.API.csproj", "Src/Services/Catalog/Catalog.API/"]
COPY ["Src/Services/Common/Common.API/Common.API.csproj", "Src/Services/Common/Common.API/"]
COPY ["Src/Services/Catalog/Catalog.DTO/Catalog.DTO.csproj", "Src/Services/Catalog/Catalog.DTO/"]
COPY ["Src/Services/Catalog/Catalog.DAL/Catalog.DAL.csproj", "Src/Services/Catalog/Catalog.DAL/"]
COPY ["Src/Services/Catalog/Catalog.Domain/Catalog.Domain.csproj", "Src/Services/Catalog/Catalog.Domain/"]
COPY ["Src/Services/Common/Common.DTO/Common.DTO.csproj", "Src/Services/Common/Common.DTO/"]
RUN dotnet restore "Src/Services/Catalog/Catalog.API/Catalog.API.csproj"
COPY . .
WORKDIR "/src/Src/Services/Catalog/Catalog.API"
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]