# AspireApp.Payment

The **AspireApp.Payment** microservice is a robust and scalable payment processing system built using **Clean Architecture** principles. It provides a flexible framework for handling various payment methods, including external providers like Stripe and Tabby, as well as manual cash payments.

## 🚀 Key Features

- **Multi-Strategy Payment Processing**: Seamlessly switch between different payment providers using the Strategy Pattern.
- **Refund Management**: Support for partial and full refunds across different payment methods.
- **Transaction History**: Detailed tracking of all payment attempts and their outcomes.
- **Domain Events**: Integrated event system for notifications (e.g., successful/failed payments).
- **Validation**: Robust request validation using FluentValidation.
- **Mapping**: Efficient object-to-object mapping with AutoMapper.

## 🛠️ Technology Stack

- **Framework**: .NET 10.0
- **Database ORM**: Entity Framework Core
- **Payment Providers**: Stripe, Tabby
- **Tools**: AutoMapper, FluentValidation, Newtonsoft.Json

## 🏗️ Architecture

The project follows the **Clean Architecture** pattern, ensuring separation of concerns and testability:

### 1. Domain Layer
Contains the core business logic, entities, value objects, and domain events.
- **Entities**: `Payment`, `PaymentTransaction`
- **Value Objects**: `Money`, `Currency`
- **Events**: `PaymentSucceeded`, `PaymentFailed`, `PaymentRefunded`

### 2. Application Layer
Defines the technical use cases and coordinates the domain logic.
- **Use Cases**: `CreatePayment`, `ProcessPayment`, `RefundPayment`, `GetPaymentHistory`
- **DTOs**: Data transfer objects for API requests and responses.

### 3. Infrastructure Layer
Implements external concerns such as database persistence and third-party API integrations.
- **Strategies**: `StripePaymentStrategy`, `TabbyPaymentStrategy`, `CashPaymentStrategy`
- **Services**: `StripePaymentService`, `TabbyPaymentService`
- **Repositories**: EF Core-based repository implementations.

## 📂 Project Structure

```text
AspireApp.Payment/
├── Application/        # Use cases, DTOs, Mappings, Validators
├── Domain/             # Entities, Events, Interfaces, Models, Value Objects
└── Infrastructure/     # Configurations, Factories, Repositories, Strategies, Services
```

## ⚙️ Setup & Configuration

Configure your payment provider options in `appsettings.json`:

```json
{
  "StripeOptions": {
    "SecretKey": "your_stripe_secret_key",
    "PublishableKey": "your_stripe_publishable_key",
    "WebhookSecret": "your_stripe_webhook_secret"
  },
  "TabbyOptions": {
    "ApiKey": "your_tabby_api_key",
    "BaseUrl": "https://api.tabby.ai/"
  }
}
```

## 🛡️ Best Practices

- **Strategy Pattern**: Simplifies adding new payment providers.
- **Domain-Driven Design (DDD)**: Uses entities and value objects to encapsulate business rules.
- **Rich Domain Model**: Logic is placed within entities where appropriate (e.g., `Money` object operations).
