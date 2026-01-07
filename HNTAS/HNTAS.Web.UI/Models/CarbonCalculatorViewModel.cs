using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class CarbonCalculatorViewModel
    {
        public CarbonCalculatorRequest Request { get; set; } = new();
        // Output display
        public decimal? TotalCarbonEmission { get; set; }
        public string? Error { get; set; }

    }
    public class CarbonCalculatorRequest
    {
        public Background Background { get; set; } = new();
        public Energy Energy { get; set; } = new();
    }

    public class Background
    {
        [Required, DataType(DataType.Date)]
        public string? DateWorkbookCompleted { get; set; } = "2025-11-10"; // YYYY-MM-DD

        [Required]
        [RegularExpression("^(new|existing|extended|other)$", ErrorMessage = "Allowed: new, existing, extended, other")]
        public string? NetworkStatus { get; set; } = "existing";

        [Required]
        [RegularExpression("^(water|space|both)$", ErrorMessage = "Allowed: water, space, both")]
        public string? NetworkServiceProvision { get; set; } = "both";

        [Required] public string? Name { get; set; } = "Sample API Call";
        [Required] public string? NetworkID { get; set; } = "HN0001234";
        [Required] public string? NetworkName { get; set; } = "Sample Heat Network";
        public string? NetworkNamePrevious { get; set; }
        public string? HeatNetworkZone { get; set; }
        public string? AddressOfThePrimaryEnergyCentre { get; set; }
        [Required] public string? PostcodeOfThePrimaryEnergyCentre { get; set; } = "AA00 0A1";
        public string? NetworkOperator { get; set; }
        public string? NetworkOperatorContact { get; set; }        
        public string? ContactTelephone { get; set; }

        [Required, EmailAddress]
        public string? ContactEmail { get; set; } = "admin@sample.com";

        public string? DescriptionOfNetwork { get; set; }

        [DataType(DataType.Date)]
        public string? DateOfInitialOperation { get; set; }

        [DataType(DataType.Date)]
        public string? CommissioningDate { get; set; } = "2025-09-14"; // YYYY-MM-DD
    }

    public class Energy
    {
        [Required] public int? YearCount { get; set; } = 1;
        [Required] public int? StartYear { get; set; } = 2024;

        // Arrays: accept comma-separated numbers (we’ll parse server-side)
        [Required] public string? EnergyHeatNetworkPrimaryLossesCsv { get; set; } // array[number]
        [Required] public List<decimal>? EnergyHeatNetworkPrimaryLosses { get; set; } = new();

        //// Plant counts & inputs
        [Required] public int? ChpCount { get; set; } = 1;
        [Required] public int? HeatPumpCount { get; set; } = 1;
        [Required] public int? RecoveredCount { get; set; } = 1;
        [Required] public int? BoilerCount { get; set; } = 1;

        // Inputs (Lists of objects)
        [Required] public List<ChpInput>? ChpInputs { get; set; } = new();
        [Required] public List<HeatPumpInput>? HeatPumpInputs { get; set; } = new();
        [Required] public List<RecoveredInput>? RecoveredInputs { get; set; } = new();
        [Required] public List<BoilerInput>? BoilerInputs { get; set; } = new();

        //// Optional arrays
        public string? EppElectricityUsedForPumpingValueCsv { get; set; } = "157";// array[number]
        public List<decimal> EppElectricityUsedForPumpingValues { get; set; } = [157];
        public string? EppElectricityUsedForPumpingNotes { get; set; }
        public string? EppSleevingPCentNotes { get; set; }
    }

    public class ChpInput
    {
        [Required] public int? ChpFuelTypeInput { get; set; } = 17;
        [Required, DataType(DataType.Date)] public string? ChpInstallationDateInput { get; set; } = "2025-09-16"; // YYYY-MM-DD
        public string? ChpOperationalModeInput { get; set; }

        [Required] public string? ChpUsefulHeatValueCsv { get; set; } = "100"; // array[int]
        [Required] public List<int> ChpUsefulHeatValues { get; set; } = [100];
        public string? ChpUsefulHeatNotes { get; set; }

        [Required] public string? ChpElectricityGeneratedValueCsv { get; set; } = "100"; // array[int]
        [Required] public List<int> ChpElectricityGeneratedValues { get; set; } = [100];
        public string? ChpElectricityGeneratedNotes { get; set; }

        [Required] public string? ChpFuelUsedValueCsv { get; set; } = "1000"; // array[int]
        [Required] public List<int> ChpFuelUsedValues { get; set; } = [1000];
        public string? ChpFuelUsedNotes { get; set; }

        [Required] public string? ChpHeatCoolingValueCsv { get; set; } = "100";// array[int]
        [Required] public List<int> ChpHeatCoolingValues { get; set; } = [100];
        public string? ChpHeatCoolingNotes { get; set; }

        [Required] public string? ChpSleevingPCentValueCsv { get; set; } = "0";// array[int]
        [Required] public List<int> ChpSleevingPCentValues { get; set; } = [0];
        public string? ChpSleevingPCentNotes { get; set; }

        [Required] public int? ChpMaxHeatOutput { get; set; } = 1000; // kWth
        [Required] public int? ChpMaxElectricityOutput { get; set; } = 1200; // kWe
    }

    public class HeatPumpInput
    {
        [Required] public int? HpmTypeFuelUsedInput { get; set; } = 11;

        [Required] public string? HpmUsefulHeatGeneratedValueCsv { get; set; } = "1000";// array[int]
        [Required] public List<int> HpmUsefulHeatGeneratedValues { get; set; } = [1000];
        public string? HpmUsefulHeatGeneratedNotes { get; set; }

        [Required] public string? HpmEnergyUsedValueCsv { get; set; } = "1000"; // array[int]
        [Required] public List<int> HpmEnergyUsedValues { get; set; } = [1000];
        public string? HpmEnergyUsedNotes { get; set; }

        [Required] public string? HpmUsefulCoolingGeneratedValueCsv { get; set; } = "1000";// array[int]
        [Required] public List<int> HpmUsefulCoolingGeneratedValues { get; set; } = [1000];
        public string? HpmUsefulCoolingGeneratedNotes { get; set; }

        [Required] public string? HpmSleevingPCentValueCsv { get; set; } = "0";// array[int]
        [Required] public List<int> HpmSleevingPCentValues { get; set; } = [0];
        public string? HpmSleevingPCentNotes { get; set; }

        [Required] public int? HpmMaxHeatOutput { get; set; } = 1000;
    }

    public sealed class RecoveredInput
    {
        [Required] public int? HrwHeatRecoverySourceInput { get; set; } = 1;
        [Required] public string? HrwUsefulHeatGeneratedValueCsv { get; set; } = "1000";// array[int]
        [Required] public List<int> HrwUsefulHeatGeneratedValues { get; set; } = [1000];
        public string? HrwUsefulHeatGeneratedNotes { get; set; }

        [Required] public string? HrwHeatUsedByCoolingProductionValueCsv { get; set; } = "1000"; // array[int]
        [Required] public List<int> HrwHeatUsedByCoolingProductionValues { get; set; } = [1000];
        public string? HrwHeatUsedByCoolingProductionNotes { get; set; }

        [Required] public string? HrwSleevingPCentValueCsv { get; set; } = "0";// array[int]
        [Required] public List<int> HrwSleevingPCentValues { get; set; } = [0];
        public string? HrwSleevingPCentNotes { get; set; }

        [Required] public int? HrwMaxHeatOutput { get; set; } = 1000;
    }

    public class BoilerInput
    {
       
        // Boilers
        [Required] public int? BlrTypeFuelUsedInput { get; set; } = 17;
        [Required] public string? BlrUsefulHeatGeneratedValueCsv { get; set; } = "1000";// array[int]
        [Required] public List<int> BlrUsefulHeatGeneratedValues { get; set; } = [1000];
        public string? BlrUsefulHeatGeneratedNotes { get; set; }

        [Required] public string? BlrFuelUsedByValueCsv { get; set; } = "1000";// array[int]
        [Required] public List<int> BlrFuelUsedByValues { get; set; } = [1000];
        public string? BlrFuelUsedByNotes { get; set; }

        [Required] public string? BlrHeatUsedForCoolingProductionValueCsv { get; set; } = "1000";// array[int]
        [Required] public List<int> BlrHeatUsedForCoolingProductionValues { get; set; } = [1000];
        public string? BlrHeatUsedForCoolingProductionNotes { get; set; }

        [Required] public string? BlrSleevingPCentValueCsv { get; set; } = "0";// array[int]
        [Required] public List<int> BlrSleevingPCentValues { get; set; } = [0];
        public string? BlrSleevingPCentNotes { get; set; }

        [Required] public int? BlrMaxHeatOutput { get; set; } = 1000;        
    }
}
