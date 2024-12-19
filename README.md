# Libplanet.Store.Remote.Executable

A server application that provides a remote interface to the `IKeyValueStore` interface by gRPC.

## Usage

```text
dotnet run -- <path> --port <port>
```

If you want to use Docker, you can use the following command:

```text
docker run -v <path>:/data -p <port>:80 planetarium/libplanet-remote-kv -- /data --port <port>
```

See the [Docker Hub page](https://hub.docker.com/repository/docker/planetariumhq/libplanet-remote-kv/general) for more information.

## Build

```text
dotnet build
```

To build Docker image:

```text
docker buildx build --builder=<builder> --platform linux/amd64,linux/arm64 .
```

## License

This project is licensed under the AGPL 3 License - see the [LICENSE](LICENSE) file for details.
