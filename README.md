# Accra Road Attendance

A comprehensive desktop application for managing church attendance records, member profiles, and attendance-related reporting for the Accra-Road Congregation.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Database Schema](#database-schema)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

## Overview

**Accra Road Attendance** is a modern WPF-based desktop application built with .NET 8 that streamlines attendance management, member administration, and reporting for church organizations. The application features role-based access control, real-time data synchronization with cloud databases, and comprehensive reporting capabilities.

## Key Features

### Member Management
- **Member Registration**: Add and maintain detailed member profiles with personal, contact, and family information
- **Profile Management**: Edit member details, update status (active/inactive), and manage photographs
- **Advanced Filtering**: Search and filter members by various criteria
- **Membership Tracking**: Track membership start dates, baptism history, and family connections

### Attendance Management
- **Service Attendance**: Mark and track attendance across multiple service types (Sunday morning, evening, midweek, etc.)
- **Attendance Status**: Record presence, absence, and late arrivals
- **Service Themes**: Document service themes and additional notes
- **Visitor & Children Tracking**: Record visitor counts, children attendance, and offerings

### Reporting System
Comprehensive report generation capabilities:
- **Individual Attendance Reports**: Detailed attendance history per member
- **Church Attendance Summary**: Overall attendance statistics by service type
- **Service Type Reports**: Attendance breakdowns by service category
- **Demographic Reports**: Gender-based attendance analysis
- **Offering Reports**: Financial contribution tracking
- **Visitor Reports**: Visitor and newcomer tracking
- **Absentee Reports**: Identify members with low attendance rates
- **PDF Export**: Generate professional PDF reports with church letterhead and formatting

### Data Synchronization
- **Cloud Sync**: Automatic synchronization with online database
- **Offline Mode**: Full functionality available offline with automatic sync when connection restored
- **Internet Detection**: Automatic internet availability checking
- **Progress Tracking**: Real-time sync progress reporting

### User Management
- **Role-Based Access Control**: Admin and User roles with granular permissions
- **User Accounts**: Create, edit, and manage user accounts
- **Admin Only Features**: User management restricted to administrators
- **Current User Tracking**: Track logged-in user and their permissions

### User Experience
- **Dark/Light Theme**: Toggle between dark and light color schemes
- **Responsive UI**: Material Design styling with smooth animations
- **Pagination**: Efficient data navigation with customizable page sizes
- **Search & Filter**: Quick member and record lookup
- **Intuitive Navigation**: Tab-based navigation between modules

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | .NET 8 |
| **Language** | C# 12.0 |
| **Desktop UI** | Windows Presentation Foundation (WPF) |
| **Database** | SQL Server (Local & Cloud) |
| **ORM** | Entity Framework Core |
| **Authentication** | ASP.NET Core Identity |
| **Reporting** | MigraDoc (PDF generation) |
| **Cloud Storage** | Google Drive API (member photos) |
| **UI Components** | Material Design Themes for WPF |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection |

## Project Structure

```
AccraRoadAttendance/
├── Views/
│   ├── MainWindow.xaml                          # Main application shell
│   ├── MainWindow.xaml.cs                       # Main window code-behind
│   ├── SplashScreen.xaml                        # Startup splash screen
│   ├── SplashScreen.xaml.cs                     # Splash screen code-behind
│   ├── Login.xaml                               # Login window
│   ├── Login.xaml.cs                            # Login code-behind
│   ├── LogoSplashWindow.xaml                    # Logo splash window
│   ├── LogoSplashWindow.xaml.cs                 # Logo splash code-behind
│   │
│   └── Pages/
│       ├── Dashboard/
│       │   ├── Dashboard.xaml                   # Dashboard/home page
│       │   └── Dashboard.xaml.cs                # Dashboard code-behind
│       │
│       ├── Attendance/
│       │   ├── MarkAttendance.xaml              # Attendance marking UI
│       │   ├── MarkAttendance.xaml.cs           # Attendance code-behind
│       │   └── VisitorsInputWindow.xaml         # Visitor input dialog
│       │
│       ├── Members/
│       │   ├── Members.xaml                     # Members list/management
│       │   ├── Members.xaml.cs                  # Members code-behind
│       │   ├── AddMembers.xaml                  # Add new member form
│       │   ├── AddMembers.xaml.cs               # Add members code-behind
│       │   ├── EditMembers.xaml                 # Edit member form
│       │   ├── EditMembers.xaml.cs              # Edit members code-behind
│       │   ├── MemberDetails.xaml               # Member detail view
│       │   └── MemberDetails.xaml.cs            # Member details code-behind
│       │
│       ├── Reports/
│       │   ├── ReportsPage.xaml                 # Reports generation UI
│       │   ├── ReportsPage.xaml.cs              # Reports code-behind
│       │   └── ReportGenerator.cs               # PDF report generation logic
│       │
│       └── Users/
│           ├── UsersManagement.xaml             # User management UI
│           └── UsersManagement.xaml.cs          # User management code-behind
│
├── Services/
│   ├── INavigationService.cs                    # Navigation service interface
│   ├── NavigationService.cs                     # Navigation service implementation
│   ├── CurrentUserService.cs                    # Current user session tracking
│   ├── GoogleDriveService.cs                    # Google Drive API integration
│   ├── SyncService.cs                           # Database synchronization service
│   └── IParameterReceiver.cs                    # Parameter receiver interface
│
├── Data/
│   ├── AttendanceDbContext.cs                   # Local database context
│   └── OnlineAttendanceDbContext.cs             # Online database context
│
├── Models/
│   ├── Member.cs                                # Member model
│   ├── Attendance.cs                            # Attendance record model
│   ├── ChurchAttendanceSummary.cs               # Attendance summary model
│   ├── User.cs                                  # User account model
│   └── ServiceType.cs                           # Service type enumeration
│
├── Resources/
│   ├── service_key.enc                          # Encrypted Google Drive credentials
│   └── (Configuration files)
│
├── AppImages/
│   ├── CLogoc.png                               # Logo (light theme)
│   ├── CLogocw.png                              # Logo (dark theme)
│   ├── CLogoCropped.png                         # Cropped logo for reports
│   └── (Other image assets)
│
├── Migrations/
│   ├── [YYYYMMDDHHmmss]_InitialCreate.cs        # Initial database migration
│   ├── [YYYYMMDDHHmmss]_AddMemberFields.cs      # Add member fields migration
│   ├── [YYYYMMDDHHmmss]_AddAttendanceFields.cs  # Add attendance fields migration
│   └── (More migration files)                   # Additional migrations
│
├── Properties/
│   ├── PublishProfiles/
│   │   ├── ClickOnceProfile.pubxml              # ClickOnce publish profile 1
│   │   └── ClickOnceProfile2.pubxml             # ClickOnce publish profile 2
│   ├── launchSettings.json                      # Launch settings
│   └── AssemblyInfo.cs                          # Assembly information
│
├── App.xaml                                     # Application XAML configuration
├── App.xaml.cs                                  # Application code-behind
├── appsettings.json                             # Application settings (database connections)
├── appsettings.Development.json                 # Development environment settings
├── appsettings.Production.json                  # Production environment settings
├── AccraRoadAttendance.csproj                   # Project file
├── README.md                                    # This file
├── LICENSE                                      # MIT License
└── .gitignore                                   # Git ignore rules
```

### Key Directories Explained

#### `/Views`
Contains all WPF window and page definitions with their code-behind files. Organized by feature modules (Dashboard, Attendance, Members, Reports, Users).

#### `/Services`
Business logic layer containing:
- Navigation service for page routing
- User session management
- Database synchronization
- Google Drive API integration

#### `/Data`
Entity Framework Core database contexts:
- `AttendanceDbContext`: Local database
- `OnlineAttendanceDbContext`: Cloud database for syncing

#### `/Models`
Data model classes representing database entities and domain objects.

#### `/Migrations`
Entity Framework Core migration files for database schema versioning.

#### `/Resources`
Application resources including encrypted credentials and configuration files.

#### `/AppImages`
Image assets used throughout the application (logos, icons).

#### `/Properties`
Project properties, publish profiles, and assembly information.

## Installation

### Prerequisites
- **Windows 10** or later (Windows 11 recommended)
- **.NET 8 Runtime** (v8.0 or higher) OR **.NET 8 SDK** for development
- **SQL Server 2019+** or **SQL Server LocalDB** (included with Visual Studio)
- **Visual Studio 2022** (Community, Professional, or Enterprise edition)
  - Workload: **.NET desktop development**
  - Workload: **Data storage and processing**
- **Internet connection** (for first-time data sync and Google Drive integration)
- **Minimum 500 MB** free disk space (database and application files)

### System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **OS** | Windows 10 (Build 19041) | Windows 11 Pro/Enterprise |
| **.NET** | 8.0 Runtime | 8.0 SDK |
| **RAM** | 2 GB | 4 GB+ |
| **CPU** | Dual-core 2.0 GHz | Quad-core 2.5 GHz+ |
| **Storage** | 500 MB | 1 GB |
| **SQL Server** | LocalDB (2019) | SQL Server 2019+ Express/Full |

### Step-by-Step Setup

#### 1. Clone the Repository

```bash
git clone https://github.com/Helmut-Essien/AccraRoadAttendance.git
cd AccraRoadAttendance
```

#### 2. Open in Visual Studio

1. Open **Visual Studio 2022**
2. Click **File** → **Open** → **Project/Solution**
3. Navigate to the cloned directory and select `AccraRoadAttendance.csproj`
4. Wait for Visual Studio to load the project and restore NuGet packages

**Verify Workloads:**
- Go to **Tools** → **Get Tools and Features**
- Ensure these workloads are installed:
  - **.NET desktop development**
  - **Data storage and processing**

#### 3. Configure Database Connection

Edit `appsettings.json` in the project root:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=AttendanceDb;Integrated Security=True;Trusted_Connection=True;",
    "OnlineConnection": "Server=your-server;Database=your-database;User Id=your-username;Password=your-password;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  }
}
```

**For LocalDB (Default):**
- The connection string above works with Windows authentication
- SQL Server LocalDB must be installed with Visual Studio

**For Remote SQL Server:**
- Update `Server` with your server address
- Update `Database` with your database name
- Update `User Id` and `Password` with valid credentials

#### 4. Install Dependencies

```bash
dotnet restore
```

Or in Visual Studio:
- **Tools** → **NuGet Package Manager** → **Package Manager Console**
- Run: `Update-Package`

#### 5. Initialize the Database

In **Package Manager Console**:

```powershell
Add-Migration InitialCreate
Update-Database
```

Or using CLI:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**This will:**
- Create the database (if it doesn't exist)
- Apply all migrations to establish the schema
- Seed default roles: **Admin** and **User**
- Create the default admin account
- Create necessary indexes and relationships

#### 6. Build the Application

```bash
dotnet build
```

Or in Visual Studio:
- **Build** → **Build Solution** (Ctrl+Shift+B)

Expected output: `Build succeeded.`

#### 7. Run the Application

```bash
dotnet run
```

Or in Visual Studio:
- Press **F5** or click **Start** (▶)

**Launch Sequence:**
1. **Logo Splash Screen** appears (AccraRoad logo)
2. **Database Initialization Check** - validates database connection
3. **Internet Connectivity Detection** - checks for internet access
4. **Automatic Data Sync** (if internet available) - syncs with cloud database
5. **Login Screen** - ready for user authentication

### Default Credentials

After initial setup, log in with these credentials:

| Field | Value |
|-------|-------|
| **Email** | admin@example.com |
| **Password** | Admin@123 |

⚠️ **CRITICAL SECURITY NOTICE**: 
- Change these credentials **immediately** after first login
- Use a strong, unique password (minimum 12 characters)
- Store the credentials securely
- Never share these credentials with unauthorized users
- Consider implementing additional security policies (password expiration, account lockout)

## Configuration

### appsettings.json Structure

- `ConnectionStrings`: Database connection strings
- `Logging`: Log file configuration
- `AllowedHosts`: CORS settings

### Environment-Specific Settings

- `appsettings.Development.json`: Used during development
- `appsettings.Production.json`: Used in production environment

Create environment-specific files to override default settings based on the hosting environment.

### Google Drive Integration

The application uses Google Drive for storing member profile pictures:

1. Encrypted credentials are stored in `Resources/service_key.enc`
2. Decryption passphrase is defined in `GoogleDriveService.cs`
3. Profile pictures folder ID: `1gN_Hhie-bN7FGIm_MN3DQ1QZF95eHRjR`

To configure your own Google Drive integration:
1. Create a Google Cloud project
2. Enable the Google Drive API
3. Create a service account
4. Encrypt the JSON key with the application's passphrase
5. Update the folder ID in `GoogleDriveService.cs`

## Usage

### Logging In

1. Launch the application
2. Enter your credentials on the login screen
3. Click **Sign In**
4. The splash screen shows sync progress
5. Dashboard loads automatically

### Adding Members

1. Click the **Members** navigation button
2. Click **Add Member**
3. Fill in all required fields:
   - First Name (required)
   - Last Name (required)
   - Gender (required)
   - Phone Number (required, 10 digits)
   - Date of Birth (required)
   - Nationality (required)
   - Address (required)
   - Marital Status (required)
   - Occupation Type (required)
   - Education Level (required)
4. Optional fields:
   - Email address
   - Other names
   - Profile picture (click "Upload Picture")
   - Baptism information
   - Family member details
   - Spouse information
   - Emergency contact

5. Click **Save Member**

### Recording Attendance

1. Click the **Mark Attendance** navigation button
2. Select the **Service Date** (calendar picker)
3. Select the **Service Type** (Sunday morning, evening, etc.)
4. Optionally add a **Service Theme**
5. Mark each member's attendance:
   - Click the **Status** column to toggle between Present/Absent
6. Enter visitor information:
   - Click **Save Attendance**
   - A dialog appears for visitor, children, and offering data
7. Click **Save** to confirm and store records

**Features:**
- Automatic pagination based on window size
- Search and filter members
- Real-time attendance totals
- Previous attendance records are replaced (not duplicated)

### Generating Reports

1. Click the **Reports** navigation button
2. Select the **Report Type**:
   - Individual Attendance
   - Church Attendance Summary
   - Service Type Report
   - Demographic Report
   - Offering Report
   - Visitor and Newcomer Report
   - Absentee Report

3. Select date range (Start Date and End Date)
4. Depending on report type, select additional parameters:
   - **Individual Attendance**: Select a member
   - **Service Type Report**: Select service type
   - **Absentee Report**: Enter absent threshold (%)

5. Click **Generate Report**
   - Report appears in the data grid
6. Click **Print to PDF**
   - Choose save location
   - PDF is generated with church letterhead

### User Management (Admin Only)

1. Click the **Users** navigation button (requires Admin role)
2. **Create New User**:
   - Click **Add User**
   - Enter email address (must be unique)
   - Set password
   - Select role (Admin or User)
   - Click **Save**

3. **Edit User**:
   - Click on a user in the list
   - Update information
   - Click **Save**

4. **Delete User**:
   - Select user
   - Click **Delete**
   - Confirm deletion

### Theme Management

Click the **Theme Toggle** button (sun/moon icon) in the top right to:
- Switch between dark and light themes
- Automatically updates logo and UI colors
- Settings persist across sessions

## Database Schema

### Core Tables

#### Members
Stores member profile information.

| Column | Type | Constraints |
|--------|------|-------------|
| Id | string | Primary Key, ULID |
| FirstName | string | Required, Indexed |
| LastName | string | Required, Indexed |
| OtherNames | string | Optional |
| Sex | int | Enum (Gender) |
| PhoneNumber | string | Required, Unique Index |
| Email | string | Optional, Unique Index |
| DateOfBirth | DateTime | Optional |
| Nationality | string | Optional |
| Address | string | Optional |
| Location | string | Optional |
| PicturePath | string | Optional |
| IsBaptized | bool | Default: false |
| BaptismDate | DateTime | Optional |
| PlaceOfBaptism | string | Optional |
| maritalStatus | int | Enum (MaritalStatus) |
| SpouseName | string | Optional |
| SpouseContact | string | Optional |
| occupationType | int | Enum (OccupationType) |
| OccupationDescription | string | Optional |
| educationalLevel | int | Enum (EducationalLevel) |
| HasFamilyMemberInChurch | bool | Default: false |
| FamilyMemberName | string | Optional |
| FamilyMemberContact | string | Optional |
| NextOfKinName | string | Optional |
| NextOfKinContact | string | Optional |
| Skills | string | Optional |
| Hometown | string | Optional |
| MotherName | string | Optional |
| MotherContact | string | Optional |
| FatherName | string | Optional |
| FatherContact | string | Optional |
| IsActive | bool | Default: true, Indexed |
| MembershipStartDate | DateTime | Default: UtcNow |

#### Attendances
Records individual attendance for each service.

| Column | Type | Constraints |
|--------|------|-------------|
| Id | string | Primary Key, ULID |
| MemberId | string | Foreign Key (Members) |
| ServiceDate | DateTime | Required, Indexed |
| ServiceType | int | Enum (ServiceType) |
| Status | int | Enum (AttendanceStatus) |
| Notes | string | Optional |
| RecordedAt | DateTime | Default: UtcNow |
| Member | Member | Navigation Property |

#### ChurchAttendanceSummaries
Summarizes attendance statistics per service date.

| Column | Type | Constraints |
|--------|------|-------------|
| Id | string | Primary Key, ULID |
| SummaryDate | DateTime | Required, Indexed |
| ServiceType | int | Enum (ServiceType) |
| TotalMembers | int | Required |
| TotalPresent | int | Required |
| TotalMalePresent | int | Required |
| TotalFemalePresent | int | Required |
| Visitors | int | Required |
| Children | int | Required |
| OfferingAmount | decimal | Required |
| ServiceTheme | string | Optional |
| SummaryLastModified | DateTime | Default: UtcNow |

#### AspNetUsers
ASP.NET Core Identity user accounts.

| Column | Type | Constraints |
|--------|------|-------------|
| Id | string | Primary Key |
| UserName | string | Required, Unique |
| Email | string | Required, Unique |
| PasswordHash | string | Required |
| SecurityStamp | string | Required |
| ConcurrencyStamp | string | Required |
| EmailConfirmed | bool | Required |
| LockoutEnabled | bool | Required |
| AccessFailedCount | int | Required |

#### AspNetRoles
Role definitions (Admin, User).

| Column | Type | Constraints |
|--------|------|-------------|
| Id | string | Primary Key |
| Name | string | Required, Unique |
| NormalizedName | string | Required, Unique |
| ConcurrencyStamp | string | Required |

### Enumerations

#### Gender
- **0**: Other/Prefer not to say
- **1**: Male
- **2**: Female

#### MaritalStatus
- **0**: Single
- **1**: Married
- **2**: Divorced
- **3**: Widowed

#### OccupationType
- **0**: Unemployed
- **1**: Employed
- **2**: Self-employed
- **3**: Student
- **4**: Retired

#### EducationalLevel
- **0**: None
- **1**: Primary
- **2**: Secondary
- **3**: Tertiary
- **4**: Diploma
- **5**: Degree
- **6**: Postgraduate

#### ServiceType
- **0**: Sunday Morning Service
- **1**: Sunday Evening Service
- **2**: Midweek Service
- **3**: Special Event

## Development

### Prerequisites for Development
- **Visual Studio 2022** (Community or higher edition)
- **.NET 8 SDK** (latest version from dotnet.microsoft.com)
- **Git** for version control
- **SQL Server Management Studio (SSMS)** - optional but recommended
- **GitHub Desktop** or command-line Git

### Development Environment Setup

#### 1. Clone and Open Project
```bash
git clone https://github.com/Helmut-Essien/AccraRoadAttendance.git
cd AccraRoadAttendance
```

Open with Visual Studio or your preferred editor.

#### 2. Restore NuGet Packages
```bash
dotnet restore
```

Or in Visual Studio:
- **Tools** → **NuGet Package Manager** → **Package Manager Console**
- Run: `Update-Package`

#### 3. Configure Development Database

Create `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=AttendanceDb_Dev;Integrated Security=True;",
    "OnlineConnection": "Server=your-dev-server;Database=your-dev-db;User Id=dev-user;Password=dev-pass;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "AccraRoadAttendance": "Debug"
    }
  }
}
```

#### 4. Initialize Development Database
```bash
dotnet ef database drop -f
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Building the Project

