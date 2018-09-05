FROM jekyll/jekyll:pages

RUN gem install github-pages

COPY . .

EXPOSE 4000

CMD ["bundle","exec","jekyll","serve"] 