```bash
cd ./src
docker compose up
dotnet vexxed.deluxxe raffle --config-file ./sample-config.json
```

dotnet tool commands

```bash
dotnet pack
dotnet tool update --add-source src/DeluxxeCli/nupkg DeluxxeCli
dotnet tool restore
dotnet vexxed.deluxxe raffle -c ./2025-04-25-portland/2025-rose-city-opener-xxxiii.json
```