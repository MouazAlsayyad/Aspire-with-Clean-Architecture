# AspireApp - .NET Aspire Application with Modular Monolith Architecture

A modern .NET Aspire application built using **Modular Monolith** architecture with **Domain-Driven Design (DDD)** principles, featuring authentication, authorization (RBAC), and comprehensive user, role, and permission management.

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
- **Modular Monolith Architecture** - Self-contained modules with clear boundaries following DDD principles
- **Domain-Driven Design** - Rich domain models, domain services, and domain events
- **Clean Architecture** - Separation of concerns across multiple layers
- **Authentication & Authorization** - JWT-based authentication with refresh tokens and Role-Based Access Control (RBAC)
- **Microservices-Ready** - Built with .NET Aspire for distributed application development, modules can be extracted into microservices
- **Modern API Design** - Minimal APIs with endpoint-based routing

The application provides a complete user management system with roles and permissions, allowing fine-grained access control to resources. It supports both role-based permissions and direct user permission assignment, providing maximum flexibility for access control. It includes a secure refresh token mechanism for seamless token renewal without requiring users to re-authenticate. The application also features comprehensive activity logging with automatic entity change tracking, domain events, and structured logging with Serilog.

## 🏛️ Architecture Pattern

This project follows a **Modular Monolith** architecture where each feature/module is organized as a self-contained unit following Domain-Driven Design principles. Modules are complete, independent projects containing their own Domain, Application, and Infrastructure layers.

**Current Modules:**
- **`AspireApp.ApiService.Notifications`** - Notification system with Firebase Cloud Messaging (Reference Pattern)
- **`AspireApp.Modules.ActivityLogs`** - Comprehensive activity logging and audit trail
- **`AspireApp.Modules.FileUpload`** - File upload and storage management
- **`AspireApp.Twilio`** - Twilio SMS and OTP integration

**Key Benefits:**
- **True Modularity**: Each module is a complete, self-contained project with clear boundaries
- **Dynamic Service Registration**: Modules are automatically discovered and registered at runtime using reflection
- **No Circular Dependencies**: Modules avoid direct project references through dynamic assembly loading
- **Domain Focus**: Business logic is centralized in domain services (Managers)
- **Testability**: Easy to unit test each module independently
- **Maintainability**: Changes are isolated to specific modules
- **Scalability**: Modules can be extracted into microservices without refactoring
- **Shared Infrastructure**: All modules share a common DbContext while maintaining their own entity configurations

## 🏗️ Architecture

This project follows **Modular Monolith** architecture with **Domain-Driven Design (DDD)** principles, organizing code into distinct layers with clear dependencies. Each module follows the same architectural pattern:

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

### Dynamic Service Registration

The application uses **dynamic assembly loading** to avoid circular dependencies and enable automatic module discovery:

**How It Works:**
1. **Assembly Discovery**: At startup, `AppDomain.CurrentDomain.GetAssemblies()` discovers loaded module assemblies
2. **Automatic Registration**: Module services (UseCases, Repositories, Domain Managers, Event Handlers) are automatically registered
3. **No Hard References**: Infrastructure layer doesn't directly reference module projects, preventing circular dependencies
4. **Configuration Loading**: Module entity configurations (EF Core) are dynamically applied to the shared DbContext

**Key Components:**
- **`ServiceCollectionExtensions.AddUseCases()`** - Dynamically registers use cases from all loaded assemblies
- **`ApplicationServiceExtensions`** - Dynamically registers AutoMapper profiles and FluentValidation validators
- **`ServiceCollectionExtensions.AddRepositories()`** - Dynamically discovers and registers repository implementations
- **`ServiceCollectionExtensions.AddDomainManagers()`** - Dynamically registers domain service implementations
- **`ApplicationDbContext.OnModelCreating()`** - Dynamically applies entity configurations from module assemblies

**Benefits:**
- ✅ No circular dependencies between projects
- ✅ Modules can be added without modifying core infrastructure code
- ✅ Seamless integration of new modules at runtime
- ✅ Maintains clean separation of concerns

### Layer Responsibilities

#### 1. **Domain Layer** (`AspireApp.ApiService.Domain`)
- **Purpose**: Core business logic and entities for the main API service
- **Contains**:
  - Domain entities (User, Role, Permission, UserRole, UserPermission, RolePermission, RefreshToken)
  - Domain interfaces (repositories, services)
  - Value objects (PasswordHash, etc.)
  - Domain services (UserManager, RoleManager, PermissionManager)
  - Domain events (IDomainEvent, EntityChangedEvent)
  - Authentication interfaces (IPasswordHasher, ITokenService, IFirebaseAuthService)
  - Enums and constants
- **Dependencies**: None (pure domain logic)
- **Key Files**:
  - `Users/` - User aggregate and services
  - `Roles/` - Role aggregate and services
  - `Permissions/` - Permission aggregate and services
  - `Authentication/` - Authentication entities and interfaces
  - `Services/` - DomainService base class
  - `ValueObjects/` - Value objects (PasswordHash)
- **Note**: Module-specific domains (Notifications, ActivityLogs, FileUpload) are in their respective module projects

#### 2. **Application Layer** (`AspireApp.ApiService.Application`)
- **Purpose**: Application use cases and business workflows for the main API service
- **Contains**:
  - Use cases (LoginUserUseCase, RegisterUserUseCase, RefreshTokenUseCase, User management, Role management, Permission management)
  - DTOs (Data Transfer Objects)
  - AutoMapper profiles
  - FluentValidation validators
  - Base classes (BaseUseCase, Result pattern)
- **Dependencies**: Domain layer only
- **Key Files**:
  - `Authentication/` - Authentication use cases and DTOs (register, login, refresh token)
  - `Users/` - User management use cases (CRUD, activation, password, roles, permissions)
  - `Roles/` - Role management use cases and DTOs
  - `Permissions/` - Permission management use cases and DTOs
  - `Common/` - BaseUseCase and Result pattern
  - `Extensions/` - Service registration extensions
- **Note**: Module-specific application logic (Notifications, ActivityLogs, FileUpload) are in their respective module projects

