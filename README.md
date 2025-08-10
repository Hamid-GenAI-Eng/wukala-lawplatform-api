# User Authentication API

A clean, modular, and production-ready ASP.NET Core Web API project for user authentication with JWT tokens and Google OAuth 2.0 integration.

## 🚀 Features

- **User Registration**: Email/password signup with strong password validation
- **User Login**: JWT-based authentication for registered users
- **Google OAuth 2.0**: Sign in with Google integration
- **Gmail Validation**: Only valid Gmail addresses (@gmail.com) are accepted
- **JWT Tokens**: Secure token-based authentication
- **PostgreSQL Database**: Entity Framework Core with code-first migrations
- **Clean Architecture**: Repository pattern with dependency injection
- **Global Exception Handling**: Consistent error responses
- **Input Validation**: FluentValidation for request validation
- **Swagger Documentation**: Interactive API documentation
- **CORS Support**: Cross-origin resource sharing configuration

## 🏗️ Architecture

The project follows clean architecture principles:

```
UserAuthAPI/
├── Controllers/          # API Controllers
├── Data/                # Data Access Layer
│   ├── Entities/        # Domain Models
│   ├── Interfaces/      # Repository Interfaces
│   └── Repositories/    # Repository Implementations
├── DTOs/                # Data Transfer Objects
├── Services/            # Business Logic Layer
│   ├── Interfaces/      # Service Interfaces
│   └── Implementations/ # Service Implementations
├── Validators/          # FluentValidation Validators
├── Middleware/          # Custom Middleware
└── Extensions/          # Extension Methods
```

## 🛠️ Technologies Used

- **ASP.NET Core 8.0**
- **Entity Framework Core** with PostgreSQL
- **JWT Authentication**
- **Google APIs** for OAuth 2.0
- **FluentValidation** for input validation
- **Swagger/OpenAPI** for documentation
- **ASP.NET Core Identity** for password hashing

## 📋 Prerequisites

- .NET 8.0 SDK
- PostgreSQL database server
- Google Cloud Console project (for Google OAuth)

## ⚙️ Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd UserAuthAPI
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Configure Database

Update the connection string in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=UserAuthDB;Username=your_username;Password=your_password"
  }
}
```

### 4. Configure JWT Settings

Update JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-jwt-key-that-is-at-least-32-characters-long",
    "Issuer": "UserAuthAPI",
    "Audience": "UserAuthAPI-Client",
    "ExpirationMinutes": 60
  }
}
```

### 5. Configure Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Update `appsettings.json`:

```json
{
  "Google": {
    "ClientId": "your-google-client-id.googleusercontent.com"
  }
}
```

### 6. Run Database Migrations

The application will automatically create the database and apply migrations on startup.

### 7. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001` (in development)

## 📚 API Endpoints

### Authentication Endpoints

#### POST /api/auth/signup
Register a new user with email and password.

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john.doe@gmail.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "success": true,
    "message": "User registered successfully.",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "John Doe",
      "email": "john.doe@gmail.com",
      "provider": "Local",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  }
}
```

#### POST /api/auth/login
Authenticate user with email and password.

**Request Body:**
```json
{
  "email": "john.doe@gmail.com",
  "password": "SecurePassword123!"
}
```

#### POST /api/auth/google-login
Authenticate user with Google ID token.

**Request Body:**
```json
{
  "idToken": "google-id-token-here"
}
```

#### GET /api/auth/validate-gmail
Validate if an email is a valid Gmail address.

**Query Parameters:**
- `email`: Email address to validate

## 🔒 Security Features

### Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character
- No common weak patterns

### Gmail Validation
- Only Gmail addresses (@gmail.com) are accepted
- Email format validation
- Basic Gmail username rules validation

### JWT Token Security
- Secure token generation with configurable expiration
- Token validation middleware
- Claims-based authorization

## 🧪 Testing the API

### Using Swagger UI
1. Navigate to the application root URL
2. Use the interactive Swagger interface
3. Authenticate using the "Authorize" button with Bearer tokens

### Using curl

**Register a new user:**
```bash
curl -X POST "https://localhost:7001/api/auth/signup" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john.doe@gmail.com",
    "password": "SecurePassword123!"
  }'
```

**Login:**
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@gmail.com",
    "password": "SecurePassword123!"
  }'
```

## 🚀 Deployment

### Environment Variables
Set the following environment variables in production:

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Jwt__SecretKey`: JWT secret key (minimum 32 characters)
- `Google__ClientId`: Google OAuth client ID

### Docker Support
The application can be containerized using Docker. Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserAuthAPI.csproj", "."]
RUN dotnet restore "UserAuthAPI.csproj"
COPY . .
RUN dotnet build "UserAuthAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserAuthAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserAuthAPI.dll"]
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Ensure PostgreSQL is running
   - Check connection string format
   - Verify credentials

2. **JWT Token Issues**
   - Ensure secret key is at least 32 characters
   - Check token expiration settings

3. **Google OAuth Issues**
   - Verify Google Client ID is correct
   - Ensure Google+ API is enabled
   - Check OAuth consent screen configuration

### Logs
Check application logs for detailed error information. In development, detailed exception information is included in error responses.

## 📧 Support

For support and questions, please open an issue in the repository or contact the development team.