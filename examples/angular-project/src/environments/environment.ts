/**
 * Production-baseline environment. `ng build` uses this file; the development build swaps in
 * environment.development.ts via the fileReplacements in angular.json.
 */
export const environment = {
  production: true,
  /** Base path for the Task API. Relative so a reverse proxy owns the host in production. */
  apiUrl: '/api',
};