#### 3. **Infrastructure Layer** (`AspireApp.ApiService.Infrastructure`)
- **Purpose**: External concerns and data access for the main API service
- **Contains**:
  - Entity Framework Core DbContext (shared by all modules)
  - Repository implementations (User, Role, Permission, RefreshToken)
  - JWT token service
  - Password hashing service
  - Authorization handlers (PermissionAuthorizationHandler, PermissionPolicyProvider)
  - Database migrations (includes all entities from modules)
  - Domain event dispatcher
  - Entity change tracking
  - Background task queue and hosted services
- **Dependencies**: Domain layer only (no direct dependency on module projects to avoid circular references)
- **Key Files**:
  - `Data/ApplicationDbContext.cs` - Shared EF Core context with dynamic module configuration loading
  - `Repositories/` - Core repository implementations (User, Role, Permission, RefreshToken)
  - `Identity/TokenService.cs` - JWT token generation
  - `Authorization/` - Permission-based authorization handlers
  - `DomainEvents/` - Domain event dispatcher and entity change tracking
  - `Services/` - Background task queue and hosted services
  - `Extensions/ServiceCollectionExtensions.cs` - Dynamic service registration for modules
- **Note**: Module-specific infrastructure (Notifications, ActivityLogs, FileUpload repositories and services) are in their respective module projects

#### 4. **Presentation Layer** (`AspireApp.ApiService.Presentation`)
- **Purpose**: API endpoints and HTTP concerns for all services and modules
- **Contains**:
  - Minimal API endpoints for core services and modules
  - Authorization attributes
  - Endpoint extensions (RequirePermission, RequireRole)
  - Result mapping extensions
- **Dependencies**: Application layer and module Application layers
- **Key Files**:
  - `Authentication/` - Authentication endpoints (register, login, refresh token)
  - `Users/` - User management endpoints
  - `Roles/` - Role management endpoints
  - `Permissions/` - Permission management endpoints
  - `Notifications/` - Notification module endpoints
  - `ActivityLogs/` - Activity log module endpoints
  - `FileUpload/` - File upload module endpoints
  - `Attributes/` - Custom authorization attributes
  - `Extensions/` - Endpoint extensions (RequirePermission, RequireRole, ResultExtensions)

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

#### 7. **Notifications Module** (`AspireApp.ApiService.Notifications`)
- **Purpose**: Self-contained notification module following DDD principles
- **Structure**: Complete module with Domain, Application, and Infrastructure layers
- **Features**:
  - Firebase Cloud Messaging (FCM) integration
  - Bilingual support (English/Arabic)
  - Localization system with JSON resources
  - Domain event-driven architecture (NotificationCreatedEvent)
  - Cursor-based pagination
  - Notification management (Create, Read, Update, Mark as Read, Delete)
  - FCM token registration
- **Key Components**:
  - `Domain/` - Notification entity, NotificationManager, Firebase service interfaces
  - `Application/` - Create, Get, Update, Mark as Read use cases
  - `Infrastructure/` - NotificationRepository, FirebaseFCMService, FirebaseAuthService, NotificationHandler
- **Reference Pattern**: This module serves as the reference implementation for creating new modules

#### 8. **Activity Logs Module** (`AspireApp.Modules.ActivityLogs`)
- **Purpose**: Comprehensive activity logging and audit trail system
- **Structure**: Complete module with Domain, Application, and Infrastructure layers
- **Features**:
  - Automatic entity change tracking
  - Domain event-driven logging (EntityChangedEvent)
  - Rich metadata capture (user, IP address, user agent, severity)
  - Permanent audit trail (no soft deletion)
  - Advanced filtering (by user, entity, operation, severity, date range)
  - Efficient pagination
- **Key Components**:
  - `Domain/` - ActivityLog entity, IActivityLogger, IActivityLogStore interfaces
  - `Application/` - GetActivityLogsUseCase, CentralizedActivityLogger, SimpleActivityLogger
  - `Infrastructure/` - ActivityLogRepository, ActivityLogConfiguration

#### 9. **File Upload Module** (`AspireApp.Modules.FileUpload`)
- **Purpose**: File upload and storage management system
- **Structure**: Complete module with Domain, Application, and Infrastructure layers
- **Features**:
  - Multiple storage strategies (FileSystem, Database, R2/S3)
  - File type validation and detection
  - File size limits
  - MIME type validation
  - Metadata storage
  - File deletion and retrieval
- **Key Components**:
  - `Domain/` - FileUpload entity, FileUploadManager, file validation helpers
  - `Application/` - Upload, Get, Delete file use cases
  - `Infrastructure/` - FileUploadRepository, FileStorageStrategyFactory, storage strategy implementations

## 📁 Project Structure

