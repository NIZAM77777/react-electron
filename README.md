# Maritime LED Display

This project is a 1:1 React/Vite display designed for a square 3×3 LED wall.

## Local development

```bash
npm install
npm run dev
```

## Build a portable Windows app

The project includes an Electron wrapper so the presentation laptop does not
need Node.js, Python, a browser, or a local server.

On the development laptop, after making your manual changes, run:

```bash
npm install
npm run electron:build
```

The portable `.exe` is created inside the `release` folder. Copy that output
to the presentation laptop and double-click the `.exe`. It opens fullscreen on
the non-primary display when an LED controller is connected.

To test the packaged window before creating the `.exe`:

```bash
npm run electron:dev
```

The Electron packaging files are in `electron/main.cjs`, and the Vite build is
configured with relative asset paths so the packaged app can load its images and
video locally.

---

# Original Vite notes

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.
