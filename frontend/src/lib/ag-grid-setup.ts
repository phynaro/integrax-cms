import {
  AllCommunityModule,
  ModuleRegistry,
  provideGlobalGridOptions,
} from 'ag-grid-community';

import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';

ModuleRegistry.registerModules([AllCommunityModule]);

provideGlobalGridOptions({
  theme: 'legacy',
});