```
AspireApp/
├── AspireApp.ApiService/              # Main API project (entry point)
│   ├── Program.cs                      # Application startup
│   ├── appsettings.json               # Configuration
│   └── AspireApp.ApiService.csproj
│
├── AspireApp.ApiService.Domain/        # Domain Layer (Core API Service)
│   ├── Authentication/                # Authentication domain
│   │   ├── Entities/                   # RefreshToken entity
│   │   └── Interfaces/                 # IPasswordHasher, IRefreshTokenRepository, ITokenService, IFirebaseAuthService
│   ├── Common/                         # Domain utilities and base classes
│   │   ├── DomainException.cs          # Domain exception handling
│   │   ├── EntityChangedEvent.cs       # Entity change domain events
│   │   ├── IAggregateRoot.cs           # Aggregate root interface
│   │   └── IDomainEvent.cs             # Domain event interface
│   ├── Entities/                       # Base entity (BaseEntity)
│   ├── Interfaces/                     # Core domain interfaces
│   │   ├── IBackgroundTaskQueue.cs     # Background task queue interface
│   │   ├── IDomainEventDispatcher.cs    # Domain event dispatcher interface
│   │   ├── IDomainService.cs            # Domain service base interface
│   │   ├── IRepository.cs              # Generic repository interface
│   │   └── IUnitOfWork.cs               # Unit of work interface
│   ├── Permissions/                    # Permission domain
│   │   ├── Entities/                    # Permission entity
│   │   ├── Interfaces/                 # IPermissionRepository
│   │   ├── PermissionNames.cs           # Permission name constants
│   │   └── Services/                    # PermissionManager domain service
│   ├── Roles/                          # Role domain
│   │   ├── Entities/                    # Role, RolePermission entities
│   │   ├── Enums/                       # RoleType enum
│   │   ├── Interfaces/                 # IRoleRepository
│   │   ├── RoleNames.cs                 # Role name constants
│   │   └── Services/                    # RoleManager domain service
│   ├── Services/                        # DomainService base class
│   ├── Users/                          # User domain
│   │   ├── Entities/                    # User, UserRole, UserPermission entities
│   │   ├── Interfaces/                 # IUserRepository
│   │   └── Services/                    # UserManager domain service
│   └── ValueObjects/                   # Value objects (PasswordHash)
│
├── AspireApp.ApiService.Application/   # Application Layer (Core API Service)
│   ├── Authentication/                  # Authentication use cases
│   │   ├── DTOs/                        # Auth DTOs (LoginRequest, RegisterRequest, etc.)
│   │   ├── Mappings/                    # Auth AutoMapper profiles
│   │   ├── UseCases/                    # LoginUserUseCase, RegisterUserUseCase, RefreshTokenUseCase
│   │   └── Validators/                  # Auth FluentValidation validators
│   ├── Common/                          # Base classes
│   │   ├── BaseUseCase.cs               # Base use case class
│   │   └── Result.cs                    # Result pattern implementation
│   ├── Extensions/                      # Service registration extensions
│   │   ├── ServiceCollectionExtensions.cs  # Dynamic UseCase registration
│   │   └── ApplicationServiceExtensions.cs # AutoMapper and FluentValidation registration
│   ├── Permissions/                     # Permission use cases
│   │   ├── DTOs/                        # Permission DTOs
│   │   ├── Mappings/                    # Permission AutoMapper profiles
│   │   ├── UseCases/                    # Permission CRUD use cases
│   │   └── Validators/                  # Permission validators
│   ├── Roles/                           # Role use cases
│   │   ├── DTOs/                        # Role DTOs
│   │   ├── Mappings/                    # Role AutoMapper profiles
│   │   ├── UseCases/                    # Role CRUD use cases
│   │   └── Validators/                  # Role validators
│   └── Users/                           # User use cases
│       ├── DTOs/                        # User DTOs
│       ├── Mappings/                    # User AutoMapper profiles
│       ├── UseCases/                    # User CRUD, password, activation use cases
│       └── Validators/                  # User validators
│
├── AspireApp.ApiService.Infrastructure/# Infrastructure Layer (Shared by all modules)
│   ├── Authorization/                  # Authorization handlers
│   │   ├── PermissionAuthorizationHandler.cs
│   │   └── PermissionPolicyProvider.cs
│   ├── Data/                            # EF Core DbContext (shared by all modules)
│   │   ├── ApplicationDbContext.cs     # Main DbContext with dynamic module configuration loading
│   │   ├── ApplicationDbContextFactory.cs
│   │   ├── DatabaseSeeder.cs           # Database seeding logic
│   │   └── EntityConfigurations/        # EF Core entity configurations (core entities only)
│   ├── DomainEvents/                    # Domain event dispatcher
│   │   ├── DomainEventDispatcher.cs
│   │   └── EntityChangeTrackingHandler.cs
│   ├── Extensions/                      # Extension methods
│   │   ├── AuthenticationExtensions.cs
│   │   └── ServiceCollectionExtensions.cs  # Dynamic repository and service registration
│   ├── Helpers/                         # Helper utilities
│   │   ├── EntityChangeTracker.cs
│   │   └── SensitiveDataFilter.cs
│   ├── Identity/                        # Identity services
│   │   └── TokenService.cs              # JWT token service
│   ├── Middleware/                      # Custom middleware
│   │   └── RequestLoggingMiddleware.cs
│   ├── Migrations/                      # Database migrations (includes all entities from modules)
│   ├── Repositories/                    # Core repository implementations
│   │   ├── PermissionRepository.cs
│   │   ├── RefreshTokenRepository.cs
│   │   ├── Repository.cs                # Generic repository base
│   │   ├── RoleRepository.cs
│   │   ├── UnitOfWork.cs                # Unit of work implementation
│   │   └── UserRepository.cs
│   └── Services/                        # Infrastructure services
│       ├── BackgroundTaskQueue.cs      # Background task queue
│       ├── PasswordHasher.cs            # Password hashing service
│       └── QueuedHostedService.cs       # Background task hosted service
│
├── AspireApp.ApiService.Presentation/  # Presentation Layer (All API endpoints)
│   ├── ActivityLogs/                    # Activity log module endpoints
│   │   └── ActivityLogEndpoints.cs
│   ├── Attributes/                      # Custom authorization attributes
│   │   ├── AuthorizePermissionAttribute.cs
│   │   └── AuthorizeRoleAttribute.cs
│   ├── Authentication/                  # Authentication endpoints
│   │   └── AuthEndpoints.cs
│   ├── Extensions/                      # Extension methods
│   │   ├── EndpointRouteBuilderExtensions.cs
│   │   ├── PresentationServiceExtensions.cs
│   │   ├── ResultExtensions.cs
│   │   └── RouteHandlerBuilderExtensions.cs  # RequirePermission, RequireRole
│   ├── FileUpload/                      # File upload module endpoints
│   │   └── FileUploadEndpoints.cs
│   ├── Filters/                         # Action filters
│   │   └── ValidationFilter.cs
│   ├── Notifications/                   # Notification module endpoints
│   │   └── NotificationEndpoints.cs
│   ├── Permissions/                     # Permission endpoints
│   │   └── PermissionEndpoints.cs
│   ├── Roles/                           # Role endpoints
│   │   └── RoleEndpoints.cs
│   └── Users/                           # User endpoints
│       └── UserEndpoints.cs
│
├── AspireApp.ApiService.Notifications/ # Notifications Module (Reference Pattern)
│   ├── Domain/                          # Domain Layer
│   │   ├── Entities/                    # Notification entity
│   │   │   └── Notification.cs          # Notification aggregate root
│   │   ├── Enums/                       # Notification enums
│   │   │   ├── NotificationType.cs
│   │   │   ├── NotificationPriority.cs
│   │   │   ├── NotificationStatus.cs
│   │   │   └── NotificationTimeFilter.cs
│   │   ├── Events/                      # Domain events
│   │   │   └── NotificationCreatedEvent.cs
│   │   ├── Interfaces/                  # Domain service interfaces
│   │   │   ├── INotificationManager.cs
│   │   │   ├── INotificationRepository.cs
│   │   │   ├── IFirebaseFCMService.cs
│   │   │   └── IFirebaseNotificationManager.cs
│   │   ├── Resources/                   # Localization resources (JSON)
│   │   └── Services/                    # Domain services (Managers)
│   │       ├── NotificationManager.cs
│   │       └── LocalizationService.cs
│   ├── Application/                     # Application Layer
│   │   ├── DTOs/                        # Notification DTOs
│   │   │   ├── CreateNotificationDto.cs
│   │   │   ├── NotificationDto.cs
│   │   │   ├── RegisterFCMTokenDto.cs
│   │   │   └── LocalizedNotificationContent.cs
│   │   ├── Mappings/                    # AutoMapper profiles
│   │   │   └── NotificationMappingProfile.cs
│   │   ├── UseCases/                    # Notification use cases
│   │   │   ├── CreateNotificationUseCase.cs
│   │   │   ├── GetNotificationsUseCase.cs
│   │   │   ├── MarkAsReadUseCase.cs
│   │   │   ├── RegisterFCMTokenUseCase.cs
│   │   │   └── HasFCMTokenUseCase.cs
│   │   └── Validators/                  # FluentValidation validators
│   │       ├── CreateNotificationDtoValidator.cs
│   │       └── RegisterFCMTokenDtoValidator.cs
│   └── Infrastructure/                  # Infrastructure Layer
│       ├── Configurations/              # EF Core configurations
│       │   └── NotificationConfiguration.cs
│       ├── Handlers/                    # Domain event handlers
│       │   └── NotificationHandler.cs
│       ├── Repositories/                # Repository implementations
│       │   └── NotificationRepository.cs
│       └── Services/                    # External services (Firebase)
│           ├── FirebaseFCMService.cs    # Firebase Cloud Messaging
│           ├── FirebaseAuthService.cs   # Firebase Authentication
│           ├── FirebaseNotificationManager.cs
│           └── NotificationLocalizationInitializer.cs
│
├── AspireApp.Modules.ActivityLogs/    # Activity Logs Module
│   ├── Domain/                          # Domain Layer
│   │   ├── Entities/                    # ActivityLog entity
│   │   │   └── ActivityLog.cs
│   │   ├── Enums/                       # ActivityLog enums
│   │   │   ├── ActivitySeverity.cs
│   │   │   ├── ActivityType.cs
│   │   │   └── OperationType.cs
│   │   └── Interfaces/                  # Domain interfaces
│   │       ├── IActivityLogger.cs
│   │       └── IActivityLogStore.cs
│   ├── Application/                     # Application Layer
│   │   ├── DTOs/                        # ActivityLog DTOs
│   │   │   ├── ActivityLogDto.cs
│   │   │   └── GetActivityLogsRequestDto.cs
│   │   ├── Mappings/                    # AutoMapper profiles
│   │   │   └── ActivityLogMappingProfile.cs
│   │   ├── UseCases/                    # ActivityLog use cases
│   │   │   └── GetActivityLogsUseCase.cs
│   │   ├── CentralizedActivityLogger.cs # HTTP context-aware logger
│   │   └── SimpleActivityLogger.cs      # Simple activity logger
│   └── Infrastructure/                  # Infrastructure Layer
│       ├── Configurations/              # EF Core configurations
│       │   └── ActivityLogConfiguration.cs
│       └── Repositories/                # Repository implementations
│           └── ActivityLogRepository.cs
│
├── AspireApp.Modules.FileUpload/      # File Upload Module
│   ├── Domain/                          # Domain Layer
│   │   ├── Entities/                    # FileUpload entity
│   │   │   └── FileUpload.cs
│   │   ├── Enums/                       # FileUpload enums
│   │   │   ├── FileStorageType.cs
│   │   │   └── FileType.cs
│   │   ├── Helpers/                     # Domain helpers
│   │   │   ├── FileTypeHelper.cs
│   │   │   └── FileValidationHelper.cs
│   │   ├── Interfaces/                  # Domain interfaces
│   │   │   ├── IFileUploadRepository.cs
│   │   │   ├── IFileStorageStrategy.cs
│   │   │   └── IFileStorageStrategyFactory.cs
│   │   └── Services/                    # Domain services
│   │       └── FileUploadManager.cs
│   ├── Application/                     # Application Layer
│   │   ├── DTOs/                        # FileUpload DTOs
│   │   │   ├── FileUploadDto.cs
│   │   │   └── UploadFileRequestDto.cs
│   │   ├── Mappings/                    # AutoMapper profiles
│   │   │   └── FileUploadMappingProfile.cs
│   │   ├── UseCases/                    # FileUpload use cases
│   │   │   ├── UploadFileUseCase.cs
│   │   │   ├── GetFileUseCase.cs
│   │   │   └── DeleteFileUseCase.cs
│   │   └── Validators/                  # FluentValidation validators
│   │       └── UploadFileRequestDtoValidator.cs
│   └── Infrastructure/                  # Infrastructure Layer
│       ├── Configurations/              # EF Core configurations
│       │   └── FileUploadConfiguration.cs
│       ├── Repositories/                # Repository implementations
│       │   └── FileUploadRepository.cs
│       └── Services/                    # Storage strategy implementations
│           ├── DatabaseStorageStrategy.cs
│           ├── FileSystemStorageStrategy.cs
│           ├── R2StorageStrategy.cs
│           └── FileStorageStrategyFactory.cs
│
├── AspireApp.Twilio/                   # Twilio Integration Module
│   ├── Domain/                          # Domain Layer
│   │   ├── Entities/                    # Twilio entities (Message, Otp)
│   │   ├── Enums/                       # Twilio enums
│   │   └── Interfaces/                  # Twilio interfaces
│   ├── Application/                     # Application Layer (if needed)
│   └── Infrastructure/                  # Infrastructure Layer
│       ├── Configurations/              # EF Core configurations
│       ├── Repositories/                # Repository implementations
│       └── Services/                    # Twilio service implementations
│
├── AspireApp.Domain.Shared/            # Shared Domain Layer
│   ├── Common/                          # Common utilities
│   │   ├── DomainErrors.cs              # Standardized error definitions
│   │   ├── Error.cs                     # Error value object
│   │   ├── PaginationHelper.cs          # Pagination utilities
│   │   └── Result.cs                    # Result pattern
│   ├── Entities/                        # Base entity
│   │   └── BaseEntity.cs                # Base entity with soft delete and domain events
│   └── Interfaces/                      # Shared interfaces
│       ├── IDomainService.cs            # Domain service base interface
│       ├── IDomainEventDispatcher.cs    # Domain event dispatcher interface
│       ├── IRepository.cs               # Generic repository interface
│       └── IUnitOfWork.cs               # Unit of work interface
│
├── AspireApp.AppHost/                  # Aspire AppHost
│   ├── AppHost.cs                       # Service orchestration
│   └── AspireApp.AppHost.csproj
│
└── AspireApp.ServiceDefaults/          # Shared Aspire defaults
    └── Extensions.cs                    # Service defaults extension
```

