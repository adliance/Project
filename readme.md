# Adliance Project: A .NET 6 template for new ASP.NET Core projects

This is a set of projects that provide a useful starting point for new projects that already provides a lot of useful functionality out-of-the-box.

# Techstack

- Basics
    - NET 6 / ASP.NET Core 6 for server-side logic and REST API
    - Entity Framework Core 6 for database access
    - Blazor for Frontend
- More
    - Hangfire for background jobs
    - NodaTime
    - Humanizer

# Code style considerations

- Nullable is enabled in every project, and compiler warnings are treated as errors. This forces every developer to
  fully embrace nullability checks.
- We use file-scoped namespaces, as they make the C# files more readable.

# Server

## Dependency Tree:

- `Shared` > `Server.Core` > `Server.Infrastructure` > `Server.Web`
- Make sure to strictly adhere to this dependency tree (for example, NEVER reference `Infrastructure` in `Core`).
- Please note that "Core" contains business logic and domain models, but NOT any information about infrastructure
  (database). This allows for testing of business logic without the need for any complex mocking of databases.
- This architecture takes cues from
  the ["Clean Architecture" style](https://ardalis.com/clean-architecture-asp-net-core/) of doing things, but we made
  the deliberate decision to not include advanced patterns like specifications (we replace specifications by just
  exposting `IQueryable` in our repositories). This decision was made to keep the architecture simpler and more in line
  with standard Microsoft tutorials but still enforce a strict separation between business models and infrastructure,

## Database

- We are using Entity Framework Core Migrations to set up the entire database. This enables a nice "clone and run"
  workflow for new development environments, as the database will automatically be created in local SQL Server.
- Because the migrations are in a different project than the startup application, we need to define the startup project
  when scripting migrations,
  like `dotnet ef migrations --startup-project ./../Adliance.Project.Server.Web add <MIGRATION_NAME>`.
- We always use repositories instead of using `DbContext` directly. This is useful for improved testability, because
  then we don't need `DbContext` in our `Core` business logic library.
- We differentiate between `IReadonlyRepository` (just for reading data) and `IRepository` (for reading and writing
  data). This is useful because `IReadonlyRepository` enforces the use of `AsNoTracking` for improved performance.
- We use temporal tables to create an audit trail of changes of articles. Please note that this tracks each and every
  change to the article for auditing, it's not some form of versioning to group a set of changes into specific article
  versions.

## Authentication & Logging

- We support authentication by cookies (for GUI) and authentication by API key (for external API clients)
- We use AzureAD to authenticate users and use cookie authentication for each subsequent request
- We log the full request and response for each API call. Please note that this will also potentially log personal
  information and will result in a lot of data in the database, that's why we also delete the logs after a few days.
- We use the existing ASP.NET Core roles model (via claims based authentication) to support different user roles.
  This also hides if an actual user (cookie authentication) or an API key (HTTP header authentication) is requesting
  something, the handling of different roles will be the same.

## Miscellaneous

- We use the `IOptions` pattern.
- We use "Hangfire" for a fully integrated "cron" solution.
- One background job automatically removes API call logs after 7 days.
- We use the *Humanizer* library for handling stuff like relative times or singular/plurals
- We use the *NodaTime* library for better handling of timezones then the default .NET `DateTime`functionality. We
  use `DateTime` tho, and always store dates/times in UTC timezone - only for user display we convert to CET timezone.
- We include *Application Insights* for performance and error monitoring as well as logging.
- We support different cultures by using the default ASP.NET Core view localization and request localization.
- We provide the endpoint `/health` for automated health checks (including a check if database is available).
- We use *Swagger* for automated API documentation (Open API specification).
    - Remember to document each API endpoint with good comments, as these will be used in the automatic documentation of
      your API. This includes error codes.
- We use a pattern for API requests/responses that mimic the MVVM pattern - one specific model for each action. We do
  not share the same model between different actions. While this seems counter-intuitive and leads to more code, it also
  makes sure that we do not break the "contract" of one API call while changing another.
    - We also try to keep the controllers as small as possible (for easy readability, controllers already get quite long
      due to all the documentation) and use distinct factories that
      encapsulate the building of these responses. So controllers only contain the actions and their documentation, no
      actual logic.
- We use the new `System.Text.Json` JSON serialization (no more Newtonsoft). This makes JSON serialization much more
  strict, as Newtonsoft tries very hard to parse JSON to match the target type - `System.Text.Json` just throws
  exceptions.
  But this strictness has the advantage of a better performance and it makes it easier to spot errors.

## Testing

- We put unit tests on business logic in `Adliance.Project.Server.Core.Test` (as `Core` contains the business logic).
- Integration tests that require a database and/or webserver are in `Adliance.Project.Server.Web.Test`. These tests
  require a full database server to run.

# Gui

## Dependency Tree:

- `Shared` > `Gui`
- Please note that communication with the server is done via REST API. We use the same REST API that can also be used by
  external clients.

## GUI Authentication

- We do not use Blazor for the initial page (AzureAD login flow), but only load the Blazor app after the user has
  successfully authenticated. This results in a start page that loads very fast (as opposed to a start page that needs
  to load the entire Blazor app before the user can do anything at all).