#### Via Command Line
```bash
# Clean build
dotnet clean
dotnet restore
dotnet build

# Release build
dotnet build -c Release
```

#### Via Visual Studio
- **Build** → **Rebuild Solution** (Ctrl+Shift+B)

### Running the Application

#### Development Mode
```bash
dotnet run --launch-profile Development
```

#### Via Visual Studio
1. Press **F5** or click **Start** (▶)
2. Application launches with development configuration

### Code Style & Conventions

**Naming Conventions:**
```csharp
// Classes, methods, properties
public class MemberService { }
public void SaveMember() { }
public string MemberName { get; set; }

// Private fields
private readonly AttendanceDbContext _context;
private string _tempData;

// Constants
private const string DefaultRole = "User";
private const int MaxRetries = 3;

// Local variables
var memberList = GetMembers();
```

**Organization:**
- Use `#region` / `#endregion` for logical sections
- Methods should be focused on single responsibility
- Keep methods under 50 lines
- Keep lines under 120 characters

### Database Migrations

**Create Migration:**
```bash
dotnet ef migrations add AddNewFeatureFields
```

**Review Generated Migration:**
- Check the `Migrations/[timestamp]_AddNewFeatureFields.cs` file
- Ensure it matches your model changes

**Apply Migration:**
```bash
dotnet ef database update
```

