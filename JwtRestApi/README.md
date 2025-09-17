# MultilingualCRUD_Api

A modern full-stack application with ASP.NET Core 9 Web API backend and HTML/JavaScript frontend. Features comprehensive CRUD operations, JWT authentication, multilingual support, and a beautiful responsive UI.

## Features

### Backend (ASP.NET Core 9)
- **User Management**: Complete CRUD operations for users
- **Role Management**: Role-based access control with Admin privileges
- **JWT Authentication**: Secure token-based authentication with refresh token support
- **Multilingual Support**: Localized responses in English, Hindi, and Bengali
- **SQLite Database**: Lightweight, serverless database for easy deployment
- **Entity Framework Core**: Modern ORM with migrations support
- **ADO.NET Integration**: Dapper for complex queries and performance optimization
- **Swagger Integration**: Interactive API documentation with JWT authentication

### Frontend (HTML/JavaScript)
- **Modern UI**: Beautiful, responsive interface built with HTML, CSS, and JavaScript
- **Authentication**: Secure login with JWT token management
- **Dashboard**: Comprehensive dashboard for managing users and roles
- **AJAX Integration**: Seamless API communication with automatic token refresh
- **No Build Process**: Simple HTML/JS files that work directly in the browser
- **Responsive Design**: Works perfectly on desktop, tablet, and mobile devices

## Technology Stack

### Backend
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0** - Object-relational mapping
- **SQLite** - Embedded database
- **JWT Bearer Authentication** - Secure token-based auth
- **Dapper** - Micro ORM for data access
- **BCrypt.Net** - Password hashing
- **Localization** - Multi-language support

### Frontend
- **HTML5** - Modern semantic markup
- **CSS3** - Responsive styling with Flexbox and Grid
- **Vanilla JavaScript** - No frameworks, pure JavaScript
- **Fetch API** - Modern HTTP client for API calls
- **Local Storage** - Client-side token management

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Entity Framework Core Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)
- Any modern web browser (Chrome, Firefox, Safari, Edge)
- Any code editor (Visual Studio, VS Code, Rider, etc.)

## Quick Start

### 1. Clone and Setup
```bash
git clone <repository-url>
cd MultilingualCRUD_Api
```

### 2. Install Dependencies
```bash
# Install backend dependencies
dotnet restore
```

### 3. Database Setup
The application uses SQLite, so no external database server is required. However, you need to create the database tables before running the application.

   ```bash
# Install EF Core tools (if not already installed)
   dotnet tool install --global dotnet-ef

# Create and apply migrations
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

**Important**: If you encounter "no such table: Users" errors when testing the API, it means the database tables haven't been created yet. Make sure to run the migration commands above before testing the API endpoints.

### 4. Run the Application
```bash
dotnet run --urls "http://localhost:5200"
```

The application will be available at: `http://localhost:5200`

### 5. Access the Application

#### Web Application
- **Main Application**: `http://localhost:5200` (HTML/JavaScript frontend)
- **Login Page**: `http://localhost:5200` (redirects to login if not authenticated)
- **Dashboard**: `http://localhost:5200` (requires authentication)

#### API Documentation (Swagger)
- **Swagger UI**: `http://localhost:5200/swagger` (API documentation)
- **API Documentation**: `http://localhost:5200/swagger/v1/swagger.json`

The Swagger UI provides:
- Interactive API testing interface
- JWT authentication support
- Request/response examples
- Complete API documentation

## API Endpoints

### Authentication
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | User login | No |
| POST | `/api/auth/refresh` | Refresh access token | No |

### Users
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/users` | Get all users | Yes |
| POST | `/api/users` | Create new user | Yes |
| GET | `/api/users/{id}` | Get user by ID | Yes |
| PUT | `/api/users/{id}` | Update user | Yes |
| DELETE | `/api/users/{id}` | Delete user | Yes |

### Roles
| Method | Endpoint | Description | Auth Required | Role Required |
|--------|----------|-------------|---------------|---------------|
| GET | `/api/roles` | Get all roles | Yes | Admin |
| POST | `/api/roles` | Create new role | Yes | Admin |
| GET | `/api/roles/{id}` | Get role by ID | Yes | Admin |
| PUT | `/api/roles/{id}` | Update role | Yes | Admin |
| DELETE | `/api/roles/{id}` | Delete role | Yes | Admin |

## Using the Application

### Web Application
The HTML/JavaScript frontend provides a modern, user-friendly interface for interacting with the API:

1. **Login**: Navigate to `http://localhost:5200`
   - Enter your credentials (username/email and password)
   - The app will automatically handle JWT token management

2. **Dashboard**: After login, you'll see the dashboard
   - View all users and their roles
   - Manage user accounts with add/edit/delete functionality
   - View and manage roles with full CRUD operations
   - Responsive design works on all devices

