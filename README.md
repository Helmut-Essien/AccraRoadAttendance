# Accra Road Attendance

A comprehensive church attendance tracking and management application built with .NET 8, WPF, and integrated with Google Drive for cloud synchronization. This application streamlines church member registration, attendance tracking, and reporting.

## Overview

Accra Road Attendance is a Windows desktop application designed to manage church operations with a focus on:
- **Member Management**: Register and maintain detailed member profiles with comprehensive information.
- **Attendance Tracking**: Record attendance for various service types (Sunday Service, Midweek Prayer, Bible Study, Special Events).
- **Cloud Synchronization**: Automatic sync between local database and cloud storage via Google Drive.
- **Reporting & Analytics**: Generate attendance reports and summary statistics.
- **User Management**: Role-based access control with Admin and User roles.

## Technology Stack

- **Framework**: .NET 8
- **UI**: Windows Presentation Foundation (WPF)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Identity
- **Cloud Storage**: Google Drive API
- **ORM**: Entity Framework Core with DbContext

## Project Structure

```
AccraRoadAttendance/
??? Models/                              # Domain models and data entities
?   ??? Member.cs                        # Church member profile with comprehensive information
?   ??? Attendance.cs                    # Service attendance records
?   ??? User.cs                          # Identity user (extends ASP.NET Identity)
?   ??? ChurchAttendanceSummary.cs       # Aggregated attendance statistics
?
??? Services/                            # Business logic and external integrations
?   ??? GoogleDriveService.cs            # Google Drive API (upload/download profile pictures)
?   ??? SyncService.cs                   # Bidirectional data synchronization
?   ??? NavigationService.cs             # WPF page navigation service
?   ??? CurrentUserService.cs            # User session and authentication management
?
??? Data/                                # Database access layer
?   ??? AttendanceDbContext.cs           # Entity Framework DbContext configuration
?
??? Views/                               # User interface (XAML and code-behind)
?   ??? MainWindow.xaml(.cs)             # Main application shell and navigation hub
?   ??? Login.xaml(.cs)                  # User authentication page
?   ??? SplashScreen.xaml(.cs)           # Application startup splash screen
?   ?
?   ??? Pages/                           # Feature-specific pages
?       ??? Dashboard/
?       ?   ??? Dashboard.xaml(.cs)      # Church statistics and quick access overview
?       ?
?       ??? Members/                     # Member management module
?       ?   ??? Members.xaml(.cs)        # Member list and browsing
?       ?   ??? AddMembers.xaml(.cs)     # New member registration form
?       ?   ??? EditMembers.xaml(.cs)    # Member profile editing
?       ?   ??? MemberDetails.xaml(.cs)  # Detailed member profile view
?       ?
?       ??? Attendance/                  # Attendance tracking module
?       ?   ??? MarkAttendance.xaml(.cs) # Record service attendance
?       ?
?       ??? Reports/                     # Reporting and analytics module
?       ?   ??? ReportsPage.xaml(.cs)    # Generate and view reports
?       ?
?       ??? Users/                       # User management module (Admin only)
?           ??? UsersManagement.xaml(.cs)# Create and manage user accounts
?
??? Migrations/                          # Entity Framework migration history
?   ??? [timestamp]_InitialCreate.cs     # Initial database schema
?   ??? [timestamp]_AddSyncToDb.cs       # Sync status tracking columns
?   ??? [...]                            # Additional migrations
?
??? Resources/                           # Configuration and static resources
?   ??? service-account-key.json         # Google service account credentials
?
??? App.xaml(.cs)                        # Application entry point and DI configuration
??? [Project Files]
    ??? .csproj                          # Project configuration
    ??? App.config                       # Application settings
    ??? appsettings.json                 # Configuration file
```

### Folder Descriptions

