# Technical Proposal: Modern Accounting Framework Kernel

## Executive Summary

This proposal outlines the development of a **Modern Accounting Framework Kernel**—a domain-driven, event-sourced accounting system that serves as a reusable foundation for both personal finance applications and enterprise accounting systems. The kernel modernizes Paul Dustin Keefer's 1994 framework using contemporary software architecture principles, including Domain-Driven Design (DDD), CQRS, Event Sourcing, and modular extensibility.

### Key Objectives

1. **Build a reusable Accounting Kernel** that abstracts core accounting concepts (accounts, transactions, attributes)
2. **Support dual use cases**: Personal finance tools (Quicken-like) and business accounting systems
3. **Enable configuration over code**: Account types, transaction mappings, and attributes defined as metadata
4. **Provide extensibility**: Framework supports bounded contexts for domain-specific features
5. **Ensure scalability**: Event-driven architecture with materialized views for performance

### Scope

**In Scope (This Proposal)**:
- ✅ **Accounting Kernel**: Core domain model, aggregates, and infrastructure
- ✅ **Event Sourcing**: Event store, projections, and read models
- ✅ **Transaction Mapping Engine**: External-to-internal transaction conversion
- ✅ **Function-of-Time Attributes**: Time-dependent calculations with O(log n) performance
- ✅ **Account Type System**: Template-based account type definitions
- ✅ **API Layer**: RESTful APIs for kernel operations
- ✅ **Infrastructure**: Event store, read models, projection processors

**Out of Scope (Future Phases)**:
- ❌ **Personal Finance Bounded Context**: Consumer-facing features (categories, budgets, CSV import)
- ❌ **Business Accounting Bounded Context**: Enterprise features (AR/AP workflows, invoicing, compliance)
- ❌ **User Interfaces**: Web UI, mobile apps, desktop applications
- ❌ **Integration Features**: Bank API connectors, payment processor integrations
- ❌ **Reporting Engine**: Configurable reports, exports, dashboards
- ❌ **Workflow Engine**: Approval workflows, business process automation

**Note**: The kernel is designed to be extended by these out-of-scope components. The proposal includes integration points and examples showing how bounded contexts would consume the kernel.

---

## 1. Background and Context

### 1.1 Original Framework (1994)

Keefer's thesis introduced an object-oriented framework for accounting systems built in Smalltalk-80. Key innovations:

- **Three account types**: Simple (stores transactions), Composite (groups accounts), Aggregate (routes transactions)
- **Transaction mapping**: Visual language for converting external transactions into internal journal entries
- **Function-of-Time attributes**: Time-dependent calculations (total, MTD, YTD) with efficient O(log n) queries
- **Template-based type system**: Account types defined as template accounts, avoiding code generation

### 1.2 Modern Interpretation

The DDD paper reinterprets these concepts for modern architectures:

- **Accounts as Aggregates**: Account becomes an Aggregate Root in DDD terms
- **Transactions as Events**: External transactions map to commands; internal transactions become domain events
- **Attributes as Projections**: Function-of-Time attributes map to CQRS projections and value objects
- **Bounded Contexts**: Kernel remains business-agnostic; domain-specific features live in separate contexts

---

## 2. Architecture Overview

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│              .NET Aspire AppHost (Orchestration)            │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                    Presentation Layer                       │
│  (ASP.NET Core Controllers / APIs)                           │
│  - JWT Auth with Google (OIDC/JWT bearer)                    │
│  - Result → HTTP mapping                                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                  Application Layer                           │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Teqniqly.Arbiter (Command/Query Dispatch)         │    │
│  │  - Command Handlers (EF Core + Teqniqly.Results)   │    │
│  │  - Query Handlers (Dapper + Teqniqly.Results)       │    │
│  └────────────────────────────────────────────────────┘    │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│        Domain Layer (Kernel) - Standalone Assembly          │
│        (No dependencies on infrastructure/hosting)           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Account    │  │ Transaction  │  │  Attribute   │      │
│  │  Aggregate   │  │   Entity     │  │ Value Object │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ AccountType  │  │ Transaction  │  │  Posting     │      │
│  │   Metadata   │  │   Mapping    │  │  Service     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│                                                              │
│  Referenced by: Application, Infrastructure, Api            │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                Infrastructure Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │ Event Store  │  │   EF Core   │  │  Projections   │    │
│  │(SQL Server)  │  │  (Writes)   │  │   (Materialized│    │
│  │              │  │             │  │    Views)      │    │
│  └──────────────┘  └──────────────┘  └──────────────┘    │
│  ┌──────────────┐                                        │
│  │    Dapper    │                                        │
│  │   (Reads)    │                                        │
│  └──────────────┘                                        │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Bounded Contexts

