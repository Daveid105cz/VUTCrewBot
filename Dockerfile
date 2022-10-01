#Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.16-arm64v8 AS build

WORKDIR /source
COPY "./VUTCrewBot/VUTCrewBot.csproj" "./VUTCrewBot/VUTCrewBot.csproj"
COPY "./VUTCrewBot.DAL/VUTCrewBot.DAL.csproj" "./VUTCrewBot.DAL/VUTCrewBot.DAL.csproj"
RUN dotnet restore "./VUTCrewBot/VUTCrewBot.csproj" --disable-parallel
RUN dotnet restore "./VUTCrewBot.DAL/VUTCrewBot.DAL.csproj" --disable-parallel

COPY . .
RUN dotnet publish "./VUTCrewBot/VUTCrewBot.csproj" -c release -o /app --no-restore

#Serve stage

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine3.16-arm64v8
WORKDIR /bot
COPY --from=build /app ./

RUN apk add --no-cache tzdata
ENV TZ=Europe/Prague
ENV LANG cs_CZ.UTF-8
ENV LANGUAGE ${LANG}
ENV LC_ALL ${LANG}
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["dotnet","VUTCrewBot.dll"]