| Folder | Purpose |
|--------|---------|
| **Models/** | Domain entities representing core business objects (Member, Attendance, etc.) |
| **Services/** | Business logic, external API integrations, and cross-cutting concerns |
| **Data/** | Entity Framework context and database access configuration |
| **Views/** | WPF XAML pages and code-behind for the user interface |
| **Migrations/** | Database schema version history managed by Entity Framework |
| **Resources/** | Configuration files, credentials, and static resources |

### Key Entry Points

- **App.xaml.cs**: Application startup, dependency injection configuration, database initialization
- **MainWindow.xaml.cs**: Main navigation hub after user authentication
- **Login.xaml.cs**: Initial login page for user authentication


## Core Models

### Member
Represents a church member with extensive profile information:
- **Basic Info**: First name, last name, other names
- **Contact**: Phone number, email
- **Personal**: Gender, date of birth, marital status, spouse information
- **Church**: Membership start date, role, baptism details, attendance records
- **Demographics**: Nationality, occupation, educational level, hometown
- **Family**: Parent and family member contacts
- **Status**: Active/inactive, sync status

**Key Properties**:
- `FullName`: Computed property combining name fields
- `AgeGroup`: Calculated from date of birth (Child, Teen, YoungAdult, Adult, Senior)
- `SyncStatus`: Tracks synchronization with cloud database

### Attendance
Records member attendance for specific services:
- **Service Date**: Date of the service
- **Service Type**: Sunday Service, Wednesday Prayer, Thursday Bible Study, Special Event
- **Status**: Present, Absent, Excused
- **Notes**: Optional notes about the attendance record
- **Tracking**: Record timestamp and sync status

### ChurchAttendanceSummary
Aggregated attendance statistics per service:
- **Totals**: Present count, male/female breakdown, total members, visitors, children
- **Offering**: Recorded offering amount
- **Service Theme**: Optional description of the service
- **Summary Date**: Date of the service
- **Sync Status**: Cloud synchronization status

### User
Identity user extending ASP.NET Identity:
- Linked to a member profile for role-based access
- Email-unique requirement for system-wide uniqueness

## Services

### GoogleDriveService
Handles Google Drive API interactions:
- **UploadImage()**: Uploads member profile pictures to Google Drive
  - Supports JPEG and PNG formats
  - Returns publicly accessible shareable URL
  - Auto-configures permissions for public access
- **DownloadImage()**: Downloads images from Google Drive to local storage
  - Caches images in the `ProfilePictures` folder

**Configuration**: Requires `service-account-key.json` in `Resources` folder with:
- Service account credentials
- Profile pictures folder ID configured in `_profilePicturesFolderId`

### SyncService
Bidirectional data synchronization between local and cloud databases:
- **SyncData()**: Main sync orchestrator
  - Pushes local changes to cloud
  - Pulls cloud updates to local database
  - Handles image synchronization via Google Drive
  - Tracks last sync time to optimize data transfer
- **PushLocalChanges()**: Uploads local modifications
- **PullOnlineChanges()**: Downloads remote updates

**Sync Strategy**:
- Based on `LastModified` timestamp comparison
- `SyncStatus` flag indicates synchronization state
- Profile pictures automatically uploaded/downloaded as needed

### NavigationService
WPF navigation abstraction:
- Manages page transitions within `MainWindow`
- Type-safe navigation to different views
- Dependency injection integrated

### CurrentUserService
User session management:
- Tracks currently logged-in user
- Role verification
- Logout functionality

## Database Schema

### Entity Framework Relationships
- **User ? Member**: One-to-one relationship
- **Member ? Attendance**: One-to-many relationship (cascade delete)
- **Identity Tables**: Configured for SQLite/SQL Server compatibility

### Key Indexes
- `Member.Email`: Unique index
- `Member.PhoneNumber`: Unique index
- `Attendance.ServiceDate`: Performance index
- `Attendance.ServiceDate + ServiceType`: Composite index
- `ChurchAttendanceSummary.SummaryDate`: Date-based index

### Database Configuration
- **Server**: SQL Server (FINSERVE\SQLEXPRESS)
- **Database Name**: AttendanceDb
- **ORM**: Entity Framework Core with POCO models
- **Migrations**: Database-first approach with migration files

## Authentication & Authorization

### Identity Setup
- ASP.NET Core Identity integration
- Role-based access control (RBAC)
- Unique email requirement per user
- Two roles: **Admin** and **User**

### Access Control
- **Admin Role**: Full access to all features including user management
- **User Role**: Standard member and attendance operations
- Protected pages check role membership before navigation

## Features

### Dashboard
- Overview of church statistics
- Quick access to main operations
- Summary views of recent activities

### Member Management
- **Add Members**: Register new church members with comprehensive profile data
- **Edit Members**: Update existing member information
- **Member Details**: View detailed member profiles
- **Member Search**: Find members by various criteria
- **Profile Pictures**: Upload member photos to Google Drive

### Attendance Management
- **Mark Attendance**: Record attendance for services
- **View Attendance**: Check attendance history
- **Multiple Service Types**: Support for different worship services
- **Status Tracking**: Present, Absent, Excused status options

### Reporting
- **Attendance Reports**: Generate reports by date range or service type
- **Summary Statistics**: Aggregate attendance data
- **Member Demographics**: Analyze member composition
- **Export Functionality**: Export reports for further analysis

### User Management (Admin Only)
- Create new user accounts
- Assign roles (Admin/User)
- Manage user permissions
- Delete user accounts

### Cloud Synchronization
- Automatic bidirectional sync
- Profile picture storage on Google Drive
- Conflict resolution based on last modified timestamp
- Sync status tracking

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (local or remote)
- Google Cloud Project with Drive API enabled
- Service account key JSON file

### Installation

1. **Clone the repository**

   git clone https://github.com/Helmut-Essien/AccraRoadAttendance.git
   cd AccraRoadAttendance


2. **Configure Database Connection**
- Update connection string in `App.xaml.cs`:

   services.AddDbContext<AttendanceDbContext>(options =>
       options.UseSqlServer("YOUR_CONNECTION_STRING"));


3. **Set Up Google Drive Credentials**
- Place `service-account-key.json` in `Resources` folder
- Update profile pictures folder ID in `GoogleDriveService.cs`:

   _profilePicturesFolderId = "YOUR_FOLDER_ID";


4. **Initialize Database**
- Run migrations automatically on startup, or manually:

   dotnet ef database update


5. **Build and Run**

   dotnet build
   dotnet run


### First Time Setup
- Application launches with login screen
- Create initial admin user through identity setup
- Assign Admin role to first user
- Login and start managing the church

## Configuration

### Database
Connection string in `App.xaml.cs`:

"Server=FINSERVE\\SQLEXPRESS;Database=AttendanceDb;Integrated Security=True;TrustServerCertificate=True;"


### Google Drive
1. Configure service account credentials in `GoogleDriveService` constructor
2. Set the profile pictures folder ID
3. Ensure service account has write permissions to the shared folder

### Application Settings
- Identity settings (email uniqueness, etc.) in `App.xaml.cs`
- Role seeding during database initialization
- View registration in dependency injection

## Enumerations

### Member Enums
- **Gender**: Male, Female
- **MaritalStatus**: Married, Single, Widowed, Divorced
- **OccupationType**: Self Employed, Salary Worker, Student, Unemployed, Apprentice, Retired
- **EducationalLevel**: No Formal Education, Primary School, Secondary School, Tertiary, Post Graduate

### Attendance Enums
- **ServiceType**: Sunday Service, Wednesday Prayer, Thursday Bible Study, Special Event
- **AttendanceStatus**: Present, Absent, Excused

## Development

### Adding New Features
1. Create model classes in `Models/`
2. Add DbSet to `AttendanceDbContext`
3. Create migration: `dotnet ef migrations add MigrationName`
4. Create service class if needed
5. Add view/page in `Views/`
6. Register in dependency injection container

### Database Migrations

// Create migration
dotnet ef migrations add InitialCreate

// Apply migrations
dotnet ef database update

// Rollback
dotnet ef database update PreviousMigration


### Testing Google Drive Integration
- Use built-in test button in MainWindow
- Select images to upload/download
- Verify URLs and local storage

## Architecture Decisions

### Cloud-First Synchronization
- Supports offline-first mobile sync scenarios
- Profile pictures stored in cloud for accessibility
- Timestamp-based conflict resolution

### Role-Based Security
- Admin controls user management
- Standard users limited to data entry
- Identity integration for future OAuth support

### Entity Framework + Identity
- Type-safe database queries
- Built-in migration system
- Standard ASP.NET authentication framework

## Known Limitations

- Sync service tracks last sync time in memory (consider persistent storage)
- Image downloading stores files locally (consider memory streaming)
- No built-in image compression before cloud upload
- Attendance reporting limited to UI views (consider API/export features)

## Future Enhancements

- [ ] Mobile app companion (Xamarin/MAUI)
- [ ] Web API for third-party integrations
- [ ] Advanced reporting with charts/analytics
- [ ] Automated backup and disaster recovery
- [ ] Payment/giving tracking module
- [ ] SMS/Email notifications
- [ ] Barcode/QR code attendance
- [ ] Sermon archive and notes
- [ ] Member small groups management
- [ ] Event calendar integration

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Contact the development team
- Check documentation in this README

## Authors

- **Helmut Essien** - Initial development
- See GitHub repository for full contributor list

## Acknowledgments

- Google Drive API documentation
- Microsoft .NET and Entity Framework teams
- WPF community resources
- Church management software best practices


