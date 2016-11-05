# Review Service

# Building images

docker build -t metheora/review-service .

docker tag metheora/review-service metheora/review-service:{vesion}
docker tag metheora/review-service metheora/review-service:latest

docker push metheora/review-service:{vesion}
docker push metheora/review-service:latest

## Usage

`mkdir review`
`cd review`
`touch docker-compose.yml`

### docker-compose.yml

```
reviewservice:
  image: metheora/review-service
  ports:
    - "5060:5060"
  links:
    - mongo
mongo:
  image: mongo
  ports:
    - "27017:27017"
  volumes:
    - ./db:/data/db
```

### Run

`docker-compose up -d`