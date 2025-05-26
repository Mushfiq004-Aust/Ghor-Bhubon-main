1. Download Microsoft Visual Studio.
2. Donwload DOTNET 8.0 version.
3. Download SQL Server Management Studio 
4. Download SQL Express.
5. 
   i. Open SQL server managmenet studio and set a server name such as "your_device_name\SQLEXPRESS".
   ii. Encryption: Mandatory
   iii. Trust server certificate: on.

6. Open the project solution and go to appsettings.json.
7. Change the server name in the 'DefaultCOnnection' same as the server name you have set in the SQL Server managment Studio.
8. Tools --> NuGet Package Manager --> Package Manager Console.
9. In ther console, type "update-database"
10. Database has been updated in your SQL server managment studio named as 'HomeRentalDB'
11. Run the project and you're good to go!
