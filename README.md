# Accra Road Church Attendance Management System

## Overview

**Accra Road Attendance** is a desktop application designed to manage church member attendance records, member information, and generate attendance reports. Built with **.NET 8**, **WPF**, and **Entity Framework Core**, the system supports both local and cloud-based data synchronization.

## Features

### Core Functionality

- **Member Management**: Add, edit, and manage comprehensive member profiles with detailed information including:
  - Personal details (name, contact, date of birth, gender)
  - Membership information (status, start date, baptism details)
  - Family information (next of kin, family members in church)
  - Educational and occupational background
  - Profile pictures with cloud storage support

- **Attendance Tracking**: Record and manage attendance for multiple service types:
  - Sunday Service
  - Wednesday Prayer
  - Thursday Bible Study
  - Special Events

- **Dashboard**: Visual overview of attendance statistics and church metrics

- **Reports**: Generate comprehensive attendance reports and summaries

- **User Management**: Admin controls for user access and roles
  - Admin role with elevated permissions
  - User role with standard access

- **Data Synchronization**: Bidirectional sync between local and cloud databases
  - Automatic conflict resolution
  - Google Drive integration for profile picture storage
  - Retry mechanism with configurable attempts

- **Theme Support**: Light/Dark theme toggle for improved user experience

### Technical Capabilities

- **Dual Database Architecture**: 
  - Local SQL Server database for offline work
  - Online SQL Server database for cloud synchronization
  
- **Google Drive Integration**: Automatic backup and cloud storage of profile pictures

- **Role-Based Access Control**: ASP.NET Core Identity integration with role management

- **Data Validation**: Comprehensive client-side and server-side validation

## Technology Stack

- **Framework**: .NET 8
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Database**: SQL Server (local & online)
- **ORM**: Entity Framework Core 9.0.1
- **Authentication**: ASP.NET Core Identity
- **Cloud Storage**: Google Drive API
- **UI Components**: Material Design In XAML (MaterialDesignInXamlToolkit)
- **ID Generation**: ULID (.NUlid)

## Prerequisites

### System Requirements

- Windows 10 or later
- .NET 8 Runtime
- SQL Server 2016 or later (local)
- Internet connection (for cloud sync and Google Drive features)

### Credentials & Configuration

- **Default Admin Credentials**: 
  - Email: `admin@example.com`
  - Password: `Admin@123`

- **Google Drive Setup**:
  - Encrypted service key stored as embedded resource
  - Passphrase: Configured in `GoogleDriveService`

## Installation

### Build from Source

1. Clone the repository:
```bash
git clone https://github.com/Helmut-Essien/AccraRoadAttendance
cd AccraRoadCoC
```