### Module Structure (Notification Module as Reference)

Each module follows this structure:

```
AspireApp.ApiService.{Module}/
├── Domain/                             # Domain Layer (Pure Business Logic)
│   ├── Entities/                       # Aggregate roots and domain entities
│   ├── Enums/                          # Domain enumerations
│   ├── Events/                         # Domain events
│   ├── Interfaces/                     # Repository and service contracts
│   ├── Resources/                      # Localization resources (if applicable)
│   └── Services/                       # Domain service implementations (Managers)
│
├── Application/                        # Application Layer (Use Cases)
│   ├── DTOs/                           # Request/Response DTOs
│   ├── UseCases/                       # Use case handlers (inherit BaseUseCase)
│   ├── Validators/                     # FluentValidation validators
│   └── Mappings/                        # AutoMapper profiles
│
└── Infrastructure/                     # Infrastructure Layer (External Concerns)
    ├── Configurations/                 # EF Core entity configurations
    ├── Handlers/                        # Domain event handlers
    ├── Repositories/                    # EF Core repository implementations
    └── Services/                        # External service implementations (e.g., Firebase)
```

**Note**: The application follows a true modular architecture where feature modules (Notifications, ActivityLogs, FileUpload, Twilio) are self-contained projects with their own Domain, Application, and Infrastructure layers. Core services (Users, Roles, Permissions, Authentication) remain in the main API service projects.

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
- **Default roles** (Admin, Manager, User)
- **Default permissions** - Automatically detected from `PermissionNames.GetAllDefinitions()`
- **Default admin user** (email: `admin@example.com`, password: `Admin@123`)

