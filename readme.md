# Adliance Project: A .NET 6 template for new ASP.NET Core & Blazor projects

This is a full solution (several interdependent projects) that provide a useful starting point for a new project with
ASP.NET Core 6 on the server-side,
SQL Server for data storage and optionally a Blazor Webassembly frontend. We already included some useful libraries and
patterns that are not included in
the default ASP.NET Core project templates but - in our experience - are useful in almost all projects.

Feel free to browse or fork the code. We're happy for any feedback or ways that we can make things even easier.

## Frameworks and Libraries

- Basics
    - **ASP.NET Core** for server-side logic and the REST API that drives the frontend. Currently this template does not
      contain and server-side views but can easily be extended the include MVC views or Razor Pages.
    - **Entity Framework Core 6** for database access. We usually use SQL Server (Azure SQL) for an RDBMS, but it should
      be trivial to replace it with NPGSQL (Postgres).
    - **Blazor Webassembly** for the SPA frontend.
    - **[DevExpress](https://www.devexpress.com/)** for Blazor UI.
    - **[Hangfire](https://www.hangfire.io)** for robust background jobs (cron) solution that also includes a nice
      dashboard. In our experience, *Hangfire* is a great replacement of the basic .NET hosted services functionality.
    - **[NodaTime](https://nodatime.org)** for everything timezone-related. But we use the default .NET `DateTime` type
      everywhere and use *NodaTime* only for calculating between different timezones.
    - **[Humanizer](https://github.com/Humanizr/Humanizer)** for formatting of singular/plural or relative times.
    - **[Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** for OpenAPI specification and Swagger
      UI.
    - **Application Insights** for performance and error monitoring and also as a log sink for logging.

## Features

- REST API supports authentication via Cookies or via API Key.

## Code Style

We try to stick as closely as possible to the code style that is suggested by Microsoft and is "included" out-of-the-box
in Visual Studio or Jetbrains Rider - so no surprises here. In addition, we made the following choices:

- An `.editorconfig` file is on root level of the project to enforce styles throughout IDEs.
- Nullable is enabled in every project, and compiler warnings are treated as errors. This forces every developer to
  fully embrace nullability checks.
- We use file-scoped namespaces, as they make the C# files more readable.

## Architecture

We take cues from the [Clean Architecture](https://ardalis.com/clean-architecture-asp-net-core/) style of doing things
but made the deliberate decision to not include advanced patterns like specifications. We feel that this decision helps
to keep the architecture simpler, easier to understand for beginners and more in line with standard Microsoft
tutorials. But we still enforce a strict separation between business models and infrastructure, while ditching Clean
Architecture patterns like specifications.

The solution contains the following projects
**Frontend:**

- `Shared` is shared between backend and frontend contains the POCOs for the REST API.
- `BlazorGui` contains the Blazor Webassembly and only references `Shared`.

**Backend:**

- `Server.Core` contains the business models and business logic as well as abstractions (interfaces). `Server.Core` does
  not reference
  any other projects and is therefore at the "bottom" of the dependency tree.
- `Server.Infrastructure` references only `Server.Core` and contains the Entity Framework DbContext (using the business
  models from `Server.Core`), the repository implementations as well as database configuration and database migrations.
  This pattern is taken from Clean Architecture as it makes the business logic not depend on database logic.
- `Shared` is shared between backend and frontend contains the POCOs for the REST API.
- `Server.Web` is the ASP.NET Core application and references `Server.Core` as well as `Server.Infrastructure` and
  brings those two together. As a typical ASP.NET Core application it contains controllers for the REST
  AOI as well as logging, dependency injection, authentication/authorization, Swagger, background jobs (using *
  Hangfire*) and all the other stuff required for an ASP.NET Core application and that is not already contained
  in `Server.Core`
  or `Server.Infrastructure`.

**Notes**

- We strictly adhere to this dependency tree. For example, we would never reference `Server.Infrastructure`
  in `Server.Core`.
- `Core` contains business logic and domain models, but not any information about infrastructure (database). This allows
  for testing of business logic without the need for any complex mocking of databases. This separation is achieved by
  using a simple Repository pattern instead of directly using the Entity Framework DbContext.

## Database

- We are using Entity Framework Core Migrations to set up the entire database. This enables a nice "clone and run"
  workflow for new development environments, as the database will automatically be created in local SQL Server.
- Because the migrations are in a different project than the startup application, we need to define the startup project
  when scripting migrations,
  like `dotnet ef migrations --startup-project ./../Adliance.Project.Server.Web add <MIGRATION_NAME>`.
- We always use repositories instead of using the DbContext directly. This is useful for improved testability, because
  then we don't need DbContext in our `Server.Core` business logic layer.
- We differentiate between `IReadonlyRepository` (just for reading data) and `IRepository` (for reading and writing
  data). This is useful because `IReadonlyRepository` enforces the use of `AsNoTracking` for improved performance of
  Entity Framework.
- We use the Temporal Tables (System Version Tables) feature of SQL Server to create an audit trail of changes of some
  of our tables. This tracks each and every change to the affected tables, but also includes quite some overhead on the
  database server (additional storage requirements and additional execution costs for updates or deletes) so this
  feature should only be used when necessary (it's nice to track everything just in case one might need it in the
  future, but this may lead to unexpected database load.

## Authentication & Logging

- We support authentication by cookies (for GUI) and authentication by API key (for external API clients that use the
  REST API). Both authentication methods use the ASP.NET Core authentication middleware. If a API key is provided it
  automatically uses the API key authentication, otherwise the cookie authentication.
    - This is also nice because it allows authenticated users to use the Swagger UI without the need for an API key.
- We use AzureAD to authenticate GUI users and then use cookie authentication for each subsequent request.
- We log the full request and response for each API call (the entire JSON). Please note that this will also potentially
  log personal information and will result in a lot of data in the database, that's why we also delete the logs after a
  few days via a background job.
- We use the existing ASP.NET Core roles model (via claims based authentication) to support different user roles.
  This also hides if an actual user (cookie authentication) or an API key (HTTP header authentication) is requesting
  something, the handling of different roles will be the same in the code and access to the currently authenticated
  user (or API key) is hidden via `ICurrentUser`.
- Cookie based authentication is built with the Blazor authentication state, these resources where used for
  inspiration:
    - https://github.com/damienbod/AspNetCore6Experiments
    - https://github.com/berhir/BlazorWebAssemblyCookieAuth

## Miscellaneous

- We use the `IOptions` pattern to read options from `appsettings.json` or environment variables.
- One background job automatically removes API call logs after a few days.
- We support different cultures by using the default ASP.NET Core view localization and request localization.
- We provide the endpoint `/health` for automated health checks (including a check if database is available).
- We make sure to provide full commenting (C# comments and *Swashbuckle* attributes) of our REST API endpoints so that
  these will be used in the automatic OpenAPI documentation of the API. This includes error codes.
- We try to keep the controllers as small as possible (for easy readability, and controllers already get quite long
  due to all the embedded OpenAPI documentation). So we use distinct "factories" that encapsulate the handling of API
  requests and the construction of the API responses. Controllers only contain the actions and their documentation, the
  actual business logic is encapsulated in these "factories".
- We use the `System.Text.Json` JSON serialization (no more *Newtonsoft*). This makes JSON serialization much more
  strict, as Newtonsoft tries very hard to parse JSON to match the target type - `System.Text.Json` just throws
  exceptions. But this strictness has the advantage of a better performance and it makes it easier to spot errors.

## Testing

- We put unit tests on business logic in `Server.Core.Test` (as `Server.Core` contains the business logic).
- Integration tests are in `Server.Web.Test` and use the ASP.NET Core `TestHost` pattern. The integration tests require
  a full database server (SQL Server) to run, as some functionality (Temporal Tables) are specific to SQL Server and not
  available in an in-memory database or *SQLite*.

## Blazor UI

- DevExpress is used in the Frontend as UI library. To make the frontend project work,
  obtain a [free trial license](https://www.devexpress.com/Products/Try/) and configure the `nuget.config` on root level
  with
  the [Nuget Feed URL](https://docs.devexpress.com/GeneralInformation/116042/installation/install-devexpress-controls-using-nuget-packages/obtain-your-nuget-feed-credentials)
  .
