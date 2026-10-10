# DatingApp — Project Architecture & Learning Guide 📘

> **Welcome Back!**  
> If you've been away from this project for a while, this document is your single source of truth. It explains the entire architecture, how the frontend and backend talk to each other, how security works, and how to run and debug the code.

---

## 📑 Table of Contents
1. [Project Overview & Tech Stack](#1-project-overview--tech-stack)
2. [Quick Start (How to Run Everything)](#2-quick-start-how-to-run-everything)
3. [The Big Picture: How a Request Travels](#3-the-big-picture-how-a-request-travels)
4. [Backend Deep Dive (.NET 10 Web API)](#4-backend-deep-dive-net-10-web-api)
   - [File 1: AppUser.cs (The Entity Blueprint)](#file-1-appusercs-the-entity-blueprint)
   - [File 2: AppDbContext.cs (The Database Bridge)](#file-2-appdbcontextcs-the-database-bridge)
   - [File 3: appsettings.json & Program.cs (The Engine Room)](#file-3-appsettingsjson--programcs-the-engine-room)
   - [File 4: BaseApiController.cs (The Shared Route Template)](#file-4-baseapicontrollercs-the-shared-route-template)
   - [File 5: RegisterDto.cs & LoginDto.cs (Incoming Data Packages)](#file-5-registerdtocs--logindtocs-incoming-data-packages)
   - [File 6: AccountController.cs (Registration & Login Logic)](#file-6-accountcontrollercs-registration--login-logic)
5. [How Passwords Work: Hashing & Salting Explained](#5-how-passwords-work-hashing--salting-explained)
6. [How URLs Are Built (The Lego Blocks Rule)](#6-how-urls-are-built-the-lego-blocks-rule)
7. [Frontend Overview (Angular + Tailwind + DaisyUI)](#7-frontend-overview-angular--tailwind--daisyui)
8. [Debugging in Visual Studio (Key Cheat Sheet)](#8-debugging-in-visual-studio-key-cheat-sheet)
9. [Common Gotchas & How to Fix Them](#9-common-gotchas--how-to-fix-them)

---

## 1. Project Overview & Tech Stack

This project is a full-stack Dating Application composed of two independent projects:
* **Backend (`/API`)**: Built with **.NET 10 (C#)**, **ASP.NET Core Web API**, and **Entity Framework Core 10**.
* **Database**: Lightweight **SQLite** stored locally as `dating.db`.
* **Frontend (`/client`)**: Built with **Angular (v21)** using standalone components and signals, styled with **Tailwind CSS (v4)** and **daisyUI (v5)**.

---

## 2. Quick Start (How to Run Everything)

### A. Run the Backend API
Open a terminal in `c:\LearningPartner\DatingApp\API`:
```bash
dotnet watch
```
* **URL:** `https://localhost:5001`
* *(Using `dotnet watch` automatically reloads the API whenever you change code).*

### B. Run the Frontend Client
Open a second terminal in `c:\LearningPartner\DatingApp\client`:
```bash
npm start
```
* **URL:** `http://localhost:4200`

### C. Testing Endpoints in Postman
* **Important Setting**: In Postman, go to **Settings (⚙️) -> General** and turn **OFF** `SSL certificate verification` (because `localhost:5001` uses a local dev certificate).
* **Base URL**: `https://localhost:5001/api/...`

---

## 3. The Big Picture: How a Request Travels

```
[Browser / Angular (localhost:4200)]
               │
               ▼  1. HTTP Request (e.g. POST /api/account/register)
[Program.cs (CORS Check & Route Matching)]
               │
               ▼  2. Forward request to controller
[AccountController.cs]
               │  3. Convert JSON to RegisterDto
               │  4. Compute PasswordHash & PasswordSalt with HMACSHA512
               ▼  5. Add user to context
[AppDbContext.cs (Entity Framework Core)]
               │
               ▼  6. Translates C# to SQL: INSERT INTO Users ...
[dating.db (SQLite Database File)]
               │
               ▼  7. Row saved to disk
[AccountController.cs]
               │
               ▼  8. Returns HTTP 200 OK + User JSON
[Angular (app.ts)]
               │  9. Signals updated (members.set(...))
               ▼
[Screen renders profile card with Tailwind & DaisyUI]
```

---

## 4. Backend Deep Dive (.NET 10 Web API)

### File 1: `AppUser.cs` (The Entity Blueprint)
📍 Location: `API/Entities/AppUser.cs`

This class defines **what a User is** in our C# code and database:

```csharp
namespace API.Entities;

public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
}
```
* `Id`: Automatically gets a unique GUID string (e.g. `"3f2504e0-..."`) if not supplied.
* `required`: Guardrail in C# ensuring an `AppUser` cannot be created without these values.
* `PasswordHash` & `PasswordSalt`: Raw binary byte arrays (`byte[]`) that store encrypted password data instead of plain text.

---

### File 2: `AppDbContext.cs` (The Database Bridge)
📍 Location: `API/Data/AppDbContext.cs`

This class is the **translator** between C# and SQLite. It inherits from EF Core's `DbContext`:

```csharp
namespace API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
}
```
* `(DbContextOptions<AppDbContext> options) : DbContext(options)`: Receives database settings (like *"use SQLite and file dating.db"*) from `Program.cs` and passes them to the parent `DbContext` engine.
* `public DbSet<AppUser> Users { get; set; }`: **Creates and manages the `Users` table in SQLite**.
  * Each row in this table follows the `AppUser` blueprint.
  * Writing `context.Users.ToListAsync()` tells EF Core to generate: `SELECT * FROM Users;`.

---

### File 3: `appsettings.json` & `Program.cs` (The Engine Room)

#### `appsettings.json`
📍 Location: `API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data source=dating.db"
  }
}
```
Tells .NET that our database file is `dating.db` located right in the project root.

#### `Program.cs`
📍 Location: `API/Program.cs`
The very first file that executes when the server starts up:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Pack the Toolbox (Register Services)
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(); // Allow localhost:4200 (Angular) to talk to API
builder.Services.AddOpenApi();

var app = builder.Build();

// 2. Open the Doors (Middleware Pipeline)
app.UseCors();        // Enable CORS
app.MapControllers(); // Match incoming URLs to Controllers
app.Run();            // Start listening on https://localhost:5001
```

---

### File 4: `BaseApiController.cs` (The Shared Route Template)
📍 Location: `API/Controllers/BaseApiController.cs`

```csharp
[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
}
```
* Acts as a **shared parent** so we don't have to repeat `[Route(...)]` and `[ApiController]` above every controller.
* Any controller inheriting from this gets the automatic route: `api/[name-of-controller]`.

---

### File 5: `RegisterDto.cs` & `LoginDto.cs` (Incoming Data Packages)
📍 Location: `API/DTOs/`

A **DTO (Data Transfer Object)** is a lightweight class used solely to catch JSON sent from the browser or Postman.

#### `RegisterDto.cs`:
```csharp
namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(4)]
    public string Password { get; set; } = "";
}
```
* Uses **Data Annotations** (`[Required]`, `[EmailAddress]`, `[MinLength(4)]`) so .NET automatically validates user input and returns an HTTP `400 Bad Request` if something is invalid.

#### `LoginDto.cs`:
```csharp
namespace API.DTOs;

public class LoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
```
* Only carries `Email` and `Password` needed for logging in.

---

### File 6: `AccountController.cs` (Registration & Login Logic)
📍 Location: `API/Controllers/AccountController.cs`

This controller handles `POST /api/account/register` and `POST /api/account/login`.

#### Method 1: `EmailExists` (Helper)
```csharp
private async Task<bool> EmailExists(string email)
{
    return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
}
```
* Checks SQLite if an email is already taken (case-insensitive) before creating a new user.

#### Method 2: `Register`
```csharp
[HttpPost("register")]
public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
{
    // 1. Prevent duplicate email
    if (await EmailExists(registerDto.Email)) return BadRequest("Email Taken");

    // 2. Generate random cryptographic key (salt)
    using var hmac = new HMACSHA512();

    // 3. Create AppUser and scramble password
    var user = new AppUser
    {
        DisplayName = registerDto.DisplayName,
        Email = registerDto.Email,
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        PasswordSalt = hmac.Key // The random secret key
    };

    // 4. Save to SQLite
    context.Users.Add(user);
    await context.SaveChangesAsync();

    // 5. Return 200 OK + User JSON
    return user;
}
```

#### Method 3: `Login`
```csharp
[HttpPost("login")]
public async Task<ActionResult<AppUser>> Login(LoginDto loginDto)
{
    // 1. Find the user in SQLite
    var user = await context.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == loginDto.Email.ToLower());
    if (user == null) return Unauthorized("Invalid email address");

    // 2. Recreate HMAC using the user's stored salt!
    using var hmac = new HMACSHA512(user.PasswordSalt);

    // 3. Hash the password entered during login
    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

    // 4. Compare byte-by-byte
    for (var i = 0; i < computedHash.Length; i++)
    {
        if (computedHash[i] != user.PasswordHash[i])
        {
            return Unauthorized("Invalid password");
        }
    }

    // 5. Success
    return user;
}
```

---

## 5. How Passwords Work: Hashing & Salting Explained

We **never** store passwords as plain text strings.

### At Registration:
1. User enters: `"Secret123"`.
2. `HMACSHA512` generates a unique random key (**`PasswordSalt`**), e.g. `[12, 85, 99]`.
3. The password and salt are run through the mathematical hash function to produce **`PasswordHash`**, e.g. `[204, 55, 11]`.
4. Both byte arrays are saved to SQLite.

### At Login:
1. User enters: `"Secret123"`.
2. API fetches the user's **`PasswordSalt`** (`[12, 85, 99]`) from SQLite.
3. API runs the newly entered password through `HMACSHA512` with that exact salt.
4. The result (`computedHash`) is compared against the stored `PasswordHash`:
   * `[204, 55, 11] == [204, 55, 11]` ✅ **Login Successful!**
   * If even 1 byte is different ❌ **401 Unauthorized!**

---

## 6. How URLs Are Built (The Lego Blocks Rule)

Take this URL:  
👉 **`https://localhost:5001/api/account/register`**

| Part | Value | Source in Code |
| :--- | :--- | :--- |
| **Server** | `https://localhost:5001` | Defined in `launchSettings.json` |
| **Prefix** | `/api/` | Defined in `[Route("api/[controller]")]` on `BaseApiController` |
| **Controller** | `account` | Taken from `AccountController` (with "Controller" stripped) |
| **Action** | `/register` | Defined in `[HttpPost("register")]` on the method |

---

## 7. Frontend Overview (Angular + Tailwind + DaisyUI)

📍 Location: `client/`

### Styling
* **Tailwind CSS v4** + **daisyUI v5** configured via `client/src/styles.css`:
  ```css
  @import "tailwindcss";
  @plugin "daisyui";
  ```
* Provides pre-styled components: `navbar`, `card`, `avatar`, `badge`, `btn btn-primary`.

### Angular Component (`client/src/app/`)
* **`app.ts`**: Uses modern Angular Signals:
  ```typescript
  protected members = signal<any>([]);

  async ngOnInit() {
    this.members.set(await this.getMembers());
  }

  async getMembers() {
    return lastValueFrom(this.http.get('https://localhost:5001/api/members'));
  }
  ```
* **`app.html`**: Uses control flow `@for` and `@empty` to render cards for all members loaded from the backend API.

---

## 8. Debugging in Visual Studio (Key Cheat Sheet)

1. **Set a Breakpoint**: Click the gray left margin next to a line of code in Visual Studio (or press **`F9`**). A solid red dot appears.
2. **Start Debugger**: Click the solid green **`▶ https`** button (or press **`F5`**).
3. **Send Request**: Click **Send** in Postman. Visual Studio will pause on the breakpoint with a yellow arrow.
4. **Navigation Keys**:
   * **`F10` (Step Over)**: Move to the next line in the same method.
   * **`F11` (Step Into / Jump In)**: Dive inside a method (e.g. jumping inside `EmailExists`).
   * **`Shift + F11` (Step Out / Jump Out)**: Finish the current helper method and jump back out to where you came from.
   * **`F5` (Continue)**: Resume execution until the next breakpoint or finish the request.
   * **`Shift + F5` (Stop)**: Stop the debugger session.

---

## 9. Common Gotchas & How to Fix Them

### Gotcha 1: `Failed to bind to address https://127.0.0.1:5001: address already in use`
* **Cause**: Another terminal or process is already running the API on port 5001.
* **Fix**: Find the terminal running `dotnet watch` or `dotnet run` and press `Ctrl + C`, or kill the process in PowerShell:
  ```powershell
  Get-Process -Id (Get-NetTCPConnection -LocalPort 5001).OwningProcess | Stop-Process -Force
  ```

### Gotcha 2: Empty EF Core Migration (`Up()` has nothing inside)
* **Cause**: Running `dotnet ef migrations add ...` before saving changes to `AppUser.cs`.
* **Fix**:
  1. Remove empty migration: `dotnet ef migrations remove`
  2. Add properties to `AppUser.cs` and save the file.
  3. Re-add migration: `dotnet ef migrations add UserEntityUpdated`
  4. Update database: `dotnet ef database update`

### Gotcha 3: Postman 400 Bad Request: "The Email field is required"
* **Cause**: Sending `"mail"` instead of `"email"` in the JSON body.
* **Fix**: Match the exact property names in `RegisterDto`:
  ```json
  {
    "displayName": "Noah",
    "email": "noah@test.com",
    "password": "Password123"
  }
  ```