#### Automatic Permission Management

The seeder includes intelligent permission management:

1. **Permission Detection**: Automatically detects all permissions defined in `PermissionNames.GetAllDefinitions()`
2. **Permission Creation**: Creates any missing permissions in the database
3. **Permission Restoration**: Restores soft-deleted permissions that exist in code
4. **Permission Cleanup**: Soft-deletes permissions that exist in database but not in code (orphaned permissions)

#### Automatic Admin Role Permission Assignment

The admin role automatically receives **all permissions** defined in the codebase:

- **On Initial Creation**: When roles are first created, admin gets all existing permissions
- **On Permission Updates**: When new permissions are added to `PermissionNames` (e.g., `FileUpload` permissions), they are automatically assigned to the admin role
- **Comparison Method**: Uses permission name comparison (not ID) for reliable detection of missing permissions
- **Automatic Sync**: Runs on every application startup to ensure admin always has the latest permissions

**Example**: If you add new `FileUpload` permissions to `PermissionNames`, they will automatically be:
1. Created in the database (if missing)
2. Assigned to the admin role (if not already assigned)

This ensures the admin role always has full access to all features without manual intervention.

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

### Background Task Queue

The application includes a structured background task processing system using `IBackgroundTaskQueue`:

#### Features

- **Structured Task Processing**: Uses `System.Threading.Channels` for efficient task queuing
- **Graceful Shutdown**: Tasks respect cancellation tokens and app lifecycle
- **Centralized Management**: Single queue for all background tasks
- **Error Handling**: Background tasks handle errors gracefully without affecting the main application
- **Lifecycle Management**: No orphaned tasks - all tasks are properly managed

#### Why Use IBackgroundTaskQueue Instead of Task.Run?

✅ **Graceful shutdown** via CancellationToken  
✅ **Centralized task queue** management  
✅ **Respects app lifecycle** (no orphaned tasks)  
✅ **Easier debugging** and logging  
✅ **Better for scaling** and reliability  

#### Usage Example

```csharp
// In a controller or service, inject IBackgroundTaskQueue:
public class MyController : ControllerBase
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    
    public MyController(IBackgroundTaskQueue backgroundTaskQueue)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
    }
    
    [HttpPost]
    public IActionResult ProcessData()
    {
        // Queue a background task instead of using Task.Run
        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            // Your fire-and-forget logic here
            await DoLongRunningWork(token);
        });
        
        return Ok("Task queued successfully");
    }
}
```

#### File Upload with Background Processing

The file upload endpoint supports background processing:

- **Synchronous** (`useBackgroundQueue=false`): Returns full file details after upload completes
- **Asynchronous** (`useBackgroundQueue=true`): Returns immediately with queued status, processes file in background