#### Core Kernel Context
- **Responsibility**: Core accounting abstractions
- **Entities**: Account, Transaction, AccountType, TransactionMapping
- **Value Objects**: AttributeDefinition, TransactionEntry
- **Domain Services**: PostingService, AttributeCalculator

#### Personal Finance Context (Optional)
- **Responsibility**: Consumer-friendly features
- **Features**: Categories, budgets, spending analytics, CSV import
- **Integration**: Uses kernel via application layer

#### Business Accounting Context (Optional)
- **Responsibility**: Enterprise features
- **Features**: AR/AP, invoicing, inventory, compliance
- **Integration**: Uses kernel via application layer

---

## 3. Technology Stack

### 3.1 Core Platform
- **Language**: C# (.NET 10)
- **Framework**: ASP.NET Core Web API with Controllers
- **Orchestration**: .NET Aspire (AppHost)
- **Domain Modeling**: Pure C# classes in standalone assembly (no framework dependencies, host/infrastructure agnostic)

### 3.2 Persistence
- **Event Store**: SQL Server 2025 with custom event table
- **Write Models**: EF Core (DbContext + transactions for commands)
- **Read Models**: Dapper (query-only models, no EF tracking)
- **Projections**: Materialized views + background projection processors

### 3.3 Architecture Patterns
- **CQS / CQRS-light**: Separate command and query models
  - **Writes**: EF Core with DbContext and transactions (for commands)
  - **Reads**: Dapper with parameterized SQL queries (query-only models, no EF tracking)
- **Event Sourcing**: All state changes as domain events (stored in event table)
- **DDD**: Aggregates, value objects, domain services
- **Messaging**: Teqniqly.Arbiter (mediator pattern for command/query dispatch)
- **Results**: Teqniqly.Results (Results pattern for error handling)
  - Commands/queries return `Result<T>` or `Result`
  - No exceptions for expected outcomes (validation, not found, conflict)
  - Map results to HTTP responses in one place (API result mapper/filter)

### 3.4 Authentication & Authorization
- **AuthN**: JWT authentication with Google (OIDC/JWT bearer)
- **AuthZ**: Authorization policies enforced centrally

### 3.5 Additional Tools
- **Validation**: FluentValidation
- **Mapping**: Manual mapping (no AutoMapper - use explicit mapping methods/constructors)
- **Testing**: xUnit with built-in assertions (Assert.Equal, Assert.True, etc.), NSubstitute (not Moq, not FluentAssertions)
- **Documentation**: Swagger/OpenAPI

### 3.6 Solution Layout

Following Teqniqly stack conventions, with the kernel domain in its own assembly for host and infrastructure agnosticism:

```
src/
  # Kernel Domain (Pure, no dependencies on infrastructure/hosting)
  AccountingKernel.Domain/            # Entities, value objects, domain rules
                                     # - No EF Core, Dapper, or framework dependencies
                                     # - Can be referenced by any host/infrastructure
  
  # Application Hosting & Infrastructure
  AccountingKernel.AppHost/          # .NET Aspire AppHost
  AccountingKernel.ServiceDefaults/  # Aspire defaults
  AccountingKernel.Api/               # Controllers, composition root
                                     # - References: Domain, Application, Infrastructure
  AccountingKernel.Application/       # CQS handlers, Arbiter handlers, DTOs
                                     # - References: Domain
  AccountingKernel.Infrastructure/    # EF Core, Dapper, repositories, adapters
                                     # - References: Domain, Application
tests/
  AccountingKernel.Domain.Tests/      # Unit tests for domain (pure domain logic)
  AccountingKernel.Tests/             # Unit tests for application/infrastructure
  AccountingKernel.IntegrationTests/  # Integration tests (Testcontainers)
docs/                                 # Exploration, proposals, stories
```

**Key Principles**:
- **AccountingKernel.Domain** is a standalone assembly with **zero dependencies** on:
  - Infrastructure (EF Core, Dapper, SQL Server)
  - Hosting (.NET Aspire, ASP.NET Core)
  - External frameworks (except .NET Standard/BCL)
