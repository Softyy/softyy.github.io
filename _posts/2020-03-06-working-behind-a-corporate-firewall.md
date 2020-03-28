---
layout: post
title: "Working Behind A Corporate Firewall With Open Source"
categories: Docker
author: "Christopher J. Adkins"
tags: [Docker, Proxy, Corporate, Python, Node, .NET]
usemathjax: false
comments: true
---

When I first started working with open source most the tools just worked! Life was wonderful on my personal machine and various package managers were easy to use. Then the big bad corporate firewall came into play and I had no idea what I was doing anymore, tools that used to work we're encountering SSL/TLS errors and connections weren't happening.

If you already know what you're doing... this will be obvious to you. If you're struggling with setting up environments behind your firewall, hopefully you find something useful here.

I'll go over a few modifications to base Docker images that allow connections to private (corporate) and public package repositories for Python, Node, .NET and Debian.

[Link to repo files here](https://github.com/Softyy/docker-images-with-proxies)

## Python (pip)

The main package manager folks use here is pip (conda is similar). So we'll need a `pip.conf` file on Linux, or a `pip.ini` file on Windows. [For full reference click this link](https://pip.pypa.io/en/stable/user_guide/#config-file). For Debian, it's nice to the proxy configs to `apt` as well with `apt.conf`. If you want to add a private repo for `apt`, I'd probably use `add-apt-repository` instead of adding the settings manually. Ok, so together we create these two files

```bash
# pip.conf
[global]
# the default index-url is https://pypi.python.org/simple
# fill in the blanks below to the location of your private repo (maybe you need to authenticate with it, [*] means optional)
index-url = https://[<username>:<password>@]<index_host>:<index_port>/<route_to_repo_index>

# if you need a proxy to access the index-url, you'll need to pass the proxy server details here
# this is equivalent to pip install --proxy http://[<username>:<password>@]<proxy_host>:<proxy_port> <package_name>
proxy = http://[<username>:<password>@]<proxy_host>:<proxy_port>

# also, if you're mucking around with some self-signed certs, don't forget to trust that host for good measure.
trusted-host = <host>
```

```bash
# apt.conf
Acquire::http::proxy "http://[<username>:<password>@]<proxy_host>:<proxy_port>"
Acquire::https::proxy "http://[<username>:<password>@]<proxy_host>:<proxy_port>"
```

```bash
# .curlrc
proxy = http://[<username>:<password>@]<proxy_host>:<proxy_port>
```

With this two files, we can create a python base Dockerfile so any internal info to server references can be left outside future Dockerfiles. i.e, if we host the following docker image internally

```Dockerfile
ARG PYTHON_VERSION=3.7

FROM python:${PYTHON_VERSION}

LABEL python_version=${PYTHON_VERSION}

COPY pip.conf /etc/pip.conf
COPY apt.conf /etc/apt/apt.conf
COPY .curlrc /root/.curlrc

CMD ["bash"]
```

I can build an app internally with docker, but also build it externally if something changes down the road and we open source the image or something.

```Dockerfile
ARG PYTHON_VERSION=3.7

#FROM python:${PYTHON_VERSION}
FROM <python_internal>:${PYTHON_VERSION}

RUN apt-get update
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN pip install pandas

...
```

Don't forget to be mindful of if you're installing from internal `apt` repo's or `curl`ing from an internal host. Then you're not really doing a fully open source solution internally, it requires slightly more setup at the repo file level.

## Node (npm)

Now-a-days most tools follow similar patterns, so all we'll have to change for Node is the npm configs! Create a `.npmrc` file.

```bash
# .npmrc

# the default registry is https://registry.npmjs.org/
# fill in the blanks below to the location of your private repo (maybe you need to authenticate with it, [*] means optional)
registry=https://[<username>:<password>@]<index_host>:<index_port>/<route_to_repo_index>

# if you need a proxy to access the index-url, you'll need to pass the proxy server details here
# this is equivalent to npm install --proxy http://[<username>:<password>@]<proxy_host>:<proxy_port> <package_name>
proxy=http://[<username>:<password>@]<proxy_host>:<proxy_port>
https-proxy=http://[<username>:<password>@]<proxy_host>:<proxy_port>

# also, if you're mucking around with some self-signed certs, you'll have to ignore them or...
strict-ssl=false

# you can add the certs of all the hosts you'd like to trust like follows
# ca[]="cert 1 base64 string"
# ca[]="cert 2 base64 string"
```

Equipped with our configs, we can create a node base image by following the same logic as before

```Dockerfile
ARG NODE_VERSION=13.10

FROM node:${NODE_VERSION}

LABEL node_version=${NODE_VERSION}

COPY .npmrc /root/.npmrc
COPY apt.conf /etc/apt/apt.conf
COPY .curlrc /root/.curlrc

CMD ["bash"]
```

Now you can use all the commands without worrying about the proxy!

## .NET (NuGet)

Even with Microsoft, it's the same story with .NET Core! Just create `NuGet.Config` with the following:

```XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="dependencyVersion" value="Highest" />
    <add key="https_proxy" value="http://[<username>:<password>@]<proxy_host>:<proxy_port>" />
    <add key="http_proxy" value="http://[<username>:<password>@]<proxy_host>:<proxy_port>" />
  </config>
  <packageSources>
    <add key="NuGet official package source" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <trustedSigners>
    <author name="microsoft">
      <certificate fingerprint="3F9001EA83C560D712C24CF213C3D312CB3BFF51EE89435D3430BD06B5D0EECE" hashAlgorithm="SHA256" allowUntrustedRoot="false" />
    </author>
    <repository name="nuget.org" serviceIndex="https://api.nuget.org/v3/index.json">
      <certificate fingerprint="0E5F38F57DC1BCC806D8494F4F90FBCEDD988B46760709CBEEC6F4219AA6157D" hashAlgorithm="SHA256" allowUntrustedRoot="false" />
      <owners>microsoft;aspnet;nuget</owners>
    </repository>
  </trustedSigners>
</configuration>
```

and then our Dockerfile follows the same format as python and node

```Dockerfile
ARG DOTNET_CORE_VERSION=3.1

FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNET_CORE_VERSION}

LABEL dotnet_core_version=${DOTNET_CORE_VERSION}

COPY NuGet.Config /root/.nuget/NuGet/
COPY apt.conf /etc/apt/apt.conf
COPY .curlrc /root/.curlrc

CMD ["bash"]
```

Now you can `dotnet restore` away

### Remarks

If you've made it this far, thanks for reading! Good luck with your hunt for knowledge, the corporate world is a political one! Stay Sane!