This provides faster response times for large file uploads while maintaining full control over the upload process.

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

6. **Configure Firebase** (optional, required for notifications)
   Edit `AspireApp.ApiService/appsettings.json`:
   ```json
   {
     "Firebase": {
       "ProjectId": "your-firebase-project-id",
       "SenderId": "your-firebase-sender-id",
       "WebApiKey": "your-firebase-web-api-key",
       "Auth": {
         "Enabled": true,
         "EmailVerificationRequired": false,
         "PasswordMinLength": 6
       },
       "ServiceAccount": {
         "type": "service_account",
         "project_id": "your-project-id",
         "private_key_id": "your-private-key-id",
         "private_key": "-----BEGIN PRIVATE KEY-----\\n...\\n-----END PRIVATE KEY-----\\n",
         "client_email": "your-service-account@your-project.iam.gserviceaccount.com",
         "client_id": "your-client-id",
         "auth_uri": "https://accounts.google.com/o/oauth2/auth",
         "token_uri": "https://oauth2.googleapis.com/token",
         "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
         "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/your-service-account%40your-project.iam.gserviceaccount.com",
         "universe_domain": "googleapis.com"
       }
     }
   }
   ```
   
   **Note**: To get your Firebase service account credentials:
   1. Go to Firebase Console → Project Settings → Service Accounts
   2. Click "Generate New Private Key"
   3. Copy the JSON content and map it to the `ServiceAccount` section in `appsettings.json`
   4. Replace `\n` in the private key with `\\n` for proper JSON formatting

7. **Run database migrations**
   ```bash
   cd AspireApp.ApiService
   dotnet ef database update --project ../AspireApp.ApiService.Infrastructure
   ```

8. **Run the application**
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
- **Root Path**: `https://localhost:7XXX/` automatically redirects to Scalar UI
- **OpenAPI/Swagger**: `https://localhost:7XXX/openapi/v1.json`
- **Scalar UI**: `https://localhost:7XXX/scalar/v1` (development only)
- **Health Check**: `https://localhost:7XXX/health`

## 💻 Development Guide

### Creating a New Module

When creating a new module, follow the **Notification module pattern** as your reference:

1. **Create Module Project**: Create `AspireApp.ApiService.{YourModule}/` following the Notification module structure
2. **Follow Layer Structure**: Implement Domain, Application, and Infrastructure layers within the module
3. **Use Domain Services**: Business logic goes in domain services (Managers), not use cases
4. **Register Services**: Register all services, repositories, and use cases in DI container
5. **Create Endpoints**: Add endpoints in `AspireApp.ApiService.Presentation/{Module}/`

### Adding a New Feature (Legacy Approach)

For features that haven't been modularized yet, follow this approach:

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

### Notification API

The application includes a complete notification system with Firebase Cloud Messaging support and Firebase Authentication integration. The Notification module serves as the reference pattern for modular architecture.

#### Firebase Integration

The notification system integrates with Firebase for both Cloud Messaging and Authentication:

- **Firebase Cloud Messaging (FCM)**: Sends push notifications to registered devices
- **Firebase Authentication**: Manages Firebase user accounts for push notification delivery
  - Automatically creates Firebase users when FCM tokens are registered
  - Retrieves Firebase UID for existing users by email
  - Handles Firebase initialization and credential management

**Firebase Authentication Service** (`FirebaseAuthService`):
- Creates Firebase users programmatically
- Retrieves Firebase UID by email
- Thread-safe initialization with singleton pattern
- Graceful error handling for missing configuration

**Key Features:**
- Bilingual support (English/Arabic) with automatic localization
- Firebase Cloud Messaging integration for push notifications
- Firebase Authentication service for user management
- Cursor-based pagination for efficient data retrieval
- Domain event-driven architecture
- Localization system with JSON resource files
- User language preference support
- FCM token management and registration

**Note:** The notification localization system is automatically initialized on application startup via `NotificationLocalizationInitializer` hosted service, which loads localization resources from JSON files.

**Create Notification (requires Notification.Write permission):**
```http
POST https://localhost:7XXX/api/notifications
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "type": "Info",
  "priority": "Normal",
  "title": "Welcome!",
  "titleAr": "مرحباً!",
  "message": "Welcome to AspireApp",
  "messageAr": "مرحباً بك في AspireApp",
  "userId": "user-guid",
  "actionUrl": "/dashboard"
}
```

**Get Notifications (requires Notification.Read permission):**
```http
GET https://localhost:7XXX/api/notifications?lastNotificationId={guid}&pageSize=20&timeFilter=All
Authorization: Bearer <your-access-token>
```

**Mark Notification as Read:**
```http
PUT https://localhost:7XXX/api/notifications/{notificationId}/read
Authorization: Bearer <your-access-token>
```

**Mark All Notifications as Read:**
```http
PUT https://localhost:7XXX/api/notifications/mark-all-read
Authorization: Bearer <your-access-token>
```

**Register FCM Token (for push notifications):**
```http
POST https://localhost:7XXX/api/notifications/register-fcm-token
Authorization: Bearer <your-access-token>
Content-Type: application/json

{
  "clientFcmToken": "firebase-cloud-messaging-token"
}
```

**Check if User Has FCM Token:**
```http
GET https://localhost:7XXX/api/notifications/has-fcm-token
Authorization: Bearer <your-access-token>
```