- Other assemblies reference the Domain, not vice versa
- Domain contains pure business logic: aggregates, value objects, domain events, domain services (interfaces only)

**Organization**: Vertical slice organization for features (commands/queries grouped by feature).

### 3.7 CQS Conventions

**Commands (Writes)**:
- Handler uses EF Core DbContext
- Return `Result<T>` or `Result` from Teqniqly.Results
- Raise domain events only if the design calls for it
- Use transactions as needed for consistency

**Queries (Reads)**:
- Use Dapper with parameterized SQL
- Return DTOs/read models (no EF entities)
- Keep SQL in dedicated folder: `Infrastructure/Queries/Sql/` (or embedded resources—choose one and be consistent)
- No EF tracking for reads

**Arbiter Conventions**:
- All handlers registered via DI in API composition root
- Handlers are small, focused, and testable (dependencies injected)
- Prefer request/response messages that are immutable records

---

## 4. Domain Model Design

**Note**: The domain model resides in the `AccountingKernel.Domain` assembly, which has **zero dependencies** on infrastructure, hosting, or external frameworks. This ensures the kernel domain can be used with any host (ASP.NET Core, console apps, Azure Functions, etc.) and any infrastructure (SQL Server, PostgreSQL, event stores, etc.).

**ID Generation**: All entity IDs use `Guid` directly, generated via `Guid.CreateVersion7()`. UUID version 7 provides time-ordered GUIDs that improve database indexing performance and are naturally sortable by creation time. ID generation is handled automatically by base class constructors (`AggregateRoot` and `Entity`), ensuring consistent ID generation across all aggregates and entities. Factory methods (e.g., `Account.Create()`) enforce business invariants and validation.

### 4.1 Core Aggregates

#### Base Classes

```csharp
public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    
    // Constructor for creating new aggregates (generates ID)
    protected AggregateRoot()
    {
        Id = Guid.CreateVersion7();
    }
    
    // Constructor for loading existing aggregates from persistence
    protected AggregateRoot(Guid id)
    {
        Id = id;
    }
    
    // Domain events collection
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class Entity
{
    public Guid Id { get; protected set; }
    
    // Constructor for creating new entities (generates ID)
    protected Entity()
    {
        Id = Guid.CreateVersion7();
    }
    
    // Constructor for loading existing entities from persistence
    protected Entity(Guid id)
    {
        Id = id;
    }
}
```

#### Account (Aggregate Root)
```csharp
public class Account : AggregateRoot
{
    public Guid Type { get; private set; }  // AccountTypeId
    public AccountName Name { get; private set; }
    public Guid? ParentId { get; private set; }
    
    private List<TransactionEntry> _entries = new();
    private Dictionary<AttributeName, AttributeDefinition> _attributes = new();
    
    // Private constructor - enforce factory method usage
    private Account() { }
    
    // Constructor for loading from persistence
    private Account(Guid id) : base(id) { }
    
    // Factory method for creating new accounts
    public static Account Create(
        Guid accountTypeId,
        AccountName name,
        Guid? parentId = null)
    {
        return new Account
        {
            Type = accountTypeId,
            Name = name,
            ParentId = parentId
            // Id is automatically generated by base class constructor
        };
    }
    
    public void RecordTransaction(TransactionEntry entry)
    {
        // Invariant checks
        _entries.Add(entry);
        AddDomainEvent(new TransactionPosted(Id, entry));
    }
    
    public void AddAttribute(AttributeName name, AttributeDefinition definition)
    {
        _attributes[name] = definition;
        AddDomainEvent(new AttributeAdded(Id, name, definition));
    }
    
    public decimal? GetAttributeValue(AttributeName name, DateTime asOfDate)
    {
        // Calculate using Function-of-Time logic
    }
}
```

#### Transaction (Entity within Account aggregate)
```csharp
public class TransactionEntry : Entity
{
    public Guid AccountId { get; private set; }
    public TransactionDate Date { get; private set; }
    public Dictionary<FieldName, FieldValue> Fields { get; private set; }
    public Guid Type { get; private set; }  // TransactionTypeId
    
    // Private constructor - enforce factory method usage
    private TransactionEntry() { }
    
    // Constructor for loading from persistence
    private TransactionEntry(Guid id) : base(id) { }
    
    // Factory method for creating new transaction entries
    public static TransactionEntry Create(
        Guid accountId,
        Guid transactionTypeId,
        TransactionDate date,
        Dictionary<FieldName, FieldValue> fields)
    {
        return new TransactionEntry
        {
            AccountId = accountId,
            Type = transactionTypeId,
            Date = date,
            Fields = fields
            // Id is automatically generated by base class constructor
        };
    }
}
```

