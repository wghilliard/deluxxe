```bash
cd ./src
docker compose up -d
dotnet vexxed.deluxxe raffle --config-file ./sample-config.json
```

dotnet tool commands

```bash
dotnet pack
dotnet tool update --add-source src/DeluxxeCli/nupkg DeluxxeCli
dotnet tool restore
dotnet vexxed.deluxxe validate-drivers -c 2025-rose-city-opener-xxxiii-ext-1.json
dotnet vexxed.deluxxe raffle -c 2025-rose-city-opener-xxxiii.json
```