**Response:**
```json
{
  "value": true,
  "isSuccess": true
}
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

### File Upload API

The application provides a comprehensive file upload system with support for multiple storage types, file categories, and background processing.

#### Features

- **Multiple Storage Types**: Files can be stored in the file system, database, or Cloudflare R2
- **File Type Detection**: Automatically categorizes files as Image, Document, Video, Audio, or Other
- **File Integrity**: MD5 hash calculation for file integrity verification
- **Metadata Support**: Optional description and tags for file organization
- **User Tracking**: Tracks which user uploaded each file
- **Permission-Based Access**: Requires `FileUpload.Read`, `FileUpload.Write`, or `FileUpload.Delete` permissions
- **Background Processing**: Optional asynchronous file processing for faster response times
- **Domain-Driven Design**: Business logic encapsulated in `FileUploadManager` domain service

#### Storage Types

- **FileSystem** (default): Stores files on the server's file system
- **Database**: Stores files as binary data in the database (suitable for small files)
- **R2**: Stores files in Cloudflare R2 (S3-compatible storage) - *Note: R2 implementation is not fully tested*

**Cloudflare R2 Setup:** For detailed instructions on configuring Cloudflare R2 storage, see [CLOUDFLARE_R2_SETUP.md](./CLOUDFLARE_R2_SETUP.md).

#### File Types

Files are automatically categorized:
- **Image**: jpg, png, gif, webp, etc.
- **Document**: pdf, doc, docx, txt, etc.
- **Video**: mp4, avi, mov, etc.
- **Audio**: mp3, wav, ogg, etc.
- **Other**: Unknown file types

#### Endpoints

**Upload File (requires FileUpload.Write permission):**

**Synchronous Upload (default):**
```http
POST https://localhost:7XXX/api/files/upload
Authorization: Bearer <your-access-token>
Content-Type: multipart/form-data

file: <file>
storageType: FileSystem (optional, default: FileSystem)
description: "Optional file description" (optional)
tags: "tag1,tag2" (optional)
useBackgroundQueue: false (optional, default: false)
```

**Response (HTTP 201 Created):**
```json
{
  "id": "guid",
  "fileName": "example.pdf",
  "contentType": "application/pdf",
  "fileSize": 1024000,
  "extension": ".pdf",
  "fileType": "Document",
  "storageType": "FileSystem",
  "storagePath": "/uploads/guid/example.pdf",
  "uploadedBy": "user-guid",
  "description": "Optional file description",
  "tags": "tag1,tag2",
  "hash": "md5hash",
  "creationTime": "2024-01-01T12:00:00Z"
}
```

**Asynchronous Upload (background processing):**
```http
POST https://localhost:7XXX/api/files/upload
Authorization: Bearer <your-access-token>
Content-Type: multipart/form-data

file: <file>
storageType: FileSystem (optional)
description: "Optional file description" (optional)
tags: "tag1,tag2" (optional)
useBackgroundQueue: true
```

**Response (HTTP 202 Accepted):**
```json
{
  "fileId": "guid",
  "fileName": "example.pdf",
  "message": "File upload has been queued and will be processed in the background. Please check the file status later."
}
```

**Note**: When `useBackgroundQueue=true`, the endpoint returns immediately with a simple queued response. The file is processed asynchronously in the background. Use the `fileId` to check the upload status via the GET endpoint.

**Get All Files (requires FileUpload.Read permission):**
```http
GET https://localhost:7XXX/api/files
Authorization: Bearer <your-access-token>
```

**Get File Metadata by ID (requires FileUpload.Read permission):**
```http
GET https://localhost:7XXX/api/files/{fileId}
Authorization: Bearer <your-access-token>
```

**Download File (requires FileUpload.Read permission):**
```http
GET https://localhost:7XXX/api/files/{fileId}/download
Authorization: Bearer <your-access-token>
```

**Delete File (requires FileUpload.Delete permission):**
```http
DELETE https://localhost:7XXX/api/files/{fileId}
Authorization: Bearer <your-access-token>
```

#### Example: Uploading a File with cURL

**Synchronous Upload:**
```bash
curl -X POST "https://localhost:7XXX/api/files/upload" \
  -H "Authorization: Bearer <your-access-token>" \
  -F "file=@/path/to/file.pdf" \
  -F "storageType=FileSystem" \
  -F "description=Important document" \
  -F "tags=document,important"
```

**Asynchronous Upload (Background Processing):**
```bash
curl -X POST "https://localhost:7XXX/api/files/upload" \
  -H "Authorization: Bearer <your-access-token>" \
  -F "file=@/path/to/file.pdf" \
  -F "storageType=FileSystem" \
  -F "description=Important document" \
  -F "tags=document,important" \
  -F "useBackgroundQueue=true"
```

#### Example: Uploading a File with JavaScript (Fetch API)

**Synchronous Upload:**
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('storageType', 'FileSystem');
formData.append('description', 'My uploaded file');
formData.append('tags', 'tag1,tag2');

const response = await fetch('https://localhost:7XXX/api/files/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`
  },
  body: formData
});

const result = await response.json();
console.log('File uploaded:', result);
```

**Asynchronous Upload (Background Processing):**
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('storageType', 'FileSystem');
formData.append('description', 'My uploaded file');
formData.append('tags', 'tag1,tag2');
formData.append('useBackgroundQueue', 'true'); // Enable background processing

const response = await fetch('https://localhost:7XXX/api/files/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`
  },
  body: formData
});

if (response.status === 202) {
  const queued = await response.json();
  console.log('File queued:', queued);
  console.log('File ID:', queued.fileId);
  console.log('Message:', queued.message);
  // File is being processed in background - check status later
} else {
  const result = await response.json();
  console.log('File uploaded:', result);
}
```

#### Permissions

The FileUpload feature uses the following permissions (automatically assigned to admin role):
- `FileUpload.Read`: Required to view file metadata and download files
- `FileUpload.Write`: Required to upload files
- `FileUpload.Delete`: Required to delete files

These permissions are automatically created and assigned to the admin role when the application starts (see [Database Seeding](#database-seeding) section).

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
           
           /// <summary>
           /// Gets all product permissions
           /// </summary>
           public static string[] GetAll()
           {
               return [Read, Write, Delete];
           }
       }
   }
   ```

2. **Add permission definition** in `PermissionNames.GetAllDefinitions()`:
   ```csharp
   public static PermissionDefinition[] GetAllDefinitions()
   {
       return
       [
           // ... other permissions ...
           
           // Product permissions
           new PermissionDefinition(Product.Read, "Read products", "Product", "Read"),
           new PermissionDefinition(Product.Write, "Create or update products", "Product", "Write"),
           new PermissionDefinition(Product.Delete, "Delete products", "Product", "Delete")
       ];
   }
   ```

