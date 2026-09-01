FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY src/k8s-streaming-lab-consumer/k8s-streaming-lab-consumer.csproj src/k8s-streaming-lab-consumer/
RUN dotnet restore src/k8s-streaming-lab-consumer/k8s-streaming-lab-consumer.csproj

COPY src/k8s-streaming-lab-consumer/ src/k8s-streaming-lab-consumer/
WORKDIR /build/src/k8s-streaming-lab-consumer
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "k8s-streaming-lab-consumer.dll"]