const { app, BrowserWindow, screen } = require('electron');
const path = require('node:path');
const { spawn } = require('node:child_process');

let cursorLockProcess;

function startCursorLock(display) {
  if (process.platform !== 'win32' || cursorLockProcess) return;

  const scriptPath = path.join(__dirname, 'cursor-lock.ps1');
  const { x, y, width, height } = display.bounds;
  const scaleFactor = display.scaleFactor || 1;

  // The helper uses the Windows ClipCursor API to confine the pointer
  // to the selected kiosk display. It releases the clip when Electron exits.
  cursorLockProcess = spawn(
    'powershell.exe',
    [
      '-NoLogo',
      '-NoProfile',
      '-NonInteractive',
      '-WindowStyle',
      'Hidden',
      '-ExecutionPolicy',
      'Bypass',
      '-File',
      scriptPath,
      '-X',
      String(Math.round(x * scaleFactor)),
      '-Y',
      String(Math.round(y * scaleFactor)),
      '-Width',
      String(Math.round(width * scaleFactor)),
      '-Height',
      String(Math.round(height * scaleFactor)),
      '-ParentPid',
      String(process.pid),
    ],
    { windowsHide: true, stdio: 'ignore' },
  );

  cursorLockProcess.on('error', () => {
    cursorLockProcess = undefined;
  });
  cursorLockProcess.on('exit', () => {
    cursorLockProcess = undefined;
  });
}

function stopCursorLock() {
  if (cursorLockProcess && !cursorLockProcess.killed) {
    cursorLockProcess.kill();
  }
  cursorLockProcess = undefined;
}

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

  if (process.platform === 'win32') {
    // Start after the kiosk window is created so the chosen display is known.
    window.once('ready-to-show', () => startCursorLock(display));
    window.on('closed', stopCursorLock);
  }
}

app.whenReady().then(() => {
  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('before-quit', stopCursorLock);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});
