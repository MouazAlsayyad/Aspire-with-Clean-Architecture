# Notification Module - DDD Architecture

This module implements a comprehensive Firebase Cloud Messaging notification system following Domain-Driven Design (DDD) principles.

## Module Structure

The Notification feature is organized as a self-contained module across all layers:

```
Notifications/
├── Domain Layer (AspireApp.ApiService.Domain/Notifications/)
│   ├── Entities/          - Notification aggregate root
│   ├── Enums/            - NotificationType, Priority, Status, TimeFilter
│   ├── Events/           - NotificationCreatedEvent domain event
│   ├── Interfaces/       - Repository and service contracts
│   ├── Services/         - Domain services (NotificationManager, Localization)
│   └── Resources/        - Localization resource files (JSON)
│
├── Application Layer (AspireApp.ApiService.Application/Notifications/)
│   ├── DTOs/             - Data Transfer Objects
│   ├── UseCases/         - Application use cases
│   ├── Validators/       - FluentValidation validators
│   └── Mappings/         - AutoMapper profiles
│
├── Infrastructure Layer (AspireApp.ApiService.Infrastructure/Notifications/)
│   ├── Repositories/     - EF Core repository implementation
│   ├── Services/         - Firebase services, localization initializer
│   ├── Handlers/         - Domain event handlers
│   └── Configurations/   - EF Core entity configurations
│
└── Presentation Layer (AspireApp.ApiService.Presentation/Notifications/)
    └── NotificationEndpoints.cs - REST API endpoints
```

## DDD Boundaries

### Domain Layer (`Domain/Notifications/`)
- **Pure domain logic** - No dependencies on other layers
- **Entities**: `Notification` aggregate root
- **Value Objects**: Enums for type, priority, status
- **Domain Events**: `NotificationCreatedEvent`
- **Domain Services**: `NotificationManager`, `NotificationLocalizationService`
- **Interfaces**: Contracts for repositories and services

### Application Layer (`Application/Notifications/`)
- **Use Cases**: Application workflows
- **DTOs**: Data transfer objects for API
- **Validators**: Input validation rules
- **Mappings**: Entity to DTO mappings

### Infrastructure Layer (`Infrastructure/Notifications/`)
- **Repositories**: Data access implementations
- **Services**: External service integrations (Firebase)
- **Handlers**: Domain event handlers
- **Configurations**: EF Core entity configurations

### Presentation Layer (`Presentation/Notifications/`)
- **Endpoints**: REST API endpoints

## Key Features

- ✅ Bilingual support (English/Arabic)
- ✅ Firebase Cloud Messaging integration
- ✅ Cursor-based pagination
- ✅ Domain event-driven architecture
- ✅ Localization system with resource files
- ✅ User language preference support
- ✅ FCM token management

## Namespace Convention

All notification-related code uses the `Notifications` namespace:
- Domain: `AspireApp.ApiService.Domain.Notifications.*`
- Application: `AspireApp.ApiService.Application.Notifications.*`
- Infrastructure: `AspireApp.ApiService.Infrastructure.Notifications.*`
- Presentation: `AspireApp.ApiService.Presentation.Notifications.*`

This ensures clear separation and follows DDD bounded context principles.

