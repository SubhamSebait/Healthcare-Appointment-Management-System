# Healthcare Appointment Management System

A simple, clean, and comprehensive full-stack Web Application for managing doctor appointments, built for BTech/College academic projects.

---

## 📋 Features

### 👤 Patient Role
- **Account Registration & Login**: JWT-based secure authentication.
- **Browse & Filter Doctors**: Search doctors by specialization.
- **View Real-Time Availability**: Check open time slots for any doctor.
- **Book Appointments**: Reserve free slots with custom notes.
- **My Appointments**: Track booking status (Pending, Confirmed, Rejected, Completed, Cancelled) and cancel appointments.

### 👨‍⚕️ Doctor Role
- **Doctor Dashboard**: Overview of total, pending, confirmed, and completed appointments.
- **Manage Appointments**: Confirm or reject pending requests, or mark confirmed appointments as completed with doctor notes.
- **Manage Availability**: Add custom date/time slots and remove unused slots.

### 🛡️ Admin Role
- **System Overview Dashboard**: Monitor system-wide statistics (Doctors, Patients, Appointments, Specializations).
- **Manage Doctors**: Add new doctor accounts with specializations, edit profile details, or delete doctors.
- **Manage Specializations**: Add, edit, or delete medical specializations.
- **View All Patients & Appointments**: Comprehensive system audit views.

---

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 8.0 Web API
- **ORM & Database**: Entity Framework Core 8.0 with SQLite / SQL Server
- **Authentication**: JWT (JSON Web Tokens) with role-based authorization (`Patient`, `Doctor`, `Admin`)
- **Password Security**: BCrypt hashing (`BCrypt.Net-Next`)
- **Logging**: Serilog (Console & daily rolling file sink)
- **API Documentation**: Swagger UI (OpenAPI)
- **Frontend**: Responsive HTML5, Vanilla CSS3 (Custom Design System with CSS variables & dark mode ready aesthetics), Modern Vanilla JavaScript (Fetch API, Async/Await)

---

## 📁 Project Structure

```
HealthcareApp/
├── HealthcareApp.API/                  # ASP.NET Core Web API Project
│   ├── Controllers/
│   │   ├── AdminController.cs          # Admin management endpoints
│   │   ├── AppointmentsController.cs   # Patient & Doctor appointment workflows
│   │   ├── AuthController.cs           # Login & Registration endpoints
│   │   ├── AvailabilityController.cs   # Time slot availability management
│   │   ├── DoctorsController.cs        # Doctor directory endpoints
│   │   └── SpecializationsController.cs # Medical specialization endpoints
│   ├── Data/
│   │   ├── AppDbContext.cs             # EF Core Database Context
│   │   └── DbSeeder.cs                 # Automatic Database Seeder
│   ├── DTOs/                           # Data Transfer Objects
│   ├── Models/                         # Domain Entities (User, Doctor, Patient, Availability, Appointment, Specialization)
│   ├── Services/                       # Business Logic & Token Generation
│   ├── appsettings.json                # Application Configuration
│   ├── Program.cs                      # Startup Configuration & Pipeline
│   └── web.config                      # IIS Deployment Configuration
│
├── HealthcareApp.Web/                  # Static Frontend Application
│   ├── admin/                          # Admin views (dashboard, manage-doctors, manage-specializations, patients, appointments)
│   ├── css/                            # Global CSS stylesheet & theme definitions
│   ├── doctor/                         # Doctor views (dashboard, availability)
│   ├── index.html                      # System Login Page
│   ├── js/                             # API client (api.js) & page scripts
│   ├── patient/                        # Patient views (dashboard, doctors, book-appointment, my-appointments)
│   └── register.html                   # Patient Registration Page
│
├── test_system.ps1                     # End-to-end integration test script
└── HealthcareApp.sln                   # Visual Studio Solution File
```

---

## 🔑 Sample Login Credentials

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@healthcare.com` | `Admin@123` |
| **Doctor** | `arjun@healthcare.com` | `Doctor@123` |
| **Doctor** | `priya@healthcare.com` | `Doctor@123` |
| **Patient** | `rahul@patient.com` | `Patient@123` |
| **Patient** | `neha@patient.com` | `Patient@123` |

---

## 🚀 How to Run Locally

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Python 3.x (or any static HTTP server for frontend hosting)

### 1. Run Backend API
```bash
cd HealthcareApp.API
dotnet run --urls "http://localhost:5000"
```
- The backend will start on `http://localhost:5000`.
- Interactive API documentation (Swagger UI) is available at `http://localhost:5000/swagger`.
- Database schema and initial seed data are automatically initialized on startup (`HealthcareDB.db`).

### 2. Run Frontend Web App
In a separate terminal window:
```bash
cd HealthcareApp.Web
python -m http.server 8080
```
- Open your browser and navigate to `http://localhost:8080`.

---

## 🌐 API Overview

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | Public | Authenticate user & get JWT token |
| `POST` | `/api/auth/register` | Public | Register new patient account |
| `GET` | `/api/doctors` | Public | List all doctors (supports `?specializationId=`) |
| `GET` | `/api/specializations` | Public | List all medical specializations |
| `GET` | `/api/availability/{doctorId}` | Public | Get free time slots for a doctor |
| `POST` | `/api/availability` | Doctor | Add a new availability time slot |
| `DELETE` | `/api/availability/{id}` | Doctor | Delete an unbooked availability slot |
| `POST` | `/api/appointments` | Patient | Book an appointment slot |
| `GET` | `/api/appointments/my` | Patient | View patient's own appointments |
| `PATCH` | `/api/appointments/{id}/cancel` | Patient | Cancel pending/confirmed appointment |
| `GET` | `/api/appointments/doctor` | Doctor | View appointments booked with doctor |
| `PATCH` | `/api/appointments/{id}/confirm` | Doctor | Confirm a pending appointment |
| `PATCH` | `/api/appointments/{id}/reject` | Doctor | Reject a pending appointment |
| `PATCH` | `/api/appointments/{id}/complete` | Doctor | Mark confirmed appointment as completed |
| `GET` | `/api/admin/doctors` | Admin | Manage doctors list |
| `POST` | `/api/admin/doctors` | Admin | Create a doctor account & profile |
| `PUT` | `/api/admin/doctors/{id}` | Admin | Edit doctor details |
| `DELETE` | `/api/admin/doctors/{id}` | Admin | Delete doctor account |
| `GET` | `/api/admin/patients` | Admin | View all registered patients |
| `GET` | `/api/appointments/all` | Admin | View all appointments in system |

---

## ⚙️ IIS Deployment Instructions

1. **Prerequisites**: Install **ASP.NET Core Hosting Bundle 8.0** on IIS Server.
2. **Publish Backend**:
   ```bash
   dotnet publish HealthcareApp.API/HealthcareApp.API.csproj -c Release -o C:\inetpub\wwwroot\HealthcareAPI
   ```
3. **Configure IIS Application Pool**:
   - Create a new AppPool (e.g., `HealthcareAppPool`).
   - Set .NET CLR version to **"No Managed Code"**.
4. **Create IIS Web Site / Application**:
   - Point site physical path to `C:\inetpub\wwwroot\HealthcareAPI`.
   - Ensure `web.config` is present in the published directory (`AspNetCoreModuleV2` handles requests).
5. **Serve Frontend**:
   - Copy `HealthcareApp.Web` folder contents to `C:\inetpub\wwwroot\HealthcareWeb` or configure as a static web site in IIS.
   - Update `API_BASE` in `js/api.js` to point to the production IIS API URL (e.g., `http://your-server-ip/api`).
