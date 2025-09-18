namespace HNTAS.Web.UI.Models.Soa
{
    public class StatementOfApplicabilityViewModel
    {
        public string ProjectName { get; set; }
        public string PageTitle { get; set; }

        public int Pathway { get; set; }
        public List<PhaseViewModel> Phases { get; set; }
        public int CurrentPhaseIndex { get; set; } = 0; // Default to Phase 1
    }

    public class PhaseViewModel
    {
        public string Name { get; set; } // e.g., "Phase 1"
        public string Title { get; set; } // e.g., "Feasibility"
        public List<StageViewModel> Stages { get; set; }
        public bool IsActive { get; set; } // To mark the active tab
    }

    public class StageViewModel
    {
        public string Name { get; set; } // e.g., "Stage 1 - concept design"
        public List<ElementViewModel> Elements { get; set; }
    }

    public class ElementViewModel
    {
        public string Name { get; set; } // e.g., "Energy centre"
        public string? Url { get; set; } // URL for the link
        public string? Status { get; set; } // e.g., "Not yet started", "Completed"
        public string? StatusClass { get; set; } // CSS class for status styling
        public int? Count { get; set; } // Number of elements
    }
}