**Revert Migration:**
```bash
dotnet ef database update [PreviousMigrationName]
```

**Remove Last Migration (not applied):**
```bash
dotnet ef migrations remove
```

**View All Migrations:**
```bash
dotnet ef migrations list
```

### Debugging

**Setting Breakpoints:**
1. Click in the code editor's left margin
2. A red circle appears
3. Run with F5
4. Execution pauses at the breakpoint

**Debug Windows (Visual Studio):**
- **Locals** (Alt+4) - View local variables
- **Watch** - Monitor specific expressions
- **Call Stack** (Ctrl+Alt+C) - View method hierarchy
- **Output** (Ctrl+Alt+O) - View debug messages
- **Immediate** (Ctrl+Alt+I) - Execute commands

**Logging for Debugging:**
```csharp
private readonly ILogger<MyService> _logger;

// Log different levels
_logger.LogInformation("Processing member: {memberId}", id);
_logger.LogWarning("Duplicate phone number: {phone}", phone);
_logger.LogError("Save failed: {ex}", exception);
_logger.LogDebug("Detailed debug info: {data}", debugInfo);
```

### Performance Tips

**Database Queries:**
```csharp
// Good - No tracking, projects only needed fields
var members = await _context.Members
    .AsNoTracking()
    .Where(m => m.IsActive)
    .Select(m => new { m.Id, m.FullName })
    .ToListAsync();

// Good - Includes related data to avoid N+1 queries
var members = await _context.Members
    .AsNoTracking()
    .Include(m => m.Attendances)
    .ToListAsync();

// Avoid - Loads full entities and all related data
var members = await _context.Members.ToListAsync();
```

