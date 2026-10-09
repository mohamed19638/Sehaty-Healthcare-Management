# Sehaty+

A beginner-friendly personal healthcare graduation project. The frontend is a separate, plain HTML/CSS/JavaScript app with an Arabic/English language toggle and RTL layout; the backend is a separate ASP.NET Core Web API. No React or advanced architecture patterns are used.

## Technology

- Backend: ASP.NET Core Web API, C#, .NET 8, Entity Framework Core, SQL Server in `backend/`
- Frontend: HTML, CSS, and vanilla JavaScript in `frontend/`
- Local run: Docker Compose, with separate frontend, API, and SQL Server containers

## Run the complete project with Docker

Requirements: Docker Desktop with Compose enabled.

From the project root in PowerShell:

```powershell
Copy-Item .env.example .env
notepad .env
```

Change the local-only values in `.env` to private values. Use a SQL Server password that meets SQL Server's password policy and a random JWT key of at least 32 bytes. Do not share or commit `.env`.

Then start the services:

```powershell
docker compose up --build -d
docker compose ps
```

Open the website at <http://localhost:8080>. The API Swagger page is at <http://localhost:8081/swagger> or through <http://localhost:8080/swagger>. The API applies EF Core migrations on startup.

The services are separate:

- `frontend`: Nginx serves the static files from `frontend/` and forwards `/api` and `/swagger` requests to the API.
- `api`: ASP.NET Core Web API, project files and C# code in `backend/`.
- `sqlserver`: SQL Server 2022 with persistent data in the `sqlserver-data` Docker volume.

The API is also exposed to the host on port `8081`; the SQL Server port is `14330` on the host (`localhost,14330` in SSMS). The frontend uses the API through its Nginx proxy, so browser requests remain same-origin.

Stop containers without deleting database data:

```powershell
docker compose down
```

Do not use `docker compose down --volumes` unless you explicitly want to delete the SQL Server data volume.

## Run the frontend and API separately for learning

Start SQL Server and the API with the connection string and JWT key configured as environment variables, then start a static server in `frontend/` on port `5500`. `frontend/config.js` points that static site to the API's default HTTP port `5295`. The API allows the local frontend origins `http://localhost:5500` and `http://127.0.0.1:5500`.

```powershell
dotnet restore .\i-am-building-a-simple-graduation.sln
dotnet run --project .\backend\i-am-building-a-simple-graduation.csproj
```

In a second terminal:

```powershell
py -m http.server 5500 --directory .\frontend
```

Then open <http://localhost:5500>. Docker Compose is the easiest way to run the complete stack because it also starts SQL Server and supplies the connection settings.

## Demo login

- Email: `demo@sehaty.com`
- Password: `Demo123!`

The demo user is created if it does not already exist. The password is stored as a password hash. The application no longer creates invented doctor, pharmacy, meal, exercise-video, measurement, or nutrition seed records. Existing records are preserved.

## Features

- Registration and login using JWT authentication
- User-scoped health measurements with date and time
- Adult reference information for blood pressure, fasting blood glucose, and resting pulse, with clear context requirements and non-diagnostic wording
- BMI calculation based on the user's saved height and weight
- Medication, medical record, lab result, and activity logs
- Doctor and pharmacy lists with WhatsApp links only when a valid international number is supplied
- Appointment requests for one user per doctor/time slot, protected by a database unique index
- Healthy meal catalogue, initially empty until real meal data is provided
- Nine owner-provided YouTube workout videos added to the exercise catalogue at API startup
- Measurement export to a real `.xlsx` workbook
- Measurement print view; choose **Save as PDF** in the browser's print dialog
- Explicitly confirmed WhatsApp sharing; Messenger copies the summary so the user can paste it into a conversation manually

Sharing health details is always a user action and requires confirmation. The API exports only the signed-in user's measurements. The website does not send messages, confirm an appointment with a doctor, diagnose a condition, or replace medical advice.

The BMI uses the height saved in the profile and the latest dated weight measurement. Recording a newer weight updates the profile weight; the user can also update height and weight on the BMI page.

