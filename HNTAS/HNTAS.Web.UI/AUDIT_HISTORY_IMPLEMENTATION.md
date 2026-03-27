# Audit History Sorting and Pagination Implementation Guide

## Overview
This document outlines the implementation of sorting (for Entry Type and Element columns) and pagination for the Audit History POC page.

## Files Modified/Created

### 1. View: `HNTAS.Web.UI/Views/AuditHistoryPOC/Index.cshtml`
**Status:** ? Updated

**Changes:**
- Added helper functions in the `@{}` block to generate sort URLs and icons
- Added ViewBag variables for pagination and sorting state
- Made "Entry type" and "Element" column headers clickable with sort indicators (?/?)
- Added item count display (e.g., "Showing 1 to 20 of 150 entries")
- Added null check for Model.Items with empty state message
- Added GDS-compliant pagination controls with Previous/Next buttons and page numbers

### 2. Service Interface: `HNTAS.Web.UI/Services/Core/IAuditLogService.cs`
**Status:** ? Created

```csharp
public interface IAuditLogService
{
    Task<AuditLogResponse> GetAuditLogsAsync(
        string? sortBy = null,
        string? sortOrder = "asc",
        int page = 1,
        int pageSize = 20);
}
```

### 3. Service Implementation: `HNTAS.Web.UI/Services/Core/AuditLogService.cs`
**Status:** ? Created

**Features:**
- Calls the Audit Logs API with sorting and pagination parameters
- Proper error handling and logging
- Returns empty response on errors

### 4. Controller: `HNTAS.Web.UI/Controllers/AuditHistoryPOCController.cs`
**Status:** ? Updated

**Changes:**
- Added query parameters: `sortBy`, `sortOrder`, `page`, `pageSize`
- Input validation:
  - Page must be >= 1
  - Page size between 1-100
  - Sort order must be "asc" or "desc"
  - Sort field limited to "EntryType" or "Element"
- Sets ViewBag properties for the view
- Improved error handling

## Required Service Registration

Add the following to your `Program.cs` file in the service registration section:

```csharp
// Register Audit Log Service
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
```

This should be added near other service registrations like:
```csharp
builder.Services.AddScoped<IHeatNetworkService, HeatNetworkService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>(); // Add this line
```

## API Requirements

The implementation assumes the API endpoint supports the following query parameters:

### Endpoint
`GET /api/AuditLogs`

### Query Parameters
- `sortBy` (string, optional): Field to sort by ("EntryType" or "Element")
- `sortOrder` (string, optional): Sort direction ("asc" or "desc")
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 20, max: 100)

### Response Model (AuditLogResponse)
```csharp
public class AuditLogResponse
{
    public List<AuditLog>? Items { get; set; }
    public int? TotalCount { get; set; }
    public int? TotalPages { get; set; }
}
```

### API Client Update
If the API client interface doesn't include these parameters, update the `IAuditLogsApi` interface:

```csharp
Task<IApiResponse<AuditLogResponse>> ApiAuditLogsGetAsync(
    string? sortBy = null,
    string? sortOrder = null,
    int? page = null,
    int? pageSize = null,
    CancellationToken cancellationToken = default);
```

## URL Examples

### Basic Page Load
```
/AuditHistoryPOC
```

### Sorted by Entry Type (Ascending)
```
/AuditHistoryPOC?sortBy=EntryType&sortOrder=asc
```

### Sorted by Element (Descending)
```
/AuditHistoryPOC?sortBy=Element&sortOrder=desc
```

### Page 2 with 50 items per page
```
/AuditHistoryPOC?page=2&pageSize=50
```

### Combined: Sort by Entry Type, Page 3
```
/AuditHistoryPOC?sortBy=EntryType&sortOrder=asc&page=3&pageSize=20
```

## UI Features

### Sorting
- Click on "Entry type" or "Element" column headers to sort
- First click: Sort ascending (?)
- Second click: Sort descending (?)
- Sort indicator shows current sort column and direction
- Sorting is maintained across pagination

### Pagination
- Shows current range (e.g., "Showing 1 to 20 of 150 entries")
- Previous/Next buttons for navigation
- Page numbers with smart ellipsis (e.g., 1 ... 5 6 7 ... 20)
- Shows up to 5 page numbers at a time
- Current page highlighted
- GDS (Government Design System) compliant styling
- Maintains sort order when navigating pages

### Empty State
- Shows "No certification history found." when no data available
- Graceful error handling with user-friendly messages

## Testing Checklist

- [ ] Service registered in DI container
- [ ] Clicking "Entry type" header sorts correctly
- [ ] Clicking "Element" header sorts correctly
- [ ] Sort direction toggles between ascending/descending
- [ ] Sort indicator (?/?) displays correctly
- [ ] Pagination displays when more than 1 page exists
- [ ] Previous button disabled on first page
- [ ] Next button disabled on last page
- [ ] Page numbers display correctly
- [ ] Clicking page numbers navigates correctly
- [ ] Item count displays correctly
- [ ] Empty state shows when no data
- [ ] Error handling works (shows user-friendly message)
- [ ] URL parameters persist correctly
- [ ] Sorting is maintained across page navigation
- [ ] Page size limits are enforced (1-100)

## Known Limitations

1. **Server-Side Sorting Required**: The API must support sorting. Client-side sorting is not implemented.

2. **Page Size**: Maximum page size is 100 to prevent performance issues.

3. **Sort Fields**: Only "EntryType" and "Element" are sortable. Other columns are display-only.

4. **API Dependencies**: The implementation requires the API to:
   - Accept sort and pagination parameters
   - Return total count and total pages
   - Perform the actual sorting and pagination server-side

## Troubleshooting

### Issue: Pagination doesn't appear
**Solution:** Check that `ViewBag.TotalPages` is > 1 and being set correctly in the controller.

### Issue: Sorting doesn't work
**Possible causes:**
1. API doesn't support sortBy/sortOrder parameters
2. API client interface doesn't include these parameters
3. Service not registered in DI container

### Issue: "No certification history found" always shows
**Possible causes:**
1. `Model.Items` is null - check API response
2. API is returning empty list
3. Error in service layer - check logs

### Issue: Sort indicator not showing
**Solution:** Verify `ViewBag.CurrentSort` and `ViewBag.CurrentOrder` are being set in the controller.

## Future Enhancements

1. **Add sorting to more columns** (Phase, Stage, Date)
2. **Add filtering capabilities** (by date range, entry type, etc.)
3. **Add export functionality** (CSV, Excel)
4. **Add search/filter input box**
5. **Remember user preferences** (page size, sort order) in session/cookie
6. **Add "Show all" option** for page size
7. **Implement client-side sorting** as fallback if API doesn't support it

## Additional Notes

- All pagination and sorting is server-side for better performance with large datasets
- The implementation follows GDS design patterns for accessibility
- URLs are shareable - users can bookmark or share links with specific page/sort settings
- The page size default is 20 but can be adjusted via query parameter
