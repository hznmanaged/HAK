# Horizon Appointment Keeper
A timer application that displays the amount of time left in the current appointment from a Graph calendar.

# Setup
On first launch, the application will prompt for a Microsoft account. 
Log in to the account that has access to the calendar that you wisht to use.
The first calendar returned by Graph is selected by default.
To change the calendar, right-click the tray icon and click settings, then change the calendar to use in that dialog.

# Customization
Colors and sounds can be customized via the settings screen.
To access the settings screen, right-click the tray icon and click settings.

# Compiling
Open in Visual Studio and build.
You will need to provide a graph client ID in the Constants.cs file before compiling.
Uses DevExpress GUI control, you'll need to demo or license those as well.