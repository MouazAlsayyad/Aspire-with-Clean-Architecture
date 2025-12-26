# AspireApp - .NET Aspire Application with Clean Architecture

A modern .NET Aspire application built using Clean Architecture principles, featuring authentication, authorization (RBAC), and comprehensive user, role, and permission management.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Development Guide](#development-guide)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)

## 🎯 Overview

AspireApp is a cloud-native application built with .NET Aspire that demonstrates best practices in:
- **Clean Architecture** - Separation of concerns across multiple layers
- **Authentication & Authorization** - JWT-based authentication with refresh tokens and Role-Based Access Control (RBAC)
- **Microservices-Ready** - Built with .NET Aspire for distributed application development
- **Modern API Design** - Minimal APIs with endpoint-based routing

The application provides a complete user management system with roles and permissions, allowing fine-grained access control to resources. It supports both role-based permissions and direct user permission assignment, providing maximum flexibility for access control. It includes a secure refresh token mechanism for seamless token renewal without requiring users to re-authenticate.

## 🏗️ Architecture

This project follows **Clean Architecture** principles, organizing code into distinct layers with clear dependencies:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│              (Endpoints, Attributes)                     │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│                  Application Layer                       │
│         (Use Cases, DTOs, Mappings, Validators)         │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│                    Domain Layer                          │
│      (Entities, Interfaces, Value Objects, Services)    │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│                Infrastructure Layer                      │
│    (Repositories, Data Access, External Services)      │
└─────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

#### 1. **Domain Layer** (`AspireApp.ApiService.Domain`)
- **Purpose**: Core business logic and entities
- **Contains**:
  - Domain entities (User, Role, Permission, UserRole, UserPermission, RolePermission, RefreshToken, etc.)
  - Domain interfaces (repositories, services)
  - Value objects (PasswordHash, etc.)
  - Domain services
  - Enums and constants
- **Dependencies**: None (pure domain logic)
- **Key Files**:
  - `Entities/` - Core domain models
  - `Interfaces/` - Contracts for repositories and services
  - `Services/` - Domain service interfaces

#### 2. **Application Layer** (`AspireApp.ApiService.Application`)
- **Purpose**: Application use cases and business workflows
- **Contains**:
  - Use cases (LoginUserUseCase, RefreshTokenUseCase, CreateRoleUseCase, etc.)
  - DTOs (Data Transfer Objects)
  - AutoMapper profiles
  - FluentValidation validators
- **Dependencies**: Domain layer only
- **Key Files**:
  - `UseCases/` - Business logic implementations
  - `DTOs/` - Data transfer objects
  - `Mappings/` - AutoMapper configurations
  - `Validators/` - Input validation rules

#### 3. **Infrastructure Layer** (`AspireApp.ApiService.Infrastructure`)
- **Purpose**: External concerns and data access
- **Contains**:
  - Entity Framework Core DbContext
  - Repository implementations
  - JWT token service
  - Password hashing service
  - Authorization handlers
  - Database migrations
- **Dependencies**: Domain layer only
- **Key Files**:
  - `Data/ApplicationDbContext.cs` - EF Core context
  - `Repositories/` - Repository implementations
  - `Identity/TokenService.cs` - JWT token generation
  - `Authorization/` - Permission-based authorization

#### 4. **Presentation Layer** (`AspireApp.ApiService.Presentation`)
- **Purpose**: API endpoints and HTTP concerns
- **Contains**:
  - Minimal API endpoints
  - Authorization attributes
  - Endpoint extensions
- **Dependencies**: Application layer
- **Key Files**:
  - `Endpoints/` - API endpoint definitions
  - `Attributes/` - Custom authorization attributes

#### 5. **Main API Project** (`AspireApp.ApiService`)
- **Purpose**: Application entry point and composition root
- **Contains**:
  - `Program.cs` - Service configuration and middleware setup
  - `appsettings.json` - Configuration files
- **Dependencies**: All layers
- **Responsibilities**:
  - Dependency injection configuration
  - Middleware pipeline setup
  - Database seeding
  - Authentication/Authorization configuration

#### 6. **AppHost** (`AspireApp.AppHost`)
- **Purpose**: .NET Aspire orchestration
- **Contains**:
  - `AppHost.cs` - Service definitions and orchestration
- **Responsibilities**:
  - Defines distributed application structure
  - Configures service discovery
  - Sets up health checks
  - Manages service dependencies

## 📁 Project Structure

```
AspireApp1/
├── AspireApp.ApiService/              # Main API project (entry point)
│   ├── Program.cs                      # Application startup
│   ├── appsettings.json               # Configuration
│   └── AspireApp.ApiService.csproj
│
├── AspireApp.ApiService.Domain/        # Domain Layer
│   ├── Common/                         # Domain utilities
│   ├── Entities/                       # Domain entities
│   ├── Enums/                          # Domain enums
│   ├── Interfaces/                     # Domain contracts
│   ├── Permissions/                    # Permission definitions
│   ├── Services/                       # Domain service interfaces
│   └── ValueObjects/                   # Value objects
│
├── AspireApp.ApiService.Application/   # Application Layer
│   ├── Common/                         # Base classes
│   ├── DTOs/                           # Data Transfer Objects
│   ├── Mappings/                       # AutoMapper profiles
│   ├── UseCases/                       # Business logic
│   └── Validators/                     # FluentValidation validators
│
├── AspireApp.ApiService.Infrastructure/# Infrastructure Layer
│   ├── Authorization/                  # Authorization handlers
│   ├── Data/                           # EF Core DbContext
│   ├── Extensions/                     # Extension methods
│   ├── Identity/                       # Identity services
│   ├── Migrations/                     # Database migrations
│   ├── Repositories/                   # Repository implementations
│   └── Services/                       # Infrastructure services
│
├── AspireApp.ApiService.Presentation/  # Presentation Layer
│   ├── Attributes/                     # Custom attributes
│   ├── Endpoints/                      # API endpoints
│   └── Extensions/                     # Extension methods
│
├── AspireApp.AppHost/                  # Aspire AppHost
│   ├── AppHost.cs                      # Service orchestration
│   └── AspireApp.AppHost.csproj
│
└── AspireApp.ServiceDefaults/          # Shared Aspire defaults
    └── Extensions.cs                   # Service defaults extension
```

## ⚙️ How It Works

### Request Flow

1. **HTTP Request** → Received by Presentation Layer endpoint
2. **Authorization** → Permission-based authorization check
3. **Validation** → FluentValidation validates input DTOs
4. **Use Case Execution** → Application layer use case processes request
5. **Domain Logic** → Domain services enforce business rules
6. **Data Access** → Infrastructure repositories access database
7. **Response** → DTO mapped and returned to client

### Authentication Flow

#### Login Flow
1. User submits credentials via `/api/auth/login` endpoint
2. `LoginUserUseCase` validates credentials
3. `PasswordHasher` verifies password hash
4. `TokenService` generates access token (1 hour expiration) and refresh token (7 days expiration)
5. Refresh token is stored in the database
6. Both tokens returned to client in `AuthResponse`
7. Client includes access token in `Authorization: Bearer <token>` header for authenticated requests

#### Refresh Token Flow
1. When access token expires, client sends refresh token to `/api/auth/refresh` endpoint
2. `RefreshTokenUseCase` validates the refresh token (checks if it exists, is not revoked, and not expired)
3. Old refresh token is revoked
4. New access token and refresh token are generated
5. New refresh token is stored in the database
6. New tokens returned to client
7. Client updates stored tokens and continues using the application without re-authentication

### Authorization Flow

1. Request arrives with JWT token
2. JWT middleware validates token
3. `PermissionAuthorizationHandler` checks user permissions
4. Permission resolution follows this priority:
   - **Direct user permissions** are checked first (permissions directly assigned to the user)
   - **Role-based permissions** are checked if no direct permission found
   - User has permission if either direct or role-based permission exists
5. Request allowed or denied based on policy evaluation

### Database Seeding

On application startup, the database is automatically seeded with:
- Default roles (Admin, Manager, User)
- Default permissions
- Default admin user (credentials in `DatabaseSeeder`)

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd AspireApp1
   ```

2. **Install .NET Aspire workload** (if not already installed)
   ```bash
   dotnet workload install aspire
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Update connection string** (if needed)
   Edit `AspireApp.ApiService/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=(localdb)\\MSSQLLocalDB;Database=Aspire;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

5. **Update JWT secret key** (important for production)
   Edit `AspireApp.ApiService/appsettings.json`:
   ```json
   {
     "Jwt": {
       "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
     }
   }
   ```

6. **Run database migrations**
   ```bash
   cd AspireApp.ApiService
   dotnet ef database update --project ../AspireApp.ApiService.Infrastructure
   ```

7. **Run the application**
   ```bash
   # Run the AppHost (orchestrates all services)
   dotnet run --project AspireApp.AppHost
   ```
   
   This will:
   - Start the API service
   - Open the Aspire dashboard in your browser
   - Display service endpoints and health status

### Accessing the API

- **API Base URL**: `https://localhost:7XXX` (port shown in Aspire dashboard)
- **OpenAPI/Swagger**: `https://localhost:7XXX/openapi/v1.json`
- **Scalar UI**: `https://localhost:7XXX/scalar/v1` (development only)
- **Health Check**: `https://localhost:7XXX/health`

## 💻 Development Guide

### Adding a New Feature

#### 1. **Define Domain Entity** (Domain Layer)

Create entity in `AspireApp.ApiService.Domain/Entities/`:
```csharp
public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    // ... other properties
}
```

#### 2. **Define Repository Interface** (Domain Layer)

Create interface in `AspireApp.ApiService.Domain/Interfaces/`:
```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByNameAsync(string name);
}
```

#### 3. **Implement Repository** (Infrastructure Layer)

Create implementation in `AspireApp.ApiService.Infrastructure/Repositories/`:
```csharp
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<Product?> GetByNameAsync(string name)
    {
        return await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Name == name);
    }
}
```

Register in `Infrastructure/Extensions/ServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<IProductRepository, ProductRepository>();
```

#### 4. **Create DTOs** (Application Layer)

Create DTOs in `AspireApp.ApiService.Application/DTOs/Product/`:
```csharp
public record CreateProductDto(string Name, decimal Price);
public record ProductDto(int Id, string Name, decimal Price);
```

#### 5. **Create Use Case** (Application Layer)

Create use case in `AspireApp.ApiService.Application/UseCases/Products/`:
```csharp
public class CreateProductUseCase : BaseUseCase<CreateProductDto, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    
    public CreateProductUseCase(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    public override async Task<Result<ProductDto>> ExecuteAsync(CreateProductDto request)
    {
        var product = new Product { Name = request.Name, Price = request.Price };
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
        
        return Result.Success(_mapper.Map<ProductDto>(product));
    }
}
```

#### 6. **Create Validator** (Application Layer)

Create validator in `AspireApp.ApiService.Application/Validators/Product/`:
```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

#### 7. **Create Mapping Profile** (Application Layer)

Create profile in `AspireApp.ApiService.Application/Mappings/`:
```csharp
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
```

#### 8. **Create Endpoint** (Presentation Layer)

Create endpoint in `AspireApp.ApiService.Presentation/Endpoints/`:
```csharp
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");
        
        group.MapPost("/", async (
            CreateProductDto dto,
            CreateProductUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(dto);
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateProduct")
        .RequirePermission("Products.Create")
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
```

#### 9. **Register Endpoint** (Main API Project)

In `AspireApp.ApiService/Program.cs`:
```csharp
app.MapProductEndpoints();
```

### Database Migrations

**Create a migration:**
```bash
cd AspireApp.ApiService
dotnet ef migrations add MigrationName --project ../AspireApp.ApiService.Infrastructure
```

**Apply migrations:**
```bash
dotnet ef database update --project ../AspireApp.ApiService.Infrastructure
```

**Using Package Manager Console (Visual Studio):**
```powershell
// Add-Migration Init -StartupProject AspireApp.ApiService
// Update-Database -StartupProject AspireApp.ApiService
```

### Testing Endpoints

Use the Scalar UI (available in development) or tools like:
- **Postman**
- **curl**
- **HTTP files** (`.http` files in the project)

Example HTTP requests:

**Login:**
```http
POST https://localhost:7XXX/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "YourPassword"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedrefreshtoken...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": { ... }
}
```

**Refresh Token:**
```http
POST https://localhost:7XXX/api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "base64encodedrefreshtoken..."
}
```

**Authenticated Request:**
```http
POST https://localhost:7XXX/api/products
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "name": "Sample Product",
  "price": 99.99
}
```

### Working with Permissions

The application supports **dual permission assignment**:
- **Role-based permissions**: Permissions assigned to roles, which are then inherited by users with those roles
- **Direct user permissions**: Permissions directly assigned to individual users (takes precedence over role-based permissions)

#### Permission Resolution Priority

When checking if a user has a permission:
1. **Direct user permissions** are checked first
2. If not found, **role-based permissions** are checked
3. User has permission if found in either source

#### Defining Permissions

1. **Define permission name** in `Domain/Permissions/PermissionNames.cs`:
   ```csharp
   public static class PermissionNames
   {
       public const string ProductsCreate = "Products.Create";
       public const string ProductsRead = "Products.Read";
       // ...
   }
   ```

2. **Use in endpoint**:
   ```csharp
   .RequirePermission(PermissionNames.ProductsCreate)
   ```

#### Assigning Permissions

**Assign permissions to a role** (role-based):
```http
POST /api/roles/{roleId}/permissions
{
  "permissionIds": [1, 2, 3]
}
```

**Assign permissions directly to a user** (direct assignment):
```http
POST /api/users/{userId}/permissions
{
  "permissionIds": [1, 2, 3]
}
```

**Note**: Direct user permissions take precedence over role-based permissions. This allows for fine-grained access control where specific users can have additional permissions beyond their roles, or exceptions where a user needs a permission without having the full role.

## ✨ Key Features

- ✅ **Clean Architecture** - Clear separation of concerns
- ✅ **JWT Authentication** - Secure token-based authentication with refresh tokens
- ✅ **Refresh Token Mechanism** - Seamless token renewal without re-authentication with token rotation and reuse detection
- ✅ **RBAC Authorization** - Role and permission-based access control
- ✅ **Dual Permission System** - Both role-based and direct user permission assignment
- ✅ **Minimal APIs** - Modern endpoint-based API design
- ✅ **Entity Framework Core** - Code-first database approach
- ✅ **AutoMapper** - Object-to-object mapping
- ✅ **FluentValidation** - Input validation
- ✅ **OpenAPI/Scalar** - Interactive API documentation
- ✅ **Health Checks** - Application health monitoring
- ✅ **.NET Aspire** - Cloud-native orchestration
- ✅ **OpenTelemetry** - Observability and tracing

## 🛠️ Technology Stack

- **.NET 10.0** - Latest .NET framework
- **.NET Aspire 9.5.0** - Cloud-native application framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core 10.0** - ORM
- **SQL Server** - Database
- **JWT Bearer Authentication** - Authentication
- **AutoMapper** - Object mapping
- **FluentValidation** - Validation
- **Scalar** - API documentation UI
- **OpenTelemetry** - Observability

## 📝 Notes

- The project uses **separate .csproj files** for each layer to enforce compile-time dependency rules
- Old folder structure in `AspireApp.ApiService/` is excluded from compilation but kept for reference
- All layers follow the same namespace convention: `AspireApp.ApiService.{Layer}.*`
- Database seeding runs automatically on application startup
- **Access tokens** expire after 60 minutes (configurable in `appsettings.json`)
- **Refresh tokens** expire after 7 days and are stored in the database
- Refresh tokens are automatically revoked when used to generate new tokens (token rotation)
- Token reuse detection: If a revoked refresh token is reused, all tokens for that user are revoked (security best practice)
- Expired refresh tokens can be cleaned up using `RefreshTokenRepository.CleanupExpiredTokensAsync()`
- **Permission system**: Supports both role-based permissions (inherited through roles) and direct user permissions (assigned directly to users)
- Direct user permissions take precedence over role-based permissions when checking access

## 🔒 Security Considerations

- **JWT Secret Key**: Change the default secret key in production
- **Refresh Tokens**: Refresh tokens are stored securely in the database and automatically revoked after use
- **Token Rotation**: Each refresh token can only be used once - new tokens are issued and old ones are revoked
- **Token Reuse Detection**: If a revoked refresh token is reused (indicating potential theft), all tokens for that user are immediately revoked
- **Permission System**: Direct user permissions take precedence over role-based permissions, allowing fine-grained access control and exceptions
- **Connection Strings**: Use secure storage (Azure Key Vault, User Secrets, etc.)
- **HTTPS**: Always use HTTPS in production
- **CORS**: Configure CORS appropriately for your frontend
- **Rate Limiting**: Consider adding rate limiting for production, especially on authentication endpoints
- **Token Storage**: Store refresh tokens securely on the client side (consider httpOnly cookies for web applications)

## 📚 Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

---

**Happy Coding! 🚀**

