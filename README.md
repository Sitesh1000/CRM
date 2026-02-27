# CRM (ASP.NET Core .NET 8 + SQLite)

Production-ready CRM sample with clean architecture, Razor UI, REST APIs, Identity auth, and SQLite.

## Solution Structure

- `Crm.Domain`: entities, enums, base model (`CreatedAt`, `UpdatedAt`, `IsDeleted`)
- `Crm.Application`: DTOs/contracts
- `Crm.Infrastructure`: EF Core, Identity, seed, migrations
- `Crm.Web`: Razor pages, APIs, auth pages, UI shell

## Local Run

1. `dotnet restore Crm.slnx`
2. `dotnet build Crm.slnx`
3. `dotnet run --project Crm.Web`
4. Open the URL shown in console

Demo users:
- `admin / Admin123!`
- `sales / Sales123!`

## Database

- SQLite file path is configurable by connection string.
- Default local path: `Crm.Web/App_Data/crm.db`
- App auto-creates the SQLite folder, applies migrations, and seeds demo data on startup.

## Render Deployment (No Server Needed)

This repo includes `render.yaml` for one-click deploy.

### 1) Push to GitHub

If this folder is not a git repo yet:

```powershell
git init
git add .
git commit -m "Initial CRM app"
git branch -M main
git remote add origin https://github.com/<your-username>/<repo-name>.git
git push -u origin main
```

### 2) Deploy on Render

1. Go to `https://dashboard.render.com`
2. New -> `Blueprint`
3. Connect your GitHub repo
4. Render detects `render.yaml` and creates service `orbit-crm`
5. Deploy

Included deploy settings:
- Build: `dotnet publish Crm.Web/Crm.Web.csproj -c Release -o out`
- Start: `dotnet out/Crm.Web.dll --urls http://0.0.0.0:$PORT`
- Env:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ConnectionStrings__DefaultConnection=Data Source=/var/data/crm.db`
- Persistent disk:
  - mount path: `/var/data`
  - size: `1 GB`

## Custom Domain (`www.sitesh.crm`)

After app is live on Render:

1. Buy/control the domain `sitesh.crm` in your registrar.
2. In Render service -> `Settings` -> `Custom Domains`, add:
   - `www.sitesh.crm`
3. Render gives DNS target records.
4. In your registrar DNS, add the records Render asks for.
5. Wait for DNS propagation.
6. Render provisions SSL automatically.

## EF Migrations

- Add migration:
  - `dotnet ef migrations add <Name> --project Crm.Infrastructure --startup-project Crm.Web --output-dir Persistence/Migrations`
- Apply migration:
  - `dotnet ef database update --project Crm.Infrastructure --startup-project Crm.Web`
