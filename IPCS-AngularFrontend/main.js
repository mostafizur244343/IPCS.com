/**
 * Electron Main Process File
 * This file controls the main window of the desktop application and system-level tasks.
 */
const { app, BrowserWindow } = require('electron');
const path = require('path');

let mainWindow;

function createWindow() {
  // Creating a new browser window
  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    webPreferences: {
      nodeIntegration: true, // Allows use of Node.js features in the frontend
      contextIsolation: false,
    },
    title: "PharmaCare Inventory System",
    autoHideMenuBar: true // Hides the menu bar for a premium desktop look
  });

  /**
   * Logic to load the app:
   * In development mode: loads from localhost:4200
   * In production mode: loads from the built index.html file
   */
  const startUrl = process.env.ELECTRON_START_URL || `file://${path.join(__dirname, 'dist/ipcs-angularfrontend/browser/index.html')}`;
  
  mainWindow.loadURL(startUrl);

  // Clear memory when window is closed
  mainWindow.on('closed', function () {
    mainWindow = null;
  });
}

// Open window when Electron is ready
app.on('ready', createWindow);

// Quit app when all windows are closed (except on Mac)
app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', function () {
  if (mainWindow === null) {
    createWindow();
  }
});