**UI Performance:**
- Implement pagination for large lists
- Use async/await for long operations
- Cache frequently accessed data
- Minimize event handler complexity

### Git Workflow

**Feature Branch Workflow:**
```bash
# Create feature branch
git checkout -b feature/add-member-validation

# Make changes and commit
git add .
git commit -m "[Feature]: Add member phone validation"

# Push to remote
git push origin feature/add-member-validation

# Create Pull Request on GitHub
```

**Commit Message Format:**
```
[Type]: Description

Types:
- [Feature]: New functionality
- [Fix]: Bug fix
- [Docs]: Documentation update
- [Refactor]: Code refactoring
- [Test]: Test additions
- [Perf]: Performance improvement
```

**Common Git Commands:**
```bash
git status              # View changes
git diff                # View detailed changes
git stash               # Temporarily save changes
git log --oneline -10   # View commit history
git revert [hash]       # Undo commit
git merge [branch]      # Merge branches
```

### Publishing

**ClickOnce Deployment:**
1. **Right-click Project** → **Publish**
2. Select publish profile (ClickOnceProfile.pubxml or ClickOnceProfile2.pubxml)
3. Click **Publish**

**Manual Release Build:**
```bash
dotnet publish -c Release -o ./publish/v1.0.0
```

## Troubleshooting

