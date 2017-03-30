# Review Service

# Building images

`docker build -t metheora/review-service .`

`docker tag metheora/review-service metheora/review-service:{vesion}`

`docker tag metheora/review-service metheora/review-service:latest`

`docker push metheora/review-service:{vesion}`

`docker push metheora/review-service:latest`

## Usage Docker

`mkdir review`

`cd review`

`touch docker-compose.yml`

### docker-compose.yml

```
version: '2'

services:
  reviewservice:
    image: metheora/review-service
    restart: always
    ports:
      - "5060:5060"
    links:
      - mongo
    command: [--MONGO_URI, 'mongodb://mongo:27017']
  mongo:
    image: mongo
    restart: always
    ports:
      - "27017:27017"
    volumes:
      - ./db:/data/db
```

### Run

`docker-compose up -d`


## Usage IIS

* Run a mongo instance
* Set an appsettings.{env}.json

Ex: ```
{
  "DbPort": 27017,
  "DbHost": "localhost"
}
```
* Run the project