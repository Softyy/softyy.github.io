---
layout: post
title: "Super charging Django settings with pydantic"
categories: django
author: "Christopher J. Adkins"
tags: [Python,Django,Pydantic,Web,Settings]
usemathjax: false
comments: true
---

Recently I've been tasked with upgrading the infrastructure around django application, and oh boy the settings this application have been in dire need of some TLC. I've had a very positive experience with [pydantic](https://github.com/pydantic/pydantic) and [pydantic settings](https://github.com/pydantic/pydantic-settings) over the last few years and wanted to use that in as our primary settings config. The main benefit being the great typing, ease of settings the configs via *.env* files and enviroment variables for containerized workloads. I was looking around and found acouple projects that have attempted to do something along these lines, namely: [pydjantic](https://github.com/erhosen-libs/pydjantic) and [django-pydantic-settings](https://github.com/joshourisman/django-pydantic-settings) (there's probably more out there as well). I don't think this approach warents another package, but I wanted outline the approach.

In terms of the approach, we're going to use pydantic settings exactly as they recommend. After we're happy with our settings, we're going to write a simple bridge to django settings. You may wonder how django's config system works, well typically you set the env var `DJANGO_SETTINGS_MODULE` to some python module. Then django will treat any variable with all captial letters defined in that module is a setting. e.g.

```python
# app/settings.py 
# DJANGO_SETTINGS_MODULE=app.settings

DEBUG=True

STRIP_CLIENT_ID="WHO_KNOWS"
STRIPE_SECRET_KEY="TOPSECRET"
```

You'll be able to access the settings via

```python
from django.conf import settings

print(settings.DEBUG) # True
print(settings.STRIP_CLIENT_ID) # WHO_KNOWS
print(settings.STRIPE_SECRET_KEY) # TOPSECRET

```

Anyhow, in practice you'll probably want a nicer method to set these values and not hard code them (at least I hope so). So you may end up adding some logic for parsing these via enviroment variables or a file, but it's extra functionality that does not come out of the box. Luckily, many flexible approaches have already been added to pydantic settings, so you could easily write something like so:

```python
from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict

class StripeSettings(BaseSettings):
    client_id: str
    client_secret: str

class AppSettings(BaseSettings):
    model_config = SettingsConfigDict(
         env_file=".env",
         env_file_encoding="utf-8",
         env_nested_delimiter="__",
         extra="ignore",
    )
    debug: bool = True
    stripe: StripeSettings = Field(default_factory=StripeSettings)

settings = AppSettings()
```

You'll notice the `SettingsConfigDict` defines some defaults I like, but this allows us to hide these secrets easily in a local `.env` with the following format:

```bash
# .env
STRIPE__CLIENT_ID=WHO_KNOWS
STRIPE__SECRET_KEY=TOPSECRET
```

Anyhow, this contains the data, but it's not in django's flat format. To flatten it, we can recursively unwrap all the data with something like this.


```python
import logging

from pydantic_settings import BaseSettings

log = logging.getLogger()

def convert_to_django_settings(settings: BaseSettings, prefix: str = ""):
    django_settings = {}

    def set_key(key,value):
        django_key = key.upper()
        if django_key in django_settings:
            log.warning("Duplicate Django setting: %s", django_key)
        django_settings[django_key] = value

    def extract_django_settings(s: BaseSettings, prefix: str = ""):
        for field_name, value in s:
            if isinstance(value, BaseSettings):
                extract_django_settings(value, prefix=f"{prefix}{field_name}_")
            else:
                set_key(f"{prefix}{field_name}",value)

    extract_django_settings(settings, prefix)
    return django_settings
```

With this function in hand, we can now structure our settings module in the following way

```
app
- settings
  - __init__.py
  - main.py
  - utils.py
```

and add this to `__init__.py` and we've reached parity with the previous django settings file.

```python
from app.settings.main import settings
from app.settings.utils import convert_to_django_settings

django_settings = convert_to_django_settings(settings)
locals().update(django_settings)
```

That's the basics to the approach, now let's take this to the next level with `Annotated`. Let's say you need a specific format of data, maybe the key needs to be top level for example. Let's say I wanted this format on the pydantic side instead:

```python
class DjangoSettings(BaseSettings):
    debug: bool = True

class AppSettings(BaseSettings):
    ...
    django: DjangoSettings = Field(default_factory=DjangoSettings)
    ...
```

With our previous recurisve approach, this would end up converting to `DJANGO_DEBUG` instead of `DEBUG`. But we can easily add an override here. Let's say I wanted all the fields on django settings to be *top level*. We can add some metadata to the field to denote that. Something like this

```python
from typing import Annotated
# from typing_extensions if you're on an older python version

class DjangoSettings(BaseSettings):
    debug: bool = True

class AppSettings(BaseSettings):
    ...
    django: Annotated[DjangoSettings, {"top_level"}] = Field(default_factory=DjangoSettings)
    ...
```

Functionally this is identical to the previous implementation, but now we've got some metadata associated with the field. I'll cut right to the chase and show you how to access this data with pydantic. (Warning: `metadata` can contain whatever types you pass into it via `Annotated`)

```python
def get_metadata(field_name, s: BaseSettings, key: str):
    field_info = s.model_fields[field_name]
    return next((True for _ in field_info.metadata if key in _), False)
```

So now that conversion function can be customized to whatever you want to happen with whatever metadata you decide to include. e.g.

```python
def convert_to_django_settings(_settings: BaseSettings, prefix: str = ""):
    django_settings = {}

    def set_key(key,value):
        django_key = key.upper()
        if django_key in django_settings:
            log.warning("Duplicate Django setting: %s", django_key)
        django_settings[django_key] = value

    def extract_django_settings(s: BaseSettings, prefix: str = ""):
        for field_name, value in s:
            if isinstance(value, BaseSettings):
                top_level = get_metadata(field_name, s, key="top_level")
                _prefix = "" if top_level else f"{prefix}{field_name}_"
                extract_django_settings(value, prefix=_prefix)
            else:
                set_key(f"{prefix}{field_name}",value)

    extract_django_settings(_settings, prefix)
    return django_settings
```

Now `DJANGO_DEBUG` is transformed to `DEBUG` as we wanted via the flagged prefix logic. Probably a more important example would be settings dictionaries on the django side. Probably the most important dictionary settings is your database connection. Recall it looks something like:

```python
DATABASES = {
    "default": {
        "ENGINE": "django.db.backends.postgresql",
        "NAME": "mydatabase",
        "USER": "mydatabaseuser",
        "PASSWORD": "mypassword",
        "HOST": "127.0.0.1",
        "PORT": "5432",
    } 
}
```

We can achieve something in pydantic settings with something like this:

```python
class DatabaseConnection(BaseSettings):
    engine: str
    name: str
    user: Optional[str] = None
    password: Optional[str] = None
    host: Optional[str] = None
    port: Optional[str] = None

class AppDatabaseSettings(BaseSettings):
    default: DatabaseConnection = Field(default_factory=DatabaseConnection)

class AppSettings(BaseSettings):
    ...
    databases: Annotated[AppDatabaseSettings, {"as_dict"}] = Field(default_factory=AppDatabaseSettings)
    ...
```

and we'll need to add a bit of logic to our translation func to handle `as_dict`. Something like this suffices:

```python
def convert_to_django_settings(_settings: BaseSettings, prefix: str = ""):
    ...

    def extract_django_settings(s: BaseSettings, prefix: str = ""):
        for field_name, value in s:
            if isinstance(value, BaseSettings):
                as_dict = get_metadata(field_name, s, key="as_dict")
                if as_dict:
                    set_key(f"{prefix}{field_name}",value.model_dump(exclude_none=True))
                    continue
                top_level = get_metadata(field_name, s, key="top_level")
                _prefix = "" if top_level else f"{prefix}{field_name}_"
                extract_django_settings(value, prefix=_prefix)
            else:
                set_key(f"{prefix}{field_name}",value)

    ...
```

With the new functionality at hand, we can set these values with environment variables like this:

```bash
# .env
DATABASES__DEFAULT__ENGINE=django.db.backends.postgresql
DATABASES__DEFAULT__NAME=mydatabase
DATABASES__DEFAULT__USER=mydatabaseuser
DATABASES__DEFAULT__PASSWORD=mypassword
DATABASES__DEFAULT__HOST=127.0.0.1
DATABASES__DEFAULT__PORT=5432
```

You can even set calculated properties this way!

Anyhow, just wanted to share this approach in case others are looking to use pydantic settings with django as well. Feel free to leave a comment if you have a question about a specific situtation. 