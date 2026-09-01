FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY src/k8s-streaming-lab-producer/k8s-streaming-lab-producer.csproj src/k8s-streaming-lab-producer/
RUN dotnet restore src/k8s-streaming-lab-producer/k8s-streaming-lab-producer.csproj

COPY src/k8s-streaming-lab-producer/ src/k8s-streaming-lab-producer/
WORKDIR /build/src/k8s-streaming-lab-producer
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "k8s-streaming-lab-producer.dll"]