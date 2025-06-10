FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

RUN apt-get update \
 && apt-get install -y curl gnupg2 unixodbc-dev software-properties-common \
 && curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg \
 && install -o root -g root -m 644 microsoft.gpg /etc/apt/trusted.gpg.d/ \
 && curl -sSL https://packages.microsoft.com/config/ubuntu/20.04/prod.list \
    > /etc/apt/sources.list.d/mssql-release.list \
 && apt-get update \
 && ACCEPT_EULA=Y apt-get install -y mssql-tools \
 && rm -rf /var/lib/apt/lists/* /microsoft.gpg

ENV PATH="${PATH}:/opt/mssql-tools/bin"

USER mssql