#### AccountType (Aggregate Root)
```csharp
public class AccountType : AggregateRoot
{
    public AccountTypeName Name { get; private set; }
    public AccountKind Kind { get; private set; } // Simple, Composite, Aggregate
    public Guid? ParentTypeId { get; private set; }
    public Dictionary<AttributeName, AttributeDefinition> DefaultAttributes { get; private set; }
    
    // Private constructor - enforce factory method usage
    private AccountType() { }
    
    // Constructor for loading from persistence
    private AccountType(Guid id) : base(id) { }
    
    // Factory method for creating new account types
    public static AccountType Create(
        AccountTypeName name,
        AccountKind kind,
        Guid? parentTypeId = null)
    {
        return new AccountType
        {
            Name = name,
            Kind = kind,
            ParentTypeId = parentTypeId,
            DefaultAttributes = new Dictionary<AttributeName, AttributeDefinition>()
            // Id is automatically generated by base class constructor
        };
    }
}
```

#### TransactionMapping (Aggregate Root)
```csharp
public class TransactionMapping : AggregateRoot
{
    public Guid AggregateAccountId { get; private set; }
    public Guid ExternalTransactionTypeId { get; private set; }
    public Dictionary<Guid, FieldMapping> Mappings { get; private set; }  // Key: InternalTransactionTypeId
    
    // Private constructor - enforce factory method usage
    private TransactionMapping() { }
    
    // Constructor for loading from persistence
    private TransactionMapping(Guid id) : base(id) { }
    
    // Factory method for creating new transaction mappings
    public static TransactionMapping Create(
        Guid aggregateAccountId,
        Guid externalTransactionTypeId)
    {
        return new TransactionMapping
        {
            AggregateAccountId = aggregateAccountId,
            ExternalTransactionTypeId = externalTransactionTypeId,
            Mappings = new Dictionary<Guid, FieldMapping>()
            // Id is automatically generated by base class constructor
        };
    }
    
    public List<InternalTransaction> MapToInternalTransactions(ExternalTransaction externalTx)
    {
        // Convert external transaction to one or more internal transactions
    }
}
```

### 4.2 Value Objects

```csharp
public record AttributeDefinition(string Expression);
public record TransactionDate(DateTime Value);
public record FieldName(string Value);
public record AttributeName(string Value);
```