### General Issues

#### Application Won't Start
**Problem:** Application crashes immediately

**Solutions:**
1. Verify .NET 8 installation:
   ```bash
   dotnet --version
   ```
   Should output `8.x.x` or higher

2. Check database connection:
   - Open `appsettings.json`
   - Verify connection string is correct
   - Test SQL Server connectivity

3. Run database migrations:
   ```bash
   dotnet ef database update
   ```

4. Check application logs:
   - Look in Windows Event Viewer
   - Review debug output in Visual Studio

#### Build Errors
**Problem:** Build fails with compilation errors

**Solutions:**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

If still failing:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
dotnet build
```

### Database Issues

#### Cannot Connect to Database
**Problem:** "Cannot connect to database" error

**Solutions:**
1. Verify SQL Server is running
2. Check connection string format
3. Test with SQL Server Management Studio (SSMS)
4. For LocalDB:
   ```bash
   sqllocaldb v
   sqllocaldb start mssqllocaldb
   ```

#### Migration Already Applied
**Problem:** "Migration X has already been applied"

**Solution:**
```bash
dotnet ef database update
```
The tool automatically skips applied migrations.

#### Foreign Key Constraint Error
**Problem:** "Foreign key constraint error" when saving

**Solutions:**
1. Ensure referenced records exist
2. Check cascading delete rules in migrations
3. Review data relationships in database schema

### Login Issues

#### Cannot Log In
**Problem:** "Invalid credentials" or authentication fails

**Solutions:**
1. Verify admin user exists:
   ```bash
   dotnet ef database update
   ```

2. Check user in database:
   - Open SQL Server Management Studio
   - Query `AspNetUsers` table
   - Verify email and password

3. Reset to default credentials:
   - Email: `admin@example.com`
   - Password: `Admin@123`

4. Check user roles:
   ```sql
   SELECT * FROM AspNetUserRoles WHERE UserId = '[admin-user-id]'
   ```

### Synchronization Issues

#### Data Won't Sync
**Problem:** Offline mode active, data not syncing

**Solutions:**
1. Check internet connection
2. Verify online connection string in `appsettings.json`
3. Ensure firewall allows database connection
4. Check online database is accessible
5. Review sync logs

#### Google Drive Upload Fails
**Problem:** Profile picture upload to Google Drive fails

**Solutions:**
1. Verify encrypted credentials in `Resources/service_key.enc`
2. Check Google Drive API is enabled
3. Verify folder ID is correct
4. Check Google Cloud credentials haven't expired

### Performance Issues

#### Application Running Slowly
**Problem:** UI lag or slow database queries

**Solutions:**
1. Check database query performance
2. Reduce page size for pagination
3. Use `.AsNoTracking()` for read-only queries
4. Profile with Visual Studio Diagnostic Tools
5. Close unnecessary applications

#### Database Locked
**Problem:** "Database is locked" error

**Solutions:**
```bash
# In Visual Studio Package Manager Console:
Update-Database
```

### UI/Theme Issues

#### Theme Not Changing
**Problem:** Dark/Light theme toggle doesn't work

**Solutions:**
1. Restart application
2. Clear application cache
3. Rebuild solution
4. Update Material Design NuGet package

#### UI Elements Not Rendering
**Problem:** Buttons, text, or controls appear broken

**Solutions:**
1. Try switching themes
2. Restart application
3. Rebuild solution

## Contributing

We welcome contributions! Please follow these guidelines:

### Before You Start
1. Check existing issues on GitHub
2. Fork the repository
3. Create a feature branch

### Development Process

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Changes**
   - Follow code style guidelines
   - Write clean, readable code
   - Add comments for complex logic
   - Test thoroughly

3. **Commit Changes**
   ```bash
   git add .
   git commit -m "[Feature]: Brief description of changes"
   ```

4. **Push to Remote**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Create Pull Request**
   - Go to GitHub repository
   - Click "New Pull Request"
   - Select your feature branch
   - Provide detailed description
   - Request reviewers

### Code Review Checklist

Before submitting a PR, verify:
- [ ] Code follows naming conventions
- [ ] No hardcoded values (use configuration)
- [ ] Comments explain complex logic
- [ ] Error handling implemented
- [ ] No `Console.WriteLine()` (use `ILogger`)
- [ ] Async/await properly used
- [ ] Database queries optimized
- [ ] UI is responsive
- [ ] Tests written (if applicable)
- [ ] No security vulnerabilities
- [ ] Migrations included (if schema changed)
- [ ] README updated (if needed)

### Types of Contributions

**Bug Fixes:**
- Reference the issue number
- Include steps to reproduce
- Add unit tests if possible

**Features:**
- Align with project vision
- Include documentation
- Add necessary tests
- Update README if user-facing

**Documentation:**
- Clear and concise
- Include examples
- Update as needed

**Performance Improvements:**
- Include benchmark results
- Explain optimization
- Ensure no functionality changes

## License

This project is licensed under the **MIT License** - see the `LICENSE` file for details.

### MIT License Summary
- ✓ You can use this commercially
- ✓ You can modify the code
- ✓ You can distribute the software
- ✓ You can use privately
- ✓ No liability or warranty provided

## Support

For issues, questions, or suggestions:

### Contact Information
**Church of Christ - Accra-Road Congregation**
- **Location**: Y0582, SONATA ST, Amanfro (behind JD Restaurant), Kasoa, Ghana
- **Phone**: 0244265642 / 0244161872
- **Email**: archurchofchrist@gmail.com
- **Digital Address**: GS-0686-7830
- **P.O. Box**: WU 554, Kasoa

### Support Channels
1. **GitHub Issues** - [Create an issue](https://github.com/Helmut-Essien/AccraRoadAttendance/issues)
2. **Email** - archurchofchrist@gmail.com
3. **In-Person** - Visit during service hours

### FAQ

**Q: Can I use this for a different church?**
A: Yes! The application is designed to be reusable. Customize branding and settings as needed.

**Q: How do I backup my data?**
A: Use SQL Server Management Studio or command-line backup tools.

**Q: Is there mobile app support?**
A: Currently desktop-only. Mobile support may be added in future versions.

**Q: How often should I sync data?**
A: Automatic sync occurs at startup. Manual sync available from MainWindow.

## Changelog

### Version 1.0.0 (Initial Release)
**Release Date:** December 2024

**Features:**
- ✓ Core attendance tracking
- ✓ Comprehensive member management
- ✓ 7 report types with PDF export
- ✓ Local and cloud data synchronization
- ✓ User authentication and roles
- ✓ Dark/Light theme support
- ✓ Google Drive integration
- ✓ Offline mode with sync
- ✓ Pagination and search

**Known Limitations:**
- Desktop application only
- Windows-only (no Mac/Linux)

---

**The Pillar and Foundation of the Truth** - 1 Timothy 3:15

© 2024 Church of Christ - Accra-Road Congregation. All rights reserved.

**Last Updated:** December 2024  
**Status:** ✓ Production Ready

