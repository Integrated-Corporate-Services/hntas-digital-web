import { initAll } from 'govuk-frontend'
import accessibleAutocomplete from 'accessible-autocomplete';


// Initialize GOV.UK Frontend (Accordions, Tabs, etc.)
initAll();

// Expose the autocomplete to the global window object
window.accessibleAutocomplete = accessibleAutocomplete;