3. **Authentication**: 
   - Automatic token refresh
   - Secure logout functionality
   - Protected routes and API calls

### API Testing

#### Option 1: Using Swagger UI (Recommended)

1. **Open Swagger UI**: Navigate to `http://localhost:5200` in your browser
2. **Test Authentication**:
   - Click on the `POST /api/auth/login` endpoint
   - Click "Try it out"
   - Enter test credentials (note: you'll need to create a user first)
   - Click "Execute"
3. **Authenticate in Swagger**:
   - Copy the `accessToken` from the login response
   - Click the "Authorize" button (🔒) at the top of the page
   - Enter: `Bearer YOUR_ACCESS_TOKEN`
   - Click "Authorize"
4. **Test Protected Endpoints**:
   - Now you can test all protected endpoints directly in Swagger
   - The JWT token will be automatically included in requests

### Option 2: Using cURL Commands

#### 1. Test Authentication (Login)
```bash
curl -X POST http://localhost:5200/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "testuser",
    "password": "testpassword"
  }'
```

**Expected Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here"
}
```

#### 2. Test Protected Endpoints
```bash
# Get all users (requires authentication)
curl -X GET http://localhost:5200/api/users \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### 3. Test Multilingual Support
```bash
# English (default)
curl -X POST http://localhost:5200/api/auth/login \
  -H "Content-Type: application/json" \
  -H "Accept-Language: en" \
  -d '{"usernameOrEmail": "invalid", "password": "invalid"}'

# Hindi
curl -X POST http://localhost:5200/api/auth/login \
  -H "Content-Type: application/json" \
  -H "Accept-Language: hi" \
  -d '{"usernameOrEmail": "invalid", "password": "invalid"}'

# Bengali
curl -X POST http://localhost:5200/api/auth/login \
  -H "Content-Type: application/json" \
  -H "Accept-Language: bn" \
  -d '{"usernameOrEmail": "invalid", "password": "invalid"}'
```

#### 4. Test Token Refresh
```bash
curl -X POST http://localhost:5200/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

### Creating Test Data

Since the database starts empty, you'll need to create users and roles before testing. You can do this through:

1. **Swagger UI**: Use the POST endpoints to create users and roles
2. **Database directly**: Insert data into the SQLite database
3. **API calls**: Use cURL or Postman to create initial data

**Note**: Make sure to hash passwords with BCrypt before inserting users directly into the database.

## Configuration

### JWT Settings
Update `appsettings.json` to configure JWT settings:

```json
{
  "Jwt": {
    "Key": "YOUR_STRONG_SECRET_KEY_HERE",
    "Issuer": "MultilingualJwtApi",
    "Audience": "MultilingualJwtApiUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Database Connection
The SQLite database file (`multilingual.db`) will be created in the project root directory.

## Project Structure

```
MultilingualCRUD_Api/
├── Controllers/           # API Controllers
│   ├── AuthController.cs  # Authentication endpoints
│   ├── UsersController.cs # User management
│   └── RolesController.cs # Role management
├── Data/                 # Data access layer
│   ├── ApplicationDbContext.cs # EF Core context
│   └── AdoNetHelper.cs   # Dapper helper
├── Models/               # Entity models
│   ├── User.cs
│   ├── Role.cs
│   ├── UserRole.cs
│   └── RefreshToken.cs
├── Services/             # Business logic
│   ├── IJwtService.cs
│   └── JwtService.cs
├── Resources/            # Localization resources
│   ├── SharedResources.cs
│   └── SharedResources.resx
├── Program.cs            # Application entry point
├── appsettings.json      # Configuration
└── multilingual.db       # SQLite database (auto-created)
```

## Development

### Adding New Migrations
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Building for Production
   ```bash
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish
```

## Security Features

- **Password Hashing**: BCrypt for secure password storage
- **JWT Tokens**: Stateless authentication with configurable expiration
- **Refresh Tokens**: Secure token renewal mechanism
- **Role-Based Authorization**: Granular access control
- **SQL Injection Protection**: Parameterized queries with Dapper

## Troubleshooting

### Common Issues

1. **"no such table: Users" error**: Run `dotnet ef database update` to create database tables
2. **Database not found**: Run `dotnet ef database update`
3. **JWT validation errors**: Check JWT configuration in `appsettings.json`
4. **Localization not working**: Ensure `Accept-Language` header is set correctly
5. **Port conflicts**: Change port in `Program.cs` or use `--urls` parameter
6. **Swagger UI not loading**: Ensure you're running in Development mode or check the environment configuration

### Logs
The application provides detailed logging. Check console output for error messages and debugging information.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
