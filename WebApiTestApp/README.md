# Product Catalog Web API

ASP.NET Core 9.0 Web API providing CRUD operations for a simple product catalog with Redis caching.

## Features
- CRUD endpoints for products
- Redis caching for Get all / Get by Id (5 minute expiration)
- Cache invalidation on create/update/delete
- In-memory repository (thread-safe list) as primary data store
- Optional paging (page & pageSize query params)

## Requirements
- .NET 9 SDK
- Running Redis instance (default connection `localhost:6379`)

## Run Redis (local dev work)
### Docker
Install Docker Desktop and run:
```
docker run -d --name redis -p 6379:6379 redis:7-alpine
```

### Windows (WSL recommended)
- Install WSL
```wsl --install
```
- Restart your PC if prompted
- Install a Linux Distribution
```wsl --install -d Ubuntu
```
- Launch Ubuntu from Start Menu
- Set your username and password when prompted
- Update Your Packages in WSL
```sudo apt update && sudo apt upgrade -y```
- Install Redis in WSL
```sudo apt install redis-server -y```
- Configure Redis to allow external connections (optional)
```sudo nano /etc/redis/redis.conf```
- Find `bind 127.0.0.1 ::1`
- Change it to `bind 0.0.0.0`
- Save and exit (Ctrl+X, Y, Enter)
- Start Redis server
```sudo service redis-server start```
- Verify Redis is running
```redis-cli ping```
- You should see `PONG` as a response.

## Configuration
`appsettings.json`:
```json
{
  "Redis": { "Configuration": "localhost:6379" }
}
```

## Run API
```
dotnet restore
dotnet build
dotnet run --project WebApiTestApp
```
Swagger/OpenAPI available at `/openapi/v1.json` (development) and Swagger UI.

## Endpoints
- GET `/api/products` -> list products (supports `?page=1&pageSize=10`)
- GET `/api/products/{id}` -> single product
- POST `/api/products` -> create product (JSON body)
- PUT `/api/products/{id}` -> update product
- DELETE `/api/products/{id}` -> delete product

## Sample Product JSON
```json
{
  "name": "Desk Lamp",
  "description": "LED adjustable lamp",
  "price": 49.99,
  "category": "Home"
}
```

## Notes
- In-memory data resets when app restarts.
