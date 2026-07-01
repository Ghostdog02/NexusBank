The Four Layers
① Domain — the core, no dependencies at all
BankingApp.Domain/
  Entities/
    User.cs
    Account.cs
    Transaction.cs
  ValueObjects/
    Money.cs         ← wraps decimal, enforces no floating point
    Currency.cs
  Interfaces/
    IAccountRepository.cs   ← defined here, implemented in Infrastructure
    ILedgerService.cs
    IStripeService.cs
  Exceptions/
    InsufficientFundsException.cs
    AccountNotFoundException.cs
This layer has zero NuGet packages. Pure C# classes only. It defines what the app is about — accounts, money, transactions. It does not know PostgreSQL exists, does not know Stripe exists, does not know ASP.NET exists.

② Application — use cases, no framework code
BankingApp.Application/
  Commands/
    Transfer/
      TransferCommand.cs      ← input: amount, recipientId
      TransferHandler.cs      ← business logic lives here
      TransferValidator.cs    ← FluentValidation rules
    Deposit/
      DepositCommand.cs
      DepositHandler.cs
  Queries/
    GetBalance/
      GetBalanceQuery.cs
      GetBalanceHandler.cs
    GetTransactions/
      GetTransactionsQuery.cs
      GetTransactionsHandler.cs
This is where CQRS comes in. Every operation is either a Command (changes state) or a Query (reads state). MediatR routes them to the right handler.
A handler looks like this:
csharppublic class TransferHandler : IRequestHandler<TransferCommand, TransferResult>
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerService _ledger;

    public async Task<TransferResult> Handle(TransferCommand cmd, CancellationToken ct)
    {
        var sender = await _accounts.GetByIdAsync(cmd.SenderId);

        if (sender.Balance < cmd.Amount)
            throw new InsufficientFundsException();

        await _ledger.RecordTransferAsync(cmd.SenderId, cmd.RecipientId, cmd.Amount);

        return new TransferResult { Success = true };
    }
}
Notice — no mention of HTTP, no mention of PostgreSQL, no mention of Stripe. Pure business logic.

③ Infrastructure — implements domain interfaces
BankingApp.Infrastructure/
  Persistence/
    AppDbContext.cs
    Repositories/
      AccountRepository.cs    ← implements IAccountRepository
      TransactionRepository.cs
  Services/
    StripeService.cs          ← implements IStripeService
    LedgerService.cs          ← implements ILedgerService
    IdempotencyService.cs
  Logging/
    SerilogConfiguration.cs
All the messy external concerns live here. PostgreSQL, Stripe, Serilog — all infrastructure details. The domain and application layers never reference these directly.

④ API — HTTP concerns only, thin controllers
BankingApp.Api/
  Controllers/
    TransactionsController.cs
    AccountsController.cs
    AuthController.cs
  Middleware/
    ExceptionHandlingMiddleware.cs
    RequestLoggingMiddleware.cs
  Filters/
    AuditLogFilter.cs
    IdempotencyFilter.cs
  DTOs/
    Requests/  TransferRequest.cs
    Responses/ TransferResponse.cs
  Program.cs
Controllers are intentionally thin — they just receive the HTTP request, map it to a Command or Query, send it through MediatR, and return the result:
csharp[HttpPost("transfer")]
[Authorize(Policy = "KycVerified")]
public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
{
    var command = new TransferCommand
    {
        SenderId = User.FindFirst("sub")!.Value,
        RecipientId = request.RecipientId,
        Amount = request.Amount
    };

    var result = await _mediator.Send(command);
    return Ok(result);
}
That is the entire controller action. Business logic is in the handler, not here.

Project Structure in Solution
BankingApp.sln
  BankingApp.Domain          ← no NuGet packages
  BankingApp.Application     ← MediatR, FluentValidation
  BankingApp.Infrastructure  ← EF Core, Stripe.net, Serilog
  BankingApp.Api             ← ASP.NET Core, JWT, Swagger
  BankingApp.Tests           ← xUnit, Moq

Why This Combination Works for Your App
Clean Architecture gives you separation of concerns — your business logic is completely isolated and testable without spinning up a database or hitting Stripe.
CQRS gives you a natural separation between reads and writes — which maps perfectly to banking because a balance query and a money transfer are fundamentally different operations that should be handled differently.
Together in an interview you can say:

"I used Clean Architecture so my business logic has zero dependencies on external frameworks, and CQRS via MediatR to separate read and write operations. This means I can unit test every transfer scenario by mocking the repository — no database needed."


