---
layout: post
title: "Open Source Analytics In A Beautiful Dashboard? - Part 3"
categories: dash
author: "Christopher J. Adkins"
tags: [Dash,Analytics,Tools,Plotly,React,Docker,Postgres,Redis]
usemathjax: false
comments: true
---

Picking up where we left off in the [last post](/dash/2018/11/16/open-source-analytics-dashboards-part-two.html), we're going to start with 2 things:

- Setting up our database/model with an ORM (Object-Relational Mapper, we'll use SQLAlchemy).
- Setting up our caching server (Redis) with dash.

The caching server is only needed in situations where you're pulling data that multiple uses might need. If you're using this for an example of analytics dashboard for multiple users, the caching serer may suit your needs.

Let's begin by installing flask_caching, redis, and flask_sqlalchemy. With those installed, let's create our database connection. Add the following to our `__init__.py`

{% highlight python %}
...
from flask_sqlalchemy import SQLAlchemy
...
app.server.config['SQLALCHEMY_DATABASE_URI'] = os.getenv('CONNECTION_STRING')
app.server.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False # silence the deprecation warning
db = SQLAlchemy(app.server)
...
{% endhighlight %}

We'll set the connection string in the environment variables. Now let's define our data model. Since the data we're working with a credit card transaction, I've called the fill with the model `transaction.py` and I've defined the model according to the data we already have.

{% highlight python %}
from webapp import db

from ..consts import TRANSACTION_TABLE_NAME

class Transaction(db.Model):
    __tablename__ = TRANSACTION_TABLE_NAME

    id = db.Column(db.Integer,unique=True, nullable=False, primary_key=True)
    date = db.Column(db.Date,nullable=False)
    vendor = db.Column(db.String,nullable=False)
    charge = db.Column(db.Float)
    credit = db.Column(db.Float)
    total_balance = db.Column(db.Float)
{% endhighlight %}

Now if we call `db.create_all()` this will create our table with a Schema that fits our Transaction class. Currently my data is scattered into csv's, so using the method `load_data()` from our data manager, I'll use pandas' built in `to_sql()` to toss that data into the db if it hasn't already done so. I've added a file called `startup.py` to run after initialization (by importing it in `__init__.py`).

{% highlight python %}
from webapp import db,dm
from .models.transaction import Transaction

# creates the table schema's if they do not already exist.
db.create_all()

#populate the database if the data isn't there.
if not Transaction.query.first():
    dm.load_data()
    dm.data.to_sql( name=Transaction.__tablename__, 
                    con=db.engine, 
                    if_exists = 'append', 
                    index=False
                    )

db.session.commit()
{% endhighlight %}

Now let's add a method to extract the data from the database in our data manager. 

{% highlight python %}
def load_from_db(self,db):
    with db.engine.connect() as conn, conn.begin():
        return pd.read_sql(TRANSACTION_TABLE_NAME,conn)
{% endhighlight %}

Now we can replace all references to self.data with this method. The finishing touch is in the docker files, in our local override I've added the connection string and the database setup.

```
...
    environment:
        - FLASK_ENV=development # this is a env var we've added to the container.
        - CONNECTION_STRING=postgresql://postgres:password@db:5432/postgres #postgresql:///<username>:<password>@<host>:<port>/<database>
        - REDIS_URL=redis://cache:6379
    command: ["python", "run.py"] # this is an override of the CMD in the Dockerfile

db:
    restart: always
    environment:
        - POSTGRES_PASSWORD=password
```

We'll also need to include the drivers to establish the postgres connection in our Dockerfile, so update the C binaries line to include those.

```
# adding the C binaries for pandas and postgres drivers
RUN apk --update add --no-cache g++ libpq postgresql-dev
```

To setup the caching server is just as simple. We initialize the cache reference in `__init__.py`

{% highlight python %}
...
db = SQLAlchemy(app.server)

CACHE_CONFIG = {
    'CACHE_TYPE': 'redis',
    'CACHE_REDIS_URL': os.getenv('REDIS_URL'),
    # should be equal to maximum number of users on the app at a single time
    # higher numbers will store more data in the redis cache
    'CACHE_THRESHOLD': 200 
}

cache = Cache()
cache.init_app(app.server, config=CACHE_CONFIG)
...
{% endhighlight %}

with reference to the env var REDIS_URL (just as we did for the db). Now around any function we'd like to cache. Simply add the cached / memorize decorators. For example I've cached the `load_from_db()` method in data manager.

{% highlight python %}
...
from webapp import cache,db
...
@cache.memoize()
def load_from_db(self):
    with db.engine.connect() as conn, conn.begin():
        return pd.read_sql(TRANSACTION_TABLE_NAME,conn)
...
{% endhighlight %}

At this point we've basically set everything up. We've got our data, we've got some simple visualizations, and we've got our cache.
