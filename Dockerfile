FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine

# Base Development Packages
RUN apk update \
  && apk upgrade \
  && apk add ca-certificates wget && update-ca-certificates \
  && apk add --no-cache --update \
  git \
  curl \
  wget \
  bash \
  make \
  rsync \
  nano

WORKDIR /Bumbershoot

COPY src/Bumbershoot.Utilities/*.csproj ./src/Bumbershoot.Utilities/


WORKDIR /Bumbershoot/src/Bumbershoot.Utilities
RUN dotnet restore

# Working Folder
WORKDIR /Bumbershoot
ENV TERM xterm-256color
RUN printf 'export PS1="\[\e[0;34;0;33m\][DCKR]\[\e[0m\] \\t \[\e[40;38;5;28m\][\w]\[\e[0m\] \$ "' >> ~/.bashrc
