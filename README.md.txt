# IllustratorsApp

A microservices-based portfolio platform for illustrators built with ASP.NET Core 8, PostgreSQL, Docker, and AWS.

## Architecture

- **Auth Service**: JWT authentication and authorization
- **Illustrator Service**: Manage illustrator profiles and portfolios
- **Image Service**: Handle image uploads to S3 and metadata
- **Search Service**: Search functionality across illustrators and images
- **Contact Service**: Handle contact forms and messages

## Tech Stack

- **Backend**: ASP.NET Core 8 (C#)
- **Database**: PostgreSQL (AWS RDS)
- **Storage**: AWS S3
- **Processing**: AWS Lambda
- **Container**: Docker
- **API Gateway**: NGINX
- **Deployment**: AWS EC2 (Free Tier)

## Prerequisites

- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL
- AWS Account

## Getting Started

### Local Development

1. Clone the repository:
```bash
git clone https://github.com/yourusername/IllustratorsApp.git
cd IllustratorsApp
```

2. Set up environment variables (create `.env` file in root):
```bash
cp .env.example .env
# Edit .env with your configuration
```

3. Run with Docker Compose:
```bash
docker-compose up -d
```

4. Access services:
- Auth Service: http://localhost:5001
- Illustrator Service: http://localhost:5002
- Image Service: http://localhost:5003
- Search Service: http://localhost:5004
- Contact Service: http://localhost:5005
- NGINX Gateway: http://localhost:8080

## Project Structure
```
IllustratorsApp/
├── src/
│   ├── AuthService/
│   ├── IllustratorService/
│   ├── ImageService/
│   ├── SearchService/
│   └── ContactService/
├── infrastructure/
│   ├── docker-compose.yml
│   └── nginx/
├── scripts/
└── docs/
```

## API Documentation

API documentation is available at `/swagger` endpoint for each service.

## Deployment

See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for AWS deployment instructions.

## License

MIT