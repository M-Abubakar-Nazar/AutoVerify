# Local IIS Deployment

These steps publish the `AutoVerifyMVC` ASP.NET Core MVC application to IIS on a Windows development machine.

## Current workstation status

The application has been published locally to `bin\IISPublish`, including its generated `web.config` and `wwwroot` assets. IIS and the ASP.NET Core Module are not installed on this workstation. Enabling IIS Windows features and installing the .NET Hosting Bundle require a Windows administrator. Microsoft's IIS guide confirms that the Hosting Bundle installs the ASP.NET Core Module and must be repaired if it was installed before IIS: [Host ASP.NET Core on Windows with IIS](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0).

## 1. Prepare Windows

1. Enable **Internet Information Services** in Windows Features. Include the Web Server role, Static Content, Default Document, Request Filtering, and IIS Management Console.
2. Install the **.NET 10 Hosting Bundle** after IIS is enabled. It installs the ASP.NET Core Module IIS needs to host the app. If IIS is installed after the bundle, run the Hosting Bundle installer again and choose Repair. See Microsoft's [IIS hosting guide](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0).

## 2. Configure SQL Server

The development connection string uses `(localdb)\MSSQLLocalDB`. LocalDB is tied to a Windows user profile, so an IIS application pool normally cannot use the developer's LocalDB instance. For IIS, use a local SQL Server or SQL Server Express instance instead.

1. Create `AutoVerifyDb` on the SQL Server instance.
2. Apply the migrations from the project directory, using the intended IIS database connection string:

   ```powershell
   dotnet ef database update
   ```

3. Grant the IIS application pool identity access to the database. For an app pool named `AutoVerifyMVC`, the Windows identity is `IIS APPPOOL\AutoVerifyMVC`. Give the runtime identity read/write access to the Identity, `CarListings`, and `CarInquiries` tables. Apply schema migrations as a database owner or deployment identity, not as the runtime identity.
4. Set a production connection string outside source control. One option is an `appsettings.Production.json` file in the publish directory with restricted filesystem permissions. Do not commit production credentials.

The app creates its initial Administrator only when both `BootstrapAdmin__Email` and `BootstrapAdmin__Password` are supplied as environment variables. Set them securely for the first startup, sign in, then remove the bootstrap variables. The Admin dashboard can create Inspector accounts. Public signup intentionally offers only Buyer and Seller roles.

Example for a local SQL Server Express instance using Windows authentication:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AutoVerifyDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Use SQL authentication only when required, and protect the secret with the machine's configuration and filesystem controls.

## 3. Publish the application

Run these commands from `D:\WebProjectFinal\AutoVerifyMVC`:

```powershell
dotnet restore
dotnet publish .\AutoVerifyMVC.csproj -c Release -o D:\Sites\AutoVerifyMVC
```

The publish output contains the generated `web.config` used by the ASP.NET Core Module. Keep that file in the site folder; the .NET SDK creates it during publish. See Microsoft's [IIS publish tutorial](https://learn.microsoft.com/aspnet/core/tutorials/publish-to-iis?view=aspnetcore-10.0).

## 4. Create the IIS site

1. In IIS Manager, create an application pool named `AutoVerifyMVC`.
2. Set **.NET CLR Version** to **No Managed Code**, **Managed pipeline mode** to **Integrated**, and leave **Enable 32-Bit Applications** disabled on a 64-bit machine.
3. Create a website with physical path `D:\Sites\AutoVerifyMVC`, assign the `AutoVerifyMVC` application pool, and bind it to an available local port such as `http://localhost:8085`.
4. Grant `IIS APPPOOL\AutoVerifyMVC` **Read & Execute** access to the published folder. Grant Modify only to folders the app must write to; this project does not currently write uploaded files.
5. Set the app pool's **Load User Profile** setting to `True` so ASP.NET Core Data Protection keys can persist for the pool identity across restarts.
6. Start the site and open its binding in a browser. Verify Signup, Login, all four dashboards, inspection requests, inspector claiming/report submission, listing status updates, and image URLs including `/assets/images/Default.png`.

For HTTPS, add an IIS HTTPS binding with a development certificate trusted by the local machine. Do not use the development certificate for a public deployment.

## 5. Operational notes

- The app seeds the four Identity roles and eight marketplace listings when it starts. Its database identity therefore needs read/write access to the relevant tables after migrations have been applied.
- Apply EF migrations with a deployment identity before starting the site. The current project includes the inspector assignment/report migration.
- Seeded marketplace listings are visible to buyers. Sellers can publish and manage only listings owned by their own account.
- Keep production secrets out of the repository, publish only the nested `AutoVerifyMVC` project, and review IIS logs and Windows Event Viewer if the site fails to start.
