# SourceLens — Frontend Dashboard

A React dashboard for the SourceLens claim-verification system. It talks to the
ASP.NET Core Web API (`SourceLensDB`, EF Core) built for Users, Papers, Claims,
Sources, Evidence, and Claim Assessments.

If the API isn't reachable, the app automatically falls back to built-in demo
data so it's always browsable — you'll see a banner at the top of each page
when that happens.

---

## 1. Run it locally

```bash
npm install
npm run dev
```

Opens at `http://localhost:5173`.

## 2. Connect it to your ASP.NET Core backend

1. Copy the example env file:
   ```bash
   cp .env.example .env
   ```
2. Open `.env` and set `VITE_API_BASE_URL` to your API's base URL **including**
   `/api`, e.g.:
   ```
   VITE_API_BASE_URL=https://localhost:7101/api
   ```
   (Find your port in `Properties/launchSettings.json` in the backend project,
   under `applicationUrl`.)
3. Restart `npm run dev` after changing `.env` — Vite only reads it on startup.

### Enable CORS on the backend

Browsers block requests from `http://localhost:5173` to your API unless the
API explicitly allows it. In `Program.cs`, add (before `app.Build()`/`app.Run()`):

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "https://your-deployed-frontend.com")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ...

app.UseCors("AllowFrontend");
```

Make sure `app.UseCors("AllowFrontend")` is called **before** `app.MapControllers()`.

### Expected API routes

The dashboard calls these endpoints (matching your controllers):

| Resource          | Endpoint                  |
|--------------------|----------------------------|
| Users              | `GET/POST /api/Users`, `PUT/DELETE /api/Users/{id}` |
| Papers             | `GET/POST /api/Papers`, `PUT/DELETE /api/Papers/{id}` |
| Claims             | `GET/POST /api/Claims`, `PUT/DELETE /api/Claims/{id}` |
| Sources            | `GET/POST /api/Sources`, `PUT/DELETE /api/Sources/{id}` |
| Evidence           | `GET/POST /api/Evidence`, `PUT/DELETE /api/Evidence/{id}` |
| Claim Assessments  | `GET/POST /api/ClaimAssessments`, `PUT/DELETE /api/ClaimAssessments/{id}` |

If your route names or casing differ, adjust the `endpoint` string passed to
`useResource(...)` at the top of each file in `src/pages/`.

---

## 3. Deploy it — getting it online

The frontend is a static site once built, so it can be hosted almost anywhere
for free. Two easy options:

### Option A: Vercel (recommended, easiest)

1. Push this project to a GitHub repo.
2. Go to vercel.com → **New Project** → import the repo.
3. Vercel auto-detects Vite. Before deploying, add an environment variable:
   - `VITE_API_BASE_URL` = your **deployed** backend's URL + `/api`
4. Click **Deploy**. You'll get a live URL like `sourcelens.vercel.app`.

### Option B: Netlify

1. Push to GitHub, then **Add new site → Import an existing project** on
   netlify.com.
2. Build command: `npm run build`. Publish directory: `dist`.
3. Add `VITE_API_BASE_URL` under **Site settings → Environment variables**.
4. Deploy.

### Deploying the backend too

The frontend needs a **publicly reachable** API URL once deployed (not
`localhost`). Common options for the ASP.NET Core + SQL Server backend:
- **Azure App Service** (free tier available) + **Azure SQL Database**
- **Render** or **Railway** (Docker deploy of the API) + a managed Postgres/SQL
  instance
- IIS on a VM, if your college/organization provides one

Whichever you choose, once the backend has a public URL:
1. Update `VITE_API_BASE_URL` in your frontend's deployment (Vercel/Netlify
   env variable) to point at it.
2. Add that same public frontend URL to the backend's CORS policy (see above).
3. Redeploy the frontend so the new env variable takes effect.

### Manual static hosting (any host)

```bash
npm run build
```

This produces a `dist/` folder — upload its contents to any static host
(GitHub Pages, an S3 bucket, a shared host's `public_html`, etc.).

---

## Project structure

```
src/
  api/
    client.js       # axios instance, reads VITE_API_BASE_URL
    mockData.js      # demo data used when the API is unreachable
  hooks/
    useResource.js   # generic load/create/update/delete + demo fallback
  components/        # Sidebar, Topbar, DataTable, Modal, EntityForm,
                      # ConfidenceDial, VerdictBadge, Aperture (logo), etc.
  pages/
    Dashboard.jsx
    Papers.jsx
    Claims.jsx
    Sources.jsx
    Evidence.jsx
    Assessments.jsx
    Users.jsx
```

Each page in `src/pages/` follows the same pattern: load data via
`useResource("EndpointName", mockData)`, render it in a `DataTable`, and use a
`Modal` + `EntityForm` for create/edit. To add a new field to any entity, add
it to that page's `fields` array and `DataTable` `columns` array.
