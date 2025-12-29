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

The application provides a complete user management system with roles and permissions, allowing fine-grained access control to resources. It supports both role-based permissions and direct user permission assignment, providing maximum flexibility for access control. It includes a secure refresh token mechanism for seamless token renewal without requiring users to re-authenticate. The application also features comprehensive activity logging with automatic entity change tracking, domain events, and structured logging with Serilog.

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
  - Domain entities (User, Role, Permission, UserRole, UserPermission, RolePermission, RefreshToken, ActivityLog, etc.)
  - Domain interfaces (repositories, services)
  - Value objects (PasswordHash, etc.)
  - Domain services (managers)
  - Domain events (IDomainEvent, EntityChangedEvent, etc.)
  - Enums and constants
- **Dependencies**: None (pure domain logic)
- **Key Files**:
  - `Entities/` - Core domain models
  - `Interfaces/` - Contracts for repositories and services
  - `Services/` - Domain service interfaces
  - `Common/` - Domain events and base classes

#### 2. **Application Layer** (`AspireApp.ApiService.Application`)
- **Purpose**: Application use cases and business workflows
- **Contains**:
  - Use cases (LoginUserUseCase, RegisterUserUseCase, RefreshTokenUseCase, CreateRoleUseCase, User management use cases, ActivityLog use cases, etc.)
  - DTOs (Data Transfer Objects)
  - AutoMapper profiles
  - FluentValidation validators
  - Activity logging services (CentralizedActivityLogger, SimpleActivityLogger)
- **Dependencies**: Domain layer only
- **Key Files**:
  - `UseCases/` - Business logic implementations
    - `Auth/` - Authentication use cases (register, login, refresh token)
    - `Users/` - User management (CRUD, activation, password, roles, permissions)
    - `Roles/` - Role management use cases
    - `Permissions/` - Permission management use cases
    - `ActivityLogs/` - Activity log retrieval use cases
  - `DTOs/` - Data transfer objects
  - `Mappings/` - AutoMapper configurations
  - `Validators/` - Input validation rules
  - `ActivityLogs/` - Activity logging implementations

#### 3. **Infrastructure Layer** (`AspireApp.ApiService.Infrastructure`)
- **Purpose**: External concerns and data access
- **Contains**:
  - Entity Framework Core DbContext
  - Repository implementations
  - JWT token service
  - Password hashing service
  - Authorization handlers
  - Database migrations
  - Domain event dispatcher and handlers
  - Entity change tracking
  - Activity log storage
- **Dependencies**: Domain layer only
- **Key Files**:
  - `Data/ApplicationDbContext.cs` - EF Core context with domain event dispatching
  - `Repositories/` - Repository implementations
  - `Identity/TokenService.cs` - JWT token generation
  - `Authorization/` - Permission-based authorization
  - `DomainEvents/` - Domain event dispatcher and handlers
  - `Helpers/` - Entity change tracking utilities

#### 4. **Presentation Layer** (`AspireApp.ApiService.Presentation`)
- **Purpose**: API endpoints and HTTP concerns
- **Contains**:
  - Minimal API endpoints
  - Authorization attributes
  - Endpoint extensions (RequirePermission, RequireRole)
  - Result mapping extensions
