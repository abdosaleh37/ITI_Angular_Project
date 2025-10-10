# Frontend - Angular App

This folder contains the Angular single-page application.

Key files and folders

- `package.json` - npm scripts and dependencies
- `angular.json` - Angular CLI configuration
- `src/` - source files
  - `src/main.ts` - app bootstrap
  - `src/index.html` - main HTML
  - `src/styles.css` - global styles
  - `src/app` - app code (components, pages, services)
  - `src/app/app.config.ts` - runtime configuration (API base URL, etc.)

Running locally (PowerShell)

1. Install dependencies:

   cd Frontend
   npm install

2. Start development server:

   npm run start

This will start the Angular dev server (usually at http://localhost:4200). Open the browser and navigate there.

Connecting to the backend

- Ensure the API base URL in `src/app/app.config.ts` (or environment files) points to the running backend API (for example `https://localhost:5001`).
- If you hit CORS issues, ensure the backend API is configured to allow requests from the frontend origin.

Testing and building

- Run unit tests (if any): `npm run test`
- Build production bundle: `npm run build`

Notes

- The project uses standard Angular CLI structure. If you need help adding routes, components or services, I can add examples or scaffolding.
# Lab4

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 20.3.2.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