## Medical reference context

The simple reference labels are intended for adults and are not diagnoses:

- Blood pressure categories follow the American Heart Association adult chart. A very high reading prompts repeat measurement and clinician guidance; emergency symptoms require urgent care.
- Fasting glucose categories are shown only when the user marks the measurement as fasting. A reading cannot diagnose diabetes; the CDC notes diagnosis requires appropriate clinical testing.
- Resting pulse uses the general adult range of 60–100 beats per minute and is interpreted only when the user marks that they were resting.
- Measurements for people under 18 are not classified using adult categories. Pregnancy, medicines, exercise, symptoms, technique, and medical history can change interpretation.

Sources: [CDC adult BMI categories](https://www.cdc.gov/bmi/adult-calculator/bmi-categories.html), [American Heart Association blood pressure categories](https://www.heart.org/en/health-topics/high-blood-pressure/understanding-blood-pressure-readings), [MedlinePlus typical resting adult vital signs](https://medlineplus.gov/ency/article/002341.htm), [CDC diabetes testing](https://www.cdc.gov/diabetes/diabetes-testing/index.html), and [MedlinePlus pulse information](https://medlineplus.gov/ency/article/003399.htm).

## Appointment schedule assumption

Doctor availability is not currently configured in the database. For this simple version, every doctor has 30-minute slots from 9:00 AM to 5:00 PM Cairo time. Each slot can be requested once. The unique database index protects against two users booking the same slot concurrently. A request is not a confirmation from the doctor. Update this default when real doctor schedules are supplied.

## Add real catalogue data later

Doctor and pharmacy records are stored in the `Doctors` and `Pharmacies` tables. WhatsApp values must be valid international numbers containing digits only, without a `+` prefix or punctuation; the app does not guess country codes. Existing legacy rows marked `PENDING` are hidden, not deleted. Meal records are in `Meals`. Exercise video records are in `Exercises`; only HTTPS YouTube watch, Shorts, or `youtu.be` links with valid video IDs are embedded. No sample catalogue records are generated.

## API routes

| Route | Purpose |
| --- | --- |
| `POST /api/auth/register`, `POST /api/auth/login` | Create an account and sign in |
| `GET /api/auth/me` | Current user's profile |
| `GET /api/dashboard` | Current user's overview |
| `/api/healthmeasurements` | Read, add, update, and delete the current user's measurements |
| `GET /api/bmi`, `PUT /api/bmi` | Read and update height/weight BMI inputs |
| `/api/appointments` | Read and request the current user's appointments |
| `GET /api/appointments/availability?doctorId=1&date=YYYY-MM-DD` | List free Cairo-time slots |
| `/api/export/measurements/xlsx` | Download the signed-in user's measurement workbook |
| `/api/medications`, `/api/medicalrecords`, `/api/labresults`, `/api/fitness` | Current user's health and activity records |
| `GET /api/doctors`, `GET /api/pharmacies?search=` | Public doctor and pharmacy catalogues |
| `GET /api/meals`, `GET /api/exercises` | Public meal and exercise-video catalogues |

Chat and personal calorie/macro logging have been removed from the active API and interface. Their existing database tables are retained so old records are not automatically deleted.

## Database migrations

Migrations are in `backend/Migrations/`. `backend/Data/AppDbContextFactory.cs` lets EF Core create migrations without starting the API or applying them to the configured database. Schema updates only add catalogue/context data and the appointment uniqueness constraint; existing nutrition, chat, and activity calorie columns are retained for data compatibility.

## Project layout

- `frontend/`: HTML, CSS, JavaScript, Nginx config, and frontend Dockerfile
- `backend/Controllers/`: beginner-friendly API controllers
- `backend/DTOs/`: API request models
- `backend/Models/`: EF Core entities
- `backend/Data/AppDbContext.cs`: database tables and relationships
- `backend/Migrations/`: SQL Server schema history
- `compose.yaml`: separate frontend, API, and database services

This is an educational project, not a production medical system.