2. Configure connection strings in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AccraRoadAttendanceDb;Trusted_Connection=true;",
    "OnlineConnection": "Server=your_server;Database=AccraRoadAttendanceOnline;User Id=sa;Password=your_password;"
  }
}
```

3. Build the solution:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

## Project Structure

```
AccraRoadAttendance/
??? Models/
?   ??? Member.cs                 # Member entity with comprehensive properties
?   ??? User.cs                   # ASP.NET Core Identity user
?   ??? Attendance.cs             # Attendance records
?   ??? ChurchAttendanceSummary.cs # Summarized attendance data
?   ??? SyncMetadata.cs           # Sync state tracking
??? Data/
?   ??? AttendanceDbContext.cs    # Local database context
?   ??? OnlineAttendanceDbContext.cs # Cloud database context
?   ??? DesignTimeDbContextFactory.cs
??? Services/
?   ??? INavigationService.cs     # Navigation between pages
?   ??? CurrentUserService.cs     # User authentication state
?   ??? SyncService.cs            # Bidirectional data synchronization
?   ??? GoogleDriveService.cs     # Google Drive integration
??? Views/
?   ??? MainWindow.xaml           # Main application window
?   ??? Login.xaml                # Login interface
?   ??? Pages/
?       ??? Dashboard/            # Dashboard overview
?       ??? Members/              # Member management pages
?       ??? Attendance/           # Attendance marking
?       ??? Reports/              # Report generation
?       ??? Users/                # User administration
??? Migrations/
?   ??? AttendanceDb/             # EF Core migrations
??? appsettings.json              # Configuration file
```

## Usage

### Adding a Member

1. Log in with admin or user credentials
2. Navigate to **Members** section
3. Click **Add Member**
4. Fill in required fields marked with asterisks
5. Upload a profile picture (optional)
6. Click **Save**

### Recording Attendance

1. Navigate to **Mark Attendance**
2. Select the service type and date
3. Mark members as Present, Absent, or Excused
4. Save attendance records

### Syncing Data

1. Click the **Sync** button in the main window
2. Wait for synchronization to complete
3. System will:
   - Push local changes to cloud
   - Pull updates from cloud database
   - Update Google Drive with profile pictures
   - Track last sync time

### Generating Reports

1. Navigate to **Reports**
2. Select date range and filters
3. View or export attendance statistics

## Database Schema

### Key Tables

- **Members**: Core member information with 30+ properties
- **Attendances**: Service attendance records with sync status tracking
- **ChurchAttendanceSummaries**: Aggregated statistics by service and date
- **AspNetUsers**: Identity users with role-based access
- **SyncMetadata**: Tracks last synchronization time

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "connection_string_to_local_db",
    "OnlineConnection": "connection_string_to_cloud_db"
  }
}
```

### Supported Service Types

- Sunday Service
- Wednesday Prayer
- Thursday Bible Study
- Special Event

### Member Fields

**Personal Info**: First Name, Last Name, Other Names, Gender, Date of Birth, Email, Phone

**Membership**: Status, Start Date, Baptism Date/Place, Education Level, Nationality

**Contact**: Address, Location, Hometown

**Family**: Next of Kin, Family Members in Church, Parents, Spouse (if married)

**Professional**: Occupation Type, Occupation Description, Skills, Church Role

## Sync Architecture

The application uses a **Conflict-Free Synchronization** model:

1. **Push Phase**: Local changes newer than last sync time are pushed to cloud
2. **Pull Phase**: Cloud changes newer than last sync time are pulled locally
3. **Image Handling**: Profile pictures are uploaded to Google Drive and stored as URLs
4. **Retry Logic**: Failed syncs retry up to 2 times with configurable delays
5. **Metadata Tracking**: Last sync time persists across sessions

## Security

- Password hashing via ASP.NET Core Identity
- Role-based access control (Admin/User)
- Encrypted Google Drive service credentials
- Unique constraints on email and phone number
- User session management

## Logging

Application logs are configured via Microsoft Extensions Logging:
- Console output in debug mode
- File logging (if configured)
- Sync operations logged for troubleshooting

## Troubleshooting

### Database Issues

- Ensure SQL Server is running and accessible
- Verify connection strings in `appsettings.json`
- Run migrations: `dotnet ef database update`

### Sync Failures

- Check internet connectivity
- Verify Google Drive service credentials
- Review logs for detailed error messages
- Ensure adequate storage on Google Drive

### Login Issues

- Clear application cache
- Verify admin credentials in database
- Check user role assignments

## Development

### Required Tools

- Visual Studio 2022 or VS Code
- .NET 8 SDK
- SQL Server (2016+) or SQL Server Express

### Running Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName -p AccraRoadAttendance

# Apply migrations
dotnet ef database update
```

## Contributing

Contributions are welcome. Please ensure:
- Code follows project conventions
- Changes include appropriate validation
- Database changes include migrations
- Code is tested before submission

## License

Internal use only - All rights reserved

## Contact & Support

For issues, suggestions, or support:
- Repository: https://github.com/Helmut-Essien/AccraRoadAttendance
- Branch: Deploy

## Changelog

### Version 1.0
- Initial release with core attendance management features
- Google Drive integration for profile pictures
- Bidirectional data synchronization
- Role-based access control
- Material Design UI
