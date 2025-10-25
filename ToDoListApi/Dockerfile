#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 443
ENTRYPOINT ["dotnet", "watch", "run"]
#
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["ToDoListApi/ToDoListApi.csproj", "ToDoListApi/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["ToDoListCore/ToDoListCore.csproj", "ToDoListCore/"]

RUN dotnet restore "ToDoListApi/ToDoListApi.csproj"
COPY . .
WORKDIR "/src/ToDoListApi"
RUN dotnet build "ToDoListApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ToDoListApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoListApi.dll"]

#WORKDIR /root
#RUN openssl x509 -inform DER -in /https-root/todolistroot.cer -out /https-root/todolistroot.crt
#RUN cp /https-root/todolistroot.crt /usr/local/share/ca-certificates/
#RUN update-ca-certificates
#


#WORKDIR /root
#ENTRYPOINT ["./app/docker-entrypoint.sh"]
#ENTRYPOINT ["/docker-entrypoint.sh"]
#





##See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["ToDoListApi/ToDoListApi.csproj", "ToDoListApi/"]
#COPY ["DAL/DAL.csproj", "DAL/"]
#COPY ["ToDoListCore/ToDoListCore.csproj", "ToDoListCore/"]
#RUN dotnet restore "./ToDoListApi/ToDoListApi.csproj"
#COPY . .
#WORKDIR "/src/ToDoListApi"
#RUN dotnet build "./ToDoListApi.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./ToDoListApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "ToDoListApi.dll"]