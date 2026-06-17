namespace HNTAS.Web.UI.Helpers
{
    public static class ElementStageMapping
    {
        public static readonly Dictionary<int, List<string>> Map = new()
        {
            { 1, new List<string> {
                "Stage 1: Concept Design",
                "Stage 2: Developed Design",
                "Stage 3: Technical Design",
                "Stage 4: Construction Design",
                "Stage 5: Installation",
                "Stage 6: Commissioning",
                "Stage 7: Operation & Maintenance"
            }},
            { 3, new List<string> {
                "Stage 3: Technical Design",
                "Stage 4: Construction Design",
                "Stage 5: Installation",
                "Stage 6: Commissioning",
                "Stage 7: Operation & Maintenance"
            }}
        };
    }
}
