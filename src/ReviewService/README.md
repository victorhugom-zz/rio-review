# Review Service

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