- **Dependencies**: Application layer
- **Key Files**:
  - `Endpoints/` - API endpoint definitions (Auth, Users, Roles, Permissions, ActivityLogs)
  - `Attributes/` - Custom authorization attributes
  - `Extensions/` - Endpoint and result extension methods

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
AspireApp/
├── AspireApp.ApiService/              # Main API project (entry point)
│   ├── Program.cs                      # Application startup
│   ├── appsettings.json               # Configuration
│   └── AspireApp.ApiService.csproj
│
├── AspireApp.ApiService.Domain/        # Domain Layer
│   ├── Common/                         # Domain utilities, domain events
│   ├── Entities/                       # Domain entities (User, Role, Permission, ActivityLog, etc.)
│   ├── Enums/                          # Domain enums (ActivitySeverity, etc.)
│   ├── Interfaces/                     # Domain contracts (repositories, services)
│   ├── Permissions/                    # Permission definitions
│   ├── Roles/                          # Role name constants
│   ├── Services/                       # Domain service interfaces
│   └── ValueObjects/                   # Value objects (PasswordHash, etc.)
│
├── AspireApp.ApiService.Application/   # Application Layer
│   ├── ActivityLogs/                   # Activity logging implementations
│   ├── Common/                         # Base classes (BaseUseCase, Result)
│   ├── DTOs/                           # Data Transfer Objects
│   ├── Extensions/                     # Service registration extensions
│   ├── Mappings/                       # AutoMapper profiles
│   ├── UseCases/                       # Business logic
│   │   ├── ActivityLogs/              # Activity log use cases
│   │   ├── Authentication/            # Auth use cases (register, login, refresh)
│   │   ├── Permissions/               # Permission management
│   │   ├── Roles/                     # Role management
│   │   └── Users/                     # User management
│   └── Validators/                     # FluentValidation validators
│
├── AspireApp.ApiService.Infrastructure/# Infrastructure Layer
│   ├── Authorization/                  # Authorization handlers
│   ├── Data/                           # EF Core DbContext
│   ├── DomainEvents/                   # Domain event dispatcher and handlers
│   ├── Extensions/                     # Extension methods
│   ├── Helpers/                         # Helper utilities (EntityChangeTracker)
│   ├── Identity/                       # Identity services (TokenService)
│   ├── Migrations/                     # Database migrations
│   ├── Repositories/                   # Repository implementations
│   └── Services/                       # Infrastructure services
│
├── AspireApp.ApiService.Presentation/  # Presentation Layer
│   ├── Attributes/                     # Custom attributes (legacy)
│   ├── Endpoints/                      # API endpoints (Auth, Users, Roles, Permissions, ActivityLogs)
│   ├── Extensions/                     # Extension methods (RequirePermission, RequireRole)
│   └── Filters/                        # Action filters
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

#### Registration Flow
1. User submits registration details via `/api/auth/register` endpoint
2. `RegisterUserUseCase` validates input and checks for duplicate email/username
3. `PasswordHasher` hashes the password
4. `UserManager` creates the user entity
5. Default "User" role is assigned automatically
6. User is saved to database
7. User DTO returned to client

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

### Activity Logging

The application includes a comprehensive activity logging system:

1. **Automatic Entity Change Tracking**: Domain events are automatically raised when entities are created, updated, or deleted
2. **Centralized Activity Logger**: HTTP context-aware logger that automatically extracts user info, IP address, and user agent
3. **Activity Log Storage**: All activities are stored in the database with rich metadata
4. **Querying**: Activity logs can be filtered by user, entity, type, severity, date range, and more
5. **Pagination**: Efficient pagination support for large log datasets

Activity logs are permanent records (no soft deletion) to maintain a complete audit trail.

### Domain Events

The application implements Domain-Driven Design (DDD) domain events:

1. **Entity Change Events**: Automatically raised when entities are created, updated, or deleted
2. **Event Dispatching**: Events are dispatched after successful database saves
3. **Event Handlers**: Infrastructure handlers can react to domain events (e.g., activity logging)
4. **Change Tracking**: Entity changes are tracked automatically via EF Core change tracker

This enables decoupled, event-driven architecture patterns while maintaining transactional consistency.

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
   cd AspireApp
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
using AspireApp.ApiService.Domain.Permissions;
using AspireApp.ApiService.Presentation.Extensions;

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
        .RequirePermission(PermissionNames.Product.Write)
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
```
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

**Register:**
```http
POST https://localhost:7XXX/api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "userName": "newuser",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123!"
}
```

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

**Get Products (requires Product.Read permission or User/Manager/Admin role):**
```http
GET https://localhost:7XXX/api/products
Authorization: Bearer <your-access-token>
```

### Activity Logs API

**Get Activity Logs (requires ActivityLog.Read permission):**
```http
GET https://localhost:7XXX/api/activity-logs?pageNumber=1&pageSize=50&activityType=UserCreated&severity=Info
Authorization: Bearer <your-access-token>
```

**Query Parameters:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 50)
- `searchKeyword` (string): Search in description
- `activityType` (string): Filter by activity type
- `userId` (Guid): Filter by user ID
- `entityId` (Guid): Filter by entity ID
- `entityType` (string): Filter by entity type
- `severity` (ActivitySeverity): Filter by severity (Info, Low, Medium, High, Critical)
- `startDate` (DateTime): Filter by start date
- `endDate` (DateTime): Filter by end date
- `isPublic` (bool): Filter by public/private logs

### User Management API

The application provides comprehensive user management endpoints:

**Get All Users (requires User.Read permission):**
```http
GET https://localhost:7XXX/api/users
Authorization: Bearer <your-access-token>
```

**Get User by ID:**
```http
GET https://localhost:7XXX/api/users/{userId}
Authorization: Bearer <your-access-token>
```

**Get Current User (any authenticated user):**
```http
GET https://localhost:7XXX/api/users/me
Authorization: Bearer <your-access-token>
```

**Create User (requires User.Write permission):**
```http
POST https://localhost:7XXX/api/users
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "email": "newuser@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123!"
}
```

**Update User Information:**
```http
PUT https://localhost:7XXX/api/users/{userId}
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Updated"
}
```

**Update Password (any authenticated user can update their own password):**
```http
PUT https://localhost:7XXX/api/users/{userId}/password
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePassword456!"
}
```

**Toggle User Activation (activate/deactivate user):**
```http
PUT https://localhost:7XXX/api/users/{userId}/activation
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "isActive": false
}
```

**Assign Roles to User (replaces existing roles):**
```http
PUT https://localhost:7XXX/api/users/{userId}/roles
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "roleIds": [1, 2]
}
```

**Assign Permissions to User (replaces existing direct permissions):**
```http
PUT https://localhost:7XXX/api/users/{userId}/permissions
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "permissionIds": [1, 2, 3]
}
```

### Activity Logging

The application includes a comprehensive activity logging system that automatically tracks entity changes and supports manual activity logging.

#### Using Activity Logging in Use Cases

**Example: Logging a user creation activity**
```csharp
using AspireApp.ApiService.Domain.Interfaces;

