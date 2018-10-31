---
layout: post
title: "Dockerizing a static web server"
categories: docker
author: "Christopher J. Adkins"
tags: [Docker,Nginx,Static,Web,Page]
usemathjax: false
comments: true
---

These days I've been dockerizing everything, and love the fact that docker allows for a very transparent deployment process with the server management side of things. Currently I'm working for a Canadian telecommunications company, and updating some of their web development processes and pipelines using the power of docker. There is something satisfying about typing `docker-compose up -d` and that's it (well, I guess I've set some of the older guys with a [Portainer](https://www.portainer.io/) GUI, semantics!)

I've created a repo with all the files [here](https://github.com/Softyy/static-web-docker), so if you don't bother with the explanation clone away. Let's start with the static page `index.html`, it's basic html so there's not to much to say

{% highlight html %}
<!DOCTYPE html>
<html>
    <head>
        <meta charset="UTF-8">
    </head>
    <body>
        <h2>Hello World!</h2>
        <h3>Containers for life!</h3>
    </body>
</html>
{% endhighlight %}

To serve this page to someone, we need a web server to handle the request. I usually use Nginx since it does it all (reverse proxy, load balancer, mail proxy, e.t.c) and it's super light weight. So we'll use the convenient docker team supported image `nginx:alpine` which you can learn more about on [nginx's dockerhub page](https://hub.docker.com/_/nginx/). I've also included an example nginx config file `default.conf` to overwrite the default's within the base image.

{% highlight conf %}
server {
    listen      80;
    listen      [::]:80;
    server_name localhost;

    location / {
        root /usr/share/nginx/html;
        index index.html index.htm;
    }
}
{% endhighlight %}

Here we can see that the server configuration is to listen on port 80 with the server name as localhost. If you're deploying this to some server, you'd just had to change to the domain name that points there e.g. example.com. Since the only location is `/`, when you ask for `server_name:port`, it will treat the root folder on server as `/usr/share/nginx/html` and there it'll serve you the the index file named `index.html` or `index.htm`. You can add other locations to point to other files in on your server (in this case it's your container) by just adding new location blocks. Now that's it for the server content and the nginx configuration. All we have to do is put it all together into a `Dockerfile`. So we'll start with the base image `nginx:alpine`, and then copy the html file and config file into the container.

{% highlight dockerfile %}
FROM nginx:alpine

COPY default.conf /etc/nginx/conf.d/default.conf
COPY index.html /usr/share/nginx/html/index.html
{% endhighlight %}

That's it for the container, we could build the image with `docker build -t static-web-page .`, but I usually prefer to use compose so I can simply call `docker-compose up -d` and call it a day. So we'll add `docker-compose.yml`

{% highlight docker %}
version: '2'

services:
  static-web:
    build: .
    ports:
     - "8000:80"
{% endhighlight %}

The basic highligths here are we're using docker-compose version 2. The one service in the compse is called `static-web` and it is built using the previous `Dockerfile` with port 80 inside the container taking requests from 8000 outside. With this compose file, we can easily launch the server with `docker-compose up -d` and visit `localhost:8000` to get the html page we created at the beginning. Now any deploying this web page is one command from any computer with docker and docker-compose on it. Feel free to comment below if you have any questions.
