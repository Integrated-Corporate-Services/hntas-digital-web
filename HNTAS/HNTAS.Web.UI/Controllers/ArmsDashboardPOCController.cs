using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HNTAS.Web.UI.Controllers
{
    public class ArmsDashboardPOCController : Controller
    {
        public IActionResult Index(string? searchTerm, int? month, int? year)
        {
            ModelState.Clear();

            // 1. Year defaults to current, but Month is now null (empty) by default
            int filterYear = year ?? DateTime.Now.Year;

            var networks = GetMockNetworks();

            // 2. Filter by Search Term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                networks = networks.Where(n =>
                    n.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    n.Hnid.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // 3. Updated Period Filtering
            // Always filter by Year. Only filter by Month if one is selected.
            networks = networks.Where(n => n.ReportingPeriod.Year == filterYear).ToList();

            if (month.HasValue)
            {
                networks = networks.Where(n => n.ReportingPeriod.Month == month.Value).ToList();
            }

            // 4. Map to ViewModel
            var viewModel = new NetworkListViewModel
            {
                SearchTerm = searchTerm,
                SelectedMonth = month, // Pass the null value back to the view
                SelectedYear = filterYear,
                Networks = networks.Select(n => new HeatNetworkRowViewModel
                {
                    Hnid = n.Hnid,
                    Name = n.Name,
                    Provider = n.Provider,
                    LastPostDate = n.ReportingPeriod
                }).ToList()
            };

            return View(viewModel);
        }


        public IActionResult Details(string hnid, int? month, int? year, List<string> statusFilter, int page = 1)
        {
            // 1. Logic for safety/defaults
            int filterYear = year ?? DateTime.Now.Year;
            int filterMonth = month ?? DateTime.Now.Month;

            var networkInfo = GetMockNetworks().FirstOrDefault(n => n.Hnid == hnid);
            if (networkInfo == null) return NotFound();

            // 2. Fetch and filter raw data
            var rawKpis = GetMockKpiRows(hnid)
                .Where(k => k.ReportingPeriod.Month == filterMonth && k.ReportingPeriod.Year == filterYear);

            // 3. Apply Status Filters
            if (statusFilter != null && statusFilter.Any())
            {
                rawKpis = rawKpis.Where(k => statusFilter.Contains(k.Status.ToString()));
            }

            // 4. Group all results first to get the total count for pagination
            var allGrouped = rawKpis.GroupBy(k => k.ElementId)
                                    .OrderBy(g => g.Key) // Ensure consistent order for paging
                                    .ToDictionary(g => g.Key, g => g.ToList());

            // 5. Pagination Math
            int pageSize = 10;
            int totalElements = allGrouped.Count;
            int totalPages = (int)Math.Ceiling(totalElements / (double)pageSize);

            // Slice the dictionary for the current page
            var pagedGrouped = allGrouped
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToDictionary(g => g.Key, g => g.Value);

            // 6. Build ViewModel
            var viewModel = new ArmsDetailsViewModel
            {
                Hnid = hnid,
                NetworkName = networkInfo.Name,
                SelectedMonth = filterMonth,
                SelectedYear = filterYear,
                StatusFilter = statusFilter ?? new List<string>(),
                GroupedElements = pagedGrouped, // Only passing the 10 for this page

                // Paging metadata
                CurrentPage = page,
                TotalPages = totalPages,
                TotalElements = totalElements,
                PageSize = pageSize,

                BackToListUrl = Url.Action("Index", new { month, year })
            };

            return View(viewModel);
        }


        // Inside your Controller or a Data Service
        private List<HeatNetworkStaticData> GetMockNetworks()
        {
            return new List<HeatNetworkStaticData>
            {
                new() { Hnid = "HN400219", Name = "Birmingham District Energy", Provider = "Cofely", ReportingPeriod = new DateTime(2026, 4, 1) },
                new() { Hnid = "HN600842", Name = "Greenwich Peninsula", Provider = "Pinnacle Power", ReportingPeriod = new DateTime(2026, 4, 1) },
                new() { Hnid = "HN100335", Name = "Pimlico District Heating", Provider = "Westminster Council", ReportingPeriod = new DateTime(2026, 4, 1) },
                new() { Hnid = "HN700111", Name = "Elephant & Castle", Provider = "Pinnacle Power", ReportingPeriod = new DateTime(2026, 3, 1) }, // Older data
                new() { Hnid = "HN200555", Name = "Leicester District Energy", Provider = "Engie", ReportingPeriod = new DateTime(2026, 4, 1) },
                new() { Hnid = "HN300999", Name = "Olympic Park Network", Provider = "East London Energy", ReportingPeriod = new DateTime(2026, 2, 1) } // Older data
            };
        }


        private List<KpiRowViewModel> GetMockKpiRows(string hnid)
        {
            var kpis = new List<KpiRowViewModel>();

            // Simulate data for various reporting periods
            for (int year = 2024; year <= 2026; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    // Stop at April 2026
                    if (year == 2026 && month > 4) break;

                    // Generate elements for this specific month/year post
                    for (int i = 1; i <= 100; i++)
                    {
                        // Incorporate hnid into Element ID to show data switching works
                        string elementId = $"{i:D5}";

                        // 1. Primary KPI (Pass)
                        kpis.Add(new KpiRowViewModel
                        {
                            KpiId = "EC-KPI-01",
                            ElementId = elementId,
                            Value = 95.0 + (i % 5),
                            Threshold = 95,
                            Status = KPIAssessmentStatus.Pass,
                            ReportingPeriod = new DateTime(year, month, 1)
                        });

                        // 2. Secondary KPI (Fail) - Every 5th element
                        if (i % 5 == 0)
                        {
                            kpis.Add(new KpiRowViewModel
                            {
                                KpiId = "EC-KPI-02",
                                ElementId = elementId,
                                Value = 82.0,
                                Threshold = 90,
                                LowerLimit = 0,
                                UpperLimit = 100,
                                Status = KPIAssessmentStatus.Fail,
                                ReportingPeriod = new DateTime(year, month, 1)
                            });
                        }

                        // 3. Alert KPI (Outside Limit) - Every 10th element
                        if (i % 10 == 0)
                        {
                            kpis.Add(new KpiRowViewModel
                            {
                                KpiId = "DD-KPI-02",
                                ElementId = elementId,
                                Value = 110.0,
                                Threshold = 80,
                                LowerLimit = 0,
                                UpperLimit = 100,
                                Status = KPIAssessmentStatus.OutsideLimit,
                                ReportingPeriod = new DateTime(year, month, 1)
                            });
                        }
                    }
                }
            }
            return kpis;
        }


        public class NetworkListViewModel
        {
            public string? SearchTerm { get; set; } = string.Empty;
            public int? SelectedMonth { get; set; }
            public int SelectedYear { get; set; }
            public List<HeatNetworkRowViewModel> Networks { get; set; } = new();

            // Dropdown helpers
            public List<int> AvailableYears => Enumerable.Range(2022, (DateTime.Now.Year - 2022) + 1).OrderByDescending(y => y).ToList();
            public Dictionary<int, string> Months => new()
            {
                { 1, "January" }, { 2, "February" }, { 3, "March" }, { 4, "April" },
                { 5, "May" }, { 6, "June" }, { 7, "July" }, { 8, "August" },
                { 9, "September" }, { 10, "October" }, { 11, "November" }, { 12, "December" }
            };
        }

        public class ArmsDetailsViewModel
        {
            // Network Info
            public string Hnid { get; set; } = string.Empty;
            public string NetworkName { get; set; } = string.Empty;

            // Selection Context
            public int SelectedMonth { get; set; }
            public int SelectedYear { get; set; }
            public List<string> StatusFilter { get; set; } = new();

            // The Data: Grouped by Element ID
            public Dictionary<string, List<KpiRowViewModel>> GroupedElements { get; set; } = new();

            // Helper for the "Back" link
            public string BackToListUrl { get; set; } = string.Empty;


            // Pagination Properties
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int TotalElements { get; set; }
            public int PageSize { get; set; } = 10; // Default to 10 per page

            public int FromRecord => ((CurrentPage - 1) * PageSize) + 1;
            public int ToRecord => Math.Min(CurrentPage * PageSize, TotalElements);

            public bool HasPreviousPage => CurrentPage > 1;
            public bool HasNextPage => CurrentPage < TotalPages;
        }

        public class HeatNetworkRowViewModel
        {
            public string Hnid { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Provider { get; set; } = string.Empty;
            public DateTime LastPostDate { get; set; } // The "Reporting Period" column
        }

        public class HeatNetworkStaticData
        {
            public string Hnid { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Provider { get; set; } = string.Empty;

            public DateTime ReportingPeriod { get; set; }

            // Helper property for the sidebar display
            public string DisplayName => $"{Hnid} ({Name})";
        }

        public class ArmsDashboardViewModel
        {
            // Core Identification
            public string Hnid { get; set; } = string.Empty;
            public string NetworkName { get; set; } = string.Empty;
            public string ArmsProvider { get; set; } = string.Empty;

            // Side Panel Search & Navigation
            public string SearchTerm { get; set; } = string.Empty;
            public List<HeatNetworkStaticData> AllNetworks { get; set; } = new();

            // Period Filtering (Monthly Database Posts)
            public int SelectedMonth { get; set; }
            public int SelectedYear { get; set; }

            public List<int> AvailableYears => Enumerable.Range(2022, (DateTime.Now.Year - 2022) + 1)
                                                         .OrderByDescending(y => y).ToList();

            public Dictionary<int, string> Months => new()
            {
                { 1, "January" }, { 2, "February" }, { 3, "March" }, { 4, "April" },
                { 5, "May" }, { 6, "June" }, { 7, "July" }, { 8, "August" },
                { 9, "September" }, { 10, "October" }, { 11, "November" }, { 12, "December" }
            };

            // KPI Status Filtering
            public List<string> StatusFilter { get; set; } = new();

            // Main Data: Grouped by Element ID
            public Dictionary<string, List<KpiRowViewModel>> GroupedElements { get; set; } = new();

            // Pagination & Counters
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int TotalElements { get; set; } // Added back
            public int FromRecord { get; set; }
            public int ToRecord { get; set; }

            public bool HasPreviousPage => CurrentPage > 1;
            public bool HasNextPage => CurrentPage < TotalPages;
        }

        public class KpiRowViewModel
        {

            public string KpiId { get; set; }
            public double Value { get; set; }

            public string ElementId { get; set; }
            public double Threshold { get; set; }
            public double LowerLimit { get; set; }
            public double UpperLimit { get; set; }
            public KPIAssessmentStatus Status { get; set; }
            public DateTime ReportingPeriod { get; internal set; }
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum KPIAssessmentStatus
        {
            [Description("Undefined")]
            /// <summary>
            /// when the rule has not been applied to the value, or if the value is missing/invalid and cannot be assessed.
            /// </summary>
            Undefined = 0,

            [Description("Pass")]
            /// <summary>
            /// Value is within limits and meets/exceeds the target threshold.
            /// </summary>
            Pass = 1,

            [Description("Fail")]
            /// <summary>
            /// Value is within limits but does not meet the target threshold.
            /// </summary>
            Fail = 2,

            [Description("Outside Limit")]
            /// <summary>
            /// Value is physically or logically outside the allowed Upper/Lower bounds.
            /// </summary>
            OutsideLimit = 3
        }
    }
}