public class CreateUserUseCase : BaseUseCase
{
    private readonly IActivityLogger _activityLogger;
    
    public CreateUserUseCase(IActivityLogger activityLogger, ...)
    {
        _activityLogger = activityLogger;
    }
    
    public async Task<Result<UserDto>> ExecuteAsync(CreateUserDto dto)
    {
        // ... create user logic ...
        
        // Log the activity
        await _activityLogger.LogAsync(
            activityType: "UserCreated",
            descriptionTemplateKey: "User {UserName} was created",
            descriptionParameters: new Dictionary<string, object>
            {
                { "UserName", user.UserName }
            },
            entityId: user.Id,
            entityType: "User",
            severity: ActivitySeverity.Info,
            tags: new[] { "user-management", "creation" }
        );
        
        return Result.Success(userDto);
    }
}
```

#### Automatic Entity Change Tracking

Entity changes are automatically tracked via domain events. When an entity is created, updated, or deleted, a domain event is raised and can be handled by event handlers (e.g., for activity logging).

**Excluding entities from logging:**
```csharp
[ExcludeFromLogging]
public class SomeEntity : BaseEntity
{
    // This entity will not generate automatic activity logs
}
```

#### Activity Log Querying

Activity logs support comprehensive filtering:
- **By User**: Filter logs for a specific user
- **By Entity**: Filter logs for a specific entity (e.g., all changes to a specific order)
- **By Type**: Filter by activity type (e.g., "UserCreated", "OrderUpdated")
- **By Severity**: Filter by severity level (Info, Low, Medium, High, Critical)
- **By Date Range**: Filter logs within a specific time period
- **By Keyword**: Search in log descriptions
- **Public/Private**: Filter by visibility

See the Activity Logs API section above for query examples.

### Working with Permissions and Roles

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
       public static class Product
       {
           public const string Read = "Product.Read";
           public const string Write = "Product.Write";
           public const string Delete = "Product.Delete";
       }
   }
   ```

2. **Use in endpoint** with the `RequirePermission` extension method:
   ```csharp
   using AspireApp.ApiService.Domain.Permissions;
   using AspireApp.ApiService.Presentation.Extensions;
   
   group.MapPost("/", CreateProduct)
       .RequirePermission(PermissionNames.Product.Write);
   ```

#### Defining Roles

1. **Role names are defined** in `Domain/Roles/RoleNames.cs`:
   ```csharp
   public static class RoleNames
   {
       public const string Admin = "Admin";
       public const string Manager = "Manager";
       public const string User = "User";
   }
   ```

2. **Use in endpoint** with the `RequireRole` extension method:
   ```csharp
   using AspireApp.ApiService.Domain.Roles;
   using AspireApp.ApiService.Presentation.Extensions;
   
   group.MapDelete("/{id}", DeleteProduct)
       .RequireRole(RoleNames.Admin);
   
   // Multiple roles (user needs at least one)
   group.MapGet("/", GetAllProducts)
       .RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Admin);
   ```

#### Extension Methods

The application provides fluent extension methods for authorization:

- **`RequirePermission(params string[] permissions)`**: Requires the user to have at least one of the specified permissions
- **`RequireRole(params string[] roles)`**: Requires the user to have at least one of the specified roles

