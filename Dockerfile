FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG TARGETPLATFORM
WORKDIR /app
COPY . ./
RUN <<EOF
#!/bin/bash
echo "TARGETPLATFROM=$TARGETPLATFORM"
apt update
apt install -y build-essential
if [[ "$TARGETPLATFORM" = "linux/amd64" ]]
then
  dotnet publish Libplanet.Store.Remote.Executable.csproj \
    -c Release \
    -r linux-x64 \
    -o out \
    --self-contained
elif [[ "$TARGETPLATFORM" = "linux/arm64" ]]
then
  dotnet publish Libplanet.Store.Remote.Executable.csproj \
    -c Release \
    -r linux-arm64 \
    -o out \
    --self-contained
else
  echo "Not supported target platform: '$TARGETPLATFORM'."
  exit -1
fi
EOF

# Build runtime image
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
WORKDIR /app
COPY --from=build-env /app/out .

# Install native deps & utilities for production
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev liblz4-dev zlib1g-dev libsnappy-dev libzstd-dev jq curl \
     && rm -rf /var/lib/apt/lists/*

VOLUME /data

ENTRYPOINT ["dotnet", "Libplanet.Store.Remote.Executable.dll"]
