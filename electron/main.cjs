const { app, BrowserWindow, screen } = require('electron');
const path = require('node:path');

function createWindow() {
  const displays = screen.getAllDisplays();
  const primaryDisplay = screen.getPrimaryDisplay();
  const display = displays.find((item) => item.id !== primaryDisplay.id) || primaryDisplay;
  const { x, y, width, height } = display.bounds;

  const window = new BrowserWindow({
    x,
    y,
    width,
    height,
    fullscreen: true,
    kiosk: true,
    autoHideMenuBar: true,
    backgroundColor: '#071b2b',
    webPreferences: {
      contextIsolation: true,
      nodeIntegration: false,
      devTools: false,
    },
  });

  window.loadFile(path.join(__dirname, '..', 'dist', 'index.html'));
}

app.whenReady().then(() => {
  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});