**Example usage:**
```csharp
// Single permission
group.MapGet("/", GetProducts)
    .RequirePermission(PermissionNames.Product.Read);

// Multiple permissions (user needs at least one)
group.MapPost("/", CreateProduct)
    .RequirePermission(PermissionNames.Product.Write, PermissionNames.Product.Create);

// Single role
group.MapDelete("/{id}", DeleteProduct)
    .RequireRole(RoleNames.Admin);

// Multiple roles (user needs at least one)
group.MapGet("/", GetProducts)
    .RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Admin);

// Combining with other endpoint configuration
group.MapPut("/{id}", UpdateProduct)
    .WithName("UpdateProduct")
    .RequirePermission(PermissionNames.Product.Write)
    .Produces<ProductDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
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

### User Management Operations

The application provides comprehensive user management through the following use cases:

| Use Case | Description |
|----------|-------------|
| `CreateUserUseCase` | Creates a new user with email, name, and password |
| `GetUserUseCase` | Retrieves a user by ID |
| `GetAllUsersUseCase` | Retrieves all users |
| `UpdateUserUseCase` | Updates user information (name, email) |
| `UpdatePasswordUseCase` | Updates user password (requires current password verification) |
| `ToggleUserActivationUseCase` | Activates or deactivates a user account |
| `AssignRoleToUserUseCase` | Assigns roles to a user (replaces existing roles with soft-delete support) |
| `AssignPermissionsToUserUseCase` | Assigns direct permissions to a user (replaces existing permissions with soft-delete support) |
| `DeleteUserUseCase` | Soft-deletes a user |
| `RemoveRoleFromUserUseCase` | Removes a specific role from a user |

**Key Implementation Details:**
- Role and permission assignments use **soft-delete pattern** - when re-assigning, previously deleted associations are restored rather than creating duplicates
- Password updates require **current password verification** for security
- User activation/deactivation allows temporary disabling of accounts without deletion

## ✨ Key Features

- ✅ **Clean Architecture** - Clear separation of concerns
- ✅ **JWT Authentication** - Secure token-based authentication with refresh tokens
- ✅ **User Registration** - Public registration endpoint with automatic role assignment
- ✅ **Refresh Token Mechanism** - Seamless token renewal without re-authentication with token rotation and reuse detection
- ✅ **RBAC Authorization** - Role and permission-based access control with fluent extension methods
- ✅ **Dual Permission System** - Both role-based and direct user permission assignment
- ✅ **Comprehensive User Management** - Full CRUD operations, password management, activation control, role/permission assignment
- ✅ **Activity Logging System** - Comprehensive activity tracking with automatic entity change tracking
- ✅ **Domain Events** - DDD-compliant domain events with automatic dispatching
- ✅ **Structured Logging** - Serilog integration with console, file, and JSON output
- ✅ **Minimal APIs** - Modern endpoint-based API design
- ✅ **Entity Framework Core** - Code-first database approach
- ✅ **Soft Delete Support** - Entities support soft deletion with restore capability
- ✅ **Unit of Work Pattern** - Transactional consistency with generic repository access
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
- **Serilog** - Structured logging framework
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
- **Extension methods**: Use `RequirePermission()` and `RequireRole()` extension methods for clean, fluent endpoint authorization
- **Type-safe constants**: Use `PermissionNames` and `RoleNames` static classes instead of magic strings for better maintainability
- **Soft Delete**: Entities support soft deletion - records are marked as deleted rather than physically removed, with ability to restore
- **Unit of Work**: Provides transactional consistency and generic repository access via `UnitOfWork.GetRepository<TEntity>()`
- **User Management**: Complete user lifecycle management including creation, updates, password changes, activation/deactivation, and role/permission assignment
- **Activity Logging**: Comprehensive activity tracking system with automatic entity change tracking via domain events
- **Domain Events**: DDD-compliant domain events are automatically raised for entity changes and dispatched after successful saves
- **Structured Logging**: Serilog configured with console, file (text and JSON), and rolling file support (30-day retention)
- **User Registration**: Public registration endpoint automatically assigns default "User" role to new users
- **Activity Logs**: Permanent audit trail - activity logs do not support soft deletion to maintain complete history

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
- **Activity Logging**: Activity logs capture sensitive information (IP addresses, user agents) - ensure proper access controls
- **Password Security**: Passwords are hashed using secure algorithms - never store plain text passwords
- **Input Validation**: All inputs are validated using FluentValidation to prevent injection attacks

## 📚 Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

---

**Happy Coding! 🚀**