**Note on IDs**: All entity IDs use `Guid` directly, generated using `Guid.CreateVersion7()`. UUID version 7 provides time-ordered GUIDs that are:
- Naturally sortable by creation time
- Better for database indexing performance
- Built into .NET 8+ (we're using .NET 10)
- Generated automatically by base class constructors (`AggregateRoot` and `Entity`)
- Factory methods (e.g., `Account.Create()`) enforce business invariants and validation

### 4.3 Domain Services

```csharp
public interface IPostingService
{
    Task<PostResult> PostAsync(ExternalTransaction transaction, Guid targetAccountId);
}

public interface IAttributeCalculator
{
    Task<decimal?> CalculateAsync(
        Guid accountId, 
        AttributeName attributeName, 
        DateTime asOfDate);
}
```

---

## 5. Implementation Phases

### Phase 1: Core Domain Model (Weeks 1-4)
**Goal**: Implement pure domain model in standalone assembly with no infrastructure dependencies

**Deliverables**:
- `AccountingKernel.Domain` assembly (standalone project)
- Account, Transaction, AccountType aggregates
- Value objects and domain events
- Domain service interfaces (interfaces only, implementations in Infrastructure)
- Unit tests for domain logic (`AccountingKernel.Domain.Tests`)

**Acceptance Criteria**:
- All aggregates enforce invariants
- Domain events capture all state changes
- **Zero dependencies** on infrastructure, hosting, or external frameworks in Domain assembly
- Domain assembly can be referenced by any host/infrastructure project
- Domain tests run without any infrastructure setup

### Phase 2: Event Sourcing Infrastructure (Weeks 5-8)
**Goal**: Implement event store and basic projection system

**Deliverables**:
- Event store implementation (SQL Server 2025-based)
- Event serialization/deserialization
- Aggregate repository using event sourcing
- Basic projection processor
- Integration tests

**Acceptance Criteria**:
- Can persist and retrieve aggregates via events
- Projections update correctly from events
- Handles concurrent writes correctly

### Phase 3: CQRS and Read Models (Weeks 9-12)
**Goal**: Implement command/query separation with optimized read models

**Deliverables**:
- Command handlers (Teqniqly.Arbiter) using EF Core
- Query handlers (Teqniqly.Arbiter) using Dapper
- Read model projections (Dapper queries in `Infrastructure/Queries/Sql/`)
- Materialized views for common queries
- API endpoints (Controllers with Result mapping)
- Teqniqly.Results integration
- Result-to-HTTP mapping (centralized filter/mapper)

**Acceptance Criteria**:
- Commands write to event store
- Queries read from optimized read models
- Projections maintain consistency

### Phase 4: Transaction Mapping Engine (Weeks 13-16)
**Goal**: Implement visual transaction mapping system

**Deliverables**:
- TransactionMapping aggregate implementation
- Mapping engine that converts external → internal transactions
- Support for multi-value fields (lists)
- Validation and error handling
- UI for mapping configuration (optional)

**Acceptance Criteria**:
- Can map external transactions to multiple internal transactions
- Handles field transformations correctly
- Supports aggregate account routing

### Phase 5: Function-of-Time Attributes (Weeks 17-20)
**Goal**: Implement efficient time-dependent attribute calculations

**Deliverables**:
- AttributeDefinition parser/evaluator
- Function-of-Time implementations (Total, MTD, YTD)
- Efficient calculation using partial results (O(log n))
- Support for binary operations (+, -, *, /)
- Component attribute references

**Acceptance Criteria**:
- Attributes calculate correctly for any date
- Performance is O(log n) for supported time ranges
- Supports inheritance from account type hierarchy

### Phase 6: Account Type System (Weeks 21-24)
**Goal**: Implement template-based account type system

**Deliverables**:
- AccountType aggregate with template support
- Account cloning from templates
- Type hierarchy management
- Attribute inheritance
- API for account type management

**Acceptance Criteria**:
- Can create account types as templates
- Accounts inherit attributes from types
- Type changes propagate correctly

### Phase 7: Integration and Testing (Weeks 25-28)
**Goal**: End-to-end integration and comprehensive testing

**Deliverables**:
- Integration test suite
- Performance testing
- Load testing
- Documentation
- Example implementations (personal finance, business accounting)

**Acceptance Criteria**:
- All features work end-to-end
- Performance meets requirements
- Documentation is complete

---

## 6. Key Design Decisions

### 6.1 Event Sourcing
**Decision**: Use event sourcing for all aggregates

**Rationale**:
- Provides audit trail automatically
- Enables time-travel queries (attribute values as of any date)
- Supports future features (budgeting, reconciliation)
- Aligns with DDD event-driven architecture

**Trade-offs**:
- Increased complexity in read model projections
- Requires event store infrastructure
- Learning curve for team

### 6.2 CQRS
**Decision**: Separate command and query models

**Rationale**:
- Optimize reads independently (materialized views)
- Scale reads and writes independently
- Clear separation of concerns

**Trade-offs**:
- Eventual consistency in read models
- More infrastructure to maintain

### 6.3 Metadata-Driven Configuration
**Decision**: Account types and mappings stored as data, not code

**Rationale**:
- Enables non-programmers to configure system
- Supports runtime changes without deployment
- Aligns with original framework vision

**Trade-offs**:
- More complex validation logic
- Requires robust metadata management

### 6.4 SQL Server 2025 for Event Store
**Decision**: Use SQL Server 2025 instead of specialized event store (e.g., EventStore DB)

**Rationale**:
- Aligns with Teqniqly stack standards (SQL Server 2025)
- Single database technology for consistency
- Good enough performance for most use cases
- Easier operations and deployment
- Can leverage SQL Server features (JSON, materialized views, temporal tables)

**Trade-offs**:
- May need optimization for very high event volumes
- Less specialized tooling than EventStore DB

---

## 7. Performance Considerations

### 7.1 Attribute Calculation Optimization

The original framework used red-black trees with partial results to achieve O(log n) attribute calculations. We'll implement similar optimization:

- **Transaction Index**: B-tree index on (AccountId, Date) for efficient range queries
  - Using `Guid.CreateVersion7()` provides time-ordered IDs that improve index performance
- **Partial Results**: Materialized aggregates at account level, updated incrementally
- **Caching**: Cache attribute values for common queries (as-of-date, account)

### 7.2 Read Model Projections

- **Materialized Views**: Pre-compute common queries (account balances, attribute values)
- **Incremental Updates**: Update projections as events arrive
- **Snapshot Strategy**: Periodic snapshots for very large aggregates

### 7.3 Scalability

- **Horizontal Scaling**: Read models can be replicated
- **Partitioning**: Partition events by account or date range
- **Caching**: Redis for frequently accessed attribute values

---

## 8. Testing Strategy

### 8.1 Unit Tests
- Domain logic (aggregates, value objects)
- Domain services
- Attribute calculation logic
- **Framework**: xUnit + NSubstitute (not Moq)

### 8.2 Integration Tests
- Event store persistence
- Projection updates
- End-to-end transaction posting
- Attribute calculation with real data

### 8.3 Performance Tests
- Attribute calculation performance (verify O(log n))
- Event store write/read performance
- Concurrent transaction posting

### 8.4 Acceptance Tests
- User scenarios (personal finance, business accounting)
- Configuration workflows (account types, mappings)

---

## 9. Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Event store performance | High | Start with SQL Server 2025, migrate to EventStore DB if needed |
| Attribute calculation complexity | Medium | Implement incrementally, extensive testing |
| Metadata validation | Medium | Strong validation layer, comprehensive error messages |
| Learning curve (event sourcing) | Medium | Training, documentation, pair programming |
| Scope creep | High | Strict phase gates, MVP first |

---

## 10. Success Metrics

### 10.1 Functional Metrics
- ✅ Can configure account types without code changes
- ✅ Can post transactions and calculate attributes correctly
- ✅ Supports both personal finance and business accounting use cases
- ✅ Attribute calculations are O(log n) for supported time ranges

### 10.2 Performance Metrics
- Transaction posting: < 100ms (p95)
- Attribute calculation: < 50ms (p95) for accounts with 10K transactions
- Event store write: < 10ms (p95)
- Read model query: < 20ms (p95)

### 10.3 Quality Metrics
- Test coverage: > 80%
- Zero data loss (event sourcing guarantees)
- Full audit trail for all changes

---

## 11. Future Enhancements

### 11.1 Phase 8+: Advanced Features
- **Budgeting and Reconciliation**: Shadow books, budget vs. actual
- **Multi-currency Support**: Currency conversion, exchange rates
- **Reporting Engine**: Configurable reports, exports
- **Workflow Engine**: Approval workflows for transactions
- **API Extensions**: Additional REST endpoints (GraphQL/gRPC would require explicit approval per Teqniqly stack rules)

### 11.2 Integration Opportunities
- **Bank APIs**: Direct transaction import
- **Payment Processors**: Stripe, PayPal integration
- **Tax Software**: Export for tax preparation
- **Business Intelligence**: BI tool connectors

---

## 12. Conclusion

This technical proposal outlines a modern, scalable architecture for an accounting framework kernel that honors the original vision while leveraging contemporary software engineering practices. The phased approach allows for iterative development, risk mitigation, and early validation of core concepts.

The resulting system will provide:
- **Reusability**: Single kernel for multiple accounting domains
- **Extensibility**: Easy addition of new features via bounded contexts
- **Performance**: Optimized for real-world transaction volumes
- **Maintainability**: Clean architecture with clear separation of concerns

---

## Appendix A: Example Use Cases

### Personal Finance
- Track checking/savings accounts
- Categorize expenses
- Calculate spending by category (MTD, YTD)
- Budget tracking
- Net worth calculation

### Business Accounting
- General ledger
- Accounts payable/receivable
- Inventory tracking
- Financial reporting
- Multi-entity support

---

## Appendix B: Technology Alternatives Considered

| Component | Chosen | Alternatives Considered |
|-----------|--------|------------------------|
| Event Store | SQL Server 2025 | EventStore DB, PostgreSQL |
| Write ORM | EF Core | NHibernate |
| Read ORM | Dapper | EF Core (rejected for reads per Teqniqly stack) |
| CQRS Framework | Teqniqly.Arbiter | MediatR, Brighter |
| Results Pattern | Teqniqly.Results | Custom, FluentResults |
| Validation | FluentValidation | Data Annotations, custom |
| Testing Mock | NSubstitute | Moq (rejected per Teqniqly stack) |

---

*Document Version: 1.0*  
*Last Updated: 2024*