3. **Use in endpoint** with the `RequirePermission` extension method:
   ```csharp
   using AspireApp.ApiService.Domain.Permissions;
   using AspireApp.ApiService.Presentation.Extensions;
   
   group.MapPost("/", CreateProduct)
       .RequirePermission(PermissionNames.Product.Write);
   ```

**Important**: When adding new permissions, make sure to:
- Add them to the appropriate class in `PermissionNames`
- Include them in `GetAllDefinitions()` method
- The admin role will automatically receive these permissions on the next application startup

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

- ✅ **Modular Monolith Architecture** - Self-contained modules with clear boundaries
- ✅ **Domain-Driven Design** - Rich domain models, domain services, and domain events
- ✅ **Clean Architecture** - Clear separation of concerns across layers
- ✅ **JWT Authentication** - Secure token-based authentication with refresh tokens
- ✅ **User Registration** - Public registration endpoint with automatic role assignment
- ✅ **Refresh Token Mechanism** - Seamless token renewal without re-authentication with token rotation and reuse detection
- ✅ **RBAC Authorization** - Role and permission-based access control with fluent extension methods
- ✅ **Dual Permission System** - Both role-based and direct user permission assignment
- ✅ **Comprehensive User Management** - Full CRUD operations, password management, activation control, role/permission assignment
- ✅ **Notification System** - Complete notification module with Firebase Cloud Messaging support and Firebase Authentication integration (reference pattern for other modules)
- ✅ **File Upload System** - Multi-storage file upload with support for FileSystem, Database, and R2 storage types with background processing
- ✅ **Background Task Queue** - Structured, scalable background task processing with graceful shutdown support
- ✅ **Activity Logging System** - Comprehensive activity tracking with automatic entity change tracking
- ✅ **Domain Events** - DDD-compliant domain events with automatic dispatching
- ✅ **Structured Logging** - Serilog integration with console, file, and JSON output
- ✅ **Notification Localization** - Automatic initialization of notification localization resources on startup
- ✅ **Root Path Redirect** - Automatic redirect from root path to Scalar UI for better developer experience
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
- **Firebase Admin SDK** - Firebase Cloud Messaging and Authentication integration

## 📝 Notes

- **Architecture Pattern**: The project follows **Modular Monolith** architecture with **Domain-Driven Design** principles
- **Module Reference**: The **Notification module** (`AspireApp.ApiService.Notifications`) serves as the reference pattern for creating new modules
- The project uses **separate .csproj files** for each layer to enforce compile-time dependency rules
- Old folder structure in `AspireApp.ApiService/` is excluded from compilation but kept for reference
- All layers follow the same namespace convention: `AspireApp.ApiService.{Layer}.*` or `AspireApp.ApiService.{Module}.{Layer}.*` for modular projects
- Database seeding runs automatically on application startup
- **Access tokens** expire after 60 minutes (configurable in `appsettings.json`)
- **Refresh tokens** expire after 7 days and are stored in the database
- Refresh tokens are automatically revoked when used to generate new tokens (token rotation)
- Token reuse detection: If a revoked refresh token is reused, all tokens for that user are revoked (security best practice)
- Expired refresh tokens can be cleaned up using `RefreshTokenRepository.CleanupExpiredTokensAsync()`
- **Permission system**: Supports both role-based permissions (inherited through roles) and direct user permissions (assigned directly to users)
- Direct user permissions take precedence over role-based permissions when checking access
- **Automatic permission management**: Permissions are automatically created, restored, and cleaned up based on `PermissionNames` definitions
- **Automatic admin permission assignment**: Admin role automatically receives all permissions defined in code - no manual assignment needed
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
- **File Upload**: Supports multiple storage types (FileSystem, Database, R2) with automatic file type detection, MD5 hash verification, metadata support (description, tags), and optional background processing
- **File Upload Permissions**: FileUpload permissions (Read, Write, Delete) are automatically created and assigned to admin role on startup
- **Background Task Queue**: Structured background task processing with graceful shutdown support - use `IBackgroundTaskQueue` instead of `Task.Run` for production-ready async operations
- **Domain-Driven Design**: File upload business logic encapsulated in `FileUploadManager` domain service following DDD principles
- **File Upload Helpers**: Domain helpers (`FileTypeHelper`, `FileValidationHelper`) provide reusable file validation and type detection logic
- **Notification Localization**: The `NotificationLocalizationInitializer` hosted service automatically loads and initializes notification localization resources from JSON files on application startup
- **Root Path Redirect**: The root path (`/`) automatically redirects to Scalar UI (`/scalar/v1`) for convenient API documentation access
- **Request Logging**: HTTP request logging middleware is disabled by default for performance reasons but can be re-enabled if needed (see `Program.cs` comments)
- **Cloudflare R2 Setup**: See [CLOUDFLARE_R2_SETUP.md](./CLOUDFLARE_R2_SETUP.md) for detailed R2 storage configuration instructions
- **Firebase Authentication**: Firebase Authentication service (`FirebaseAuthService`) provides programmatic user management for push notifications. The service automatically initializes on first use and handles Firebase user creation and UID retrieval
- **FCM Token Management**: Users can register FCM tokens for push notifications. The system automatically creates Firebase users when tokens are registered and checks for existing tokens via the `HasFCMToken` endpoint

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
- **Firebase Configuration**: Store Firebase service account credentials securely (use Azure Key Vault, User Secrets, or environment variables in production). Never commit service account keys to version control
- **FCM Tokens**: FCM tokens are user-specific and should be managed securely. Tokens may change over time and should be re-registered periodically

## 📚 Additional Resources

### Project Documentation
- **Cloudflare R2 Setup**: See [CLOUDFLARE_R2_SETUP.md](./CLOUDFLARE_R2_SETUP.md) for R2 storage configuration guide
- **Notification Module**: See `AspireApp.ApiService.Domain/Notifications/README.md` for notification module documentation

### External Resources
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Domain-Driven Design](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Modular Monolith Architecture](https://www.kamilgrzybek.com/blog/modular-monolith-primer)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

---

**Happy Coding! 🚀**

