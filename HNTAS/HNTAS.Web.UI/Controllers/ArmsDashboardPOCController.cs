using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HNTAS.Web.UI.Controllers
{
    public class ArmsDashboardPOCController : Controller
    {
        public IActionResult Index(string hnid = "HN400219", string searchTerm = "", string[] statusFilter = null, int? month = null, int? year = null, int page = 1)
        {
            int pageSize = 10;

            // Default to current month/year if none selected
            int filterMonth = month ?? DateTime.Now.Month;
            int filterYear = year ?? DateTime.Now.Year;

            var allKpis = GetMockKpiRows(hnid);

            // 1. Filter by the Monthly Database Entry
            var periodFiltered = allKpis.Where(k => k.ReportingPeriod.Month == filterMonth &&
                                                   k.ReportingPeriod.Year == filterYear);

            // 2. Filter by KPI Status (Fail / Outside Limit)
            if (statusFilter != null && statusFilter.Any())
            {
                periodFiltered = periodFiltered.Where(k => statusFilter.Contains(k.Status.ToString()));
            }

            // 3. Grouping and Pagination
            var grouped = periodFiltered.GroupBy(k => k.ElementId)
                                        .ToDictionary(g => g.Key, g => g.ToList());

            int totalElements = grouped.Count;
            int totalPages = (int)Math.Ceiling(totalElements / (double)pageSize);
            var pagedGroups = grouped.Skip((page - 1) * pageSize).Take(pageSize).ToDictionary(g => g.Key, g => g.Value);

            // 7. Build the ViewModel
            var viewModel = new ArmsDashboardViewModel
            {
                Hnid = hnid,
                SelectedMonth = filterMonth,
                SelectedYear = filterYear,
                StatusFilter = statusFilter?.ToList() ?? new List<string>(),
                GroupedElements = pagedGroups,
                TotalElements = totalElements,
                CurrentPage = page,
                TotalPages = totalPages,
                FromRecord = totalElements > 0 ? ((page - 1) * pageSize) + 1 : 0,
                ToRecord = Math.Min(page * pageSize, totalElements),
                AllNetworks = GetMockNetworks()
            };

            return View(viewModel);
        }


        // Inside your Controller or a Data Service
        private List<HeatNetworkStaticData> GetMockNetworks()
        {
            return new List<HeatNetworkStaticData>
            {
                new() {
                    Hnid = "HN400219",
                    Name = "Birmingham District Energy",
                    Provider = "Cofely"
                },
                new() {
                    Hnid = "HN600842",
                    Name = "Greenwich Peninsula",
                    Provider = "Pinnacle Power"
                },
                new() {
                    Hnid = "HN100335",
                    Name = "Pimlico District Heating",
                    Provider = "Westminster Council"
                },
                new() {
                    Hnid = "HN700111",
                    Name = "Elephant & Castle",
                    Provider = "Pinnacle Power"
                },
                new() {
                    Hnid = "HN200555",
                    Name = "Leicester District Energy",
                    Provider = "Engie"
                },
                new() {
                    Hnid = "HN300999",
                    Name = "Olympic Park Network",
                    Provider = "East London Energy"
                }
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




        public class HeatNetworkStaticData
        {
            public string Hnid { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Provider { get; set; } = string.Empty;

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
