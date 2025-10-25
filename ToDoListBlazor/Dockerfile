#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y libgdiplus

WORKDIR /app
EXPOSE 7060

ENTRYPOINT ["dotnet", "watch", "run"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ToDoListBlazor/ToDoListBlazor.csproj", "ToDoListBlazor/"]
COPY ["ToDoListCore/ToDoListCore.csproj", "ToDoListCore/"]
RUN dotnet restore "./ToDoListBlazor/ToDoListBlazor.csproj"
COPY . .
WORKDIR "/src/ToDoListBlazor"
#RUN dotnet build "./ToDoListBlazor.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet build "ToDoListBlazor.csproj" -c Release -o /app/build

FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./ToDoListBlazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet publish "ToDoListBlazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoListBlazor.dll"]

#WORKDIR /root
#RUN openssl x509 -inform DER -in /https-root/todolistroot.cer -out /https-root/todolistroot.crt
#RUN cp /https-root/todolistroot.crt /usr/local/share/ca-certificates/
#RUN update-ca-certificates




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
#COPY ["ToDoListBlazor/ToDoListBlazor.csproj", "ToDoListBlazor/"]
#COPY ["ToDoListCore/ToDoListCore.csproj", "ToDoListCore/"]
#COPY ["DAL/DAL.csproj", "DAL/"]
#RUN dotnet restore "./ToDoListBlazor/ToDoListBlazor.csproj"
#COPY . .
#WORKDIR "/src/ToDoListBlazor"
#RUN dotnet build "./ToDoListBlazor.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./ToDoListBlazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "ToDoListBlazor.dll"]