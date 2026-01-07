using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Text.Json;

namespace HNTAS.Web.UI.Controllers
{
    public class CarbonCalculatorController : Controller
    {
        private readonly ICarbonCalculatorService _carbonCalculatorService;
        private readonly ILogger<CarbonCalculatorController> _logger;

        public CarbonCalculatorController(ICarbonCalculatorService carbonCalculatorService, ILogger<CarbonCalculatorController> logger)
        {
            _carbonCalculatorService = carbonCalculatorService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            var vm = new CarbonCalculatorViewModel();
            // ensure at least one item for each repeater so view can safely access [0]
            vm.Request.Energy.ChpInputs = new List<Models.ChpInput> { new Models.ChpInput() };
            vm.Request.Energy.HeatPumpInputs = new List<Models.HeatPumpInput> { new Models.HeatPumpInput() };
            vm.Request.Energy.RecoveredInputs = new List<Models.RecoveredInput> { new Models.RecoveredInput() };
            vm.Request.Energy.BoilerInputs = new List<Models.BoilerInput> { new Models.BoilerInput() };
            return View("Index", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Result(CarbonCalculatorViewModel model)
        {
            this.ShowBackButton("Index");
            ValidateAllArrays(model, ModelState);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                //var requestJSON = FormRequestJSON(model);
                var request = FormRequestJSON(model);
                var response = await _carbonCalculatorService.CalculateAsync(request);
                var result = JsonDocument.Parse(response.RawContent);
                var resultRoot = result.RootElement;

                decimal? intensity = null;
                if (resultRoot.TryGetProperty("totalCarbonEmission", out var val))
                {
                    if (val.ValueKind == JsonValueKind.String
                        && decimal.TryParse(val.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    {
                        intensity = d;
                    }
                    else if (val.ValueKind == JsonValueKind.Number && val.TryGetDecimal(out var dn))
                    {
                        intensity = dn;
                    }
                }

                model.TotalCarbonEmission = intensity;
                if (intensity == null)
                {
                    model.Error = "No data found for the provided Heat Network ID.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate carbon emission for hnId {HnId}", model.Request.Background.NetworkID);
                model.Error = "An unexpected error occurred while calculating carbon emission.";
            }
            return View("Result", model);
        }



        /// <summary>
        /// Validates all CSV arrays and numbers in the new nested view model.
        /// Populates parsed List<T> properties and adds model state errors for any issues.
        /// </summary>
        public static void ValidateAllArrays(CarbonCalculatorViewModel m, ModelStateDictionary modelState)
        {
            if (m?.Request?.Energy is null)
            {
                modelState.AddModelError(nameof(CarbonCalculatorViewModel.Request),
                    "Energy section is required.");
                return;
            }

            var energy = m.Request.Energy;

            // ---- Optional top-level array in Energy ----
            ValidateDecimalCsv(
                display: "Epp Electricity Used For Pumping Value",
                modelStateKeyCsv: "Request.Energy.EppElectricityUsedForPumpingValueCsv",
                csv: energy.EppElectricityUsedForPumpingValueCsv,
                setValues: vals => energy.EppElectricityUsedForPumpingValues = vals,
                modelState);

            ValidateDecimalCsv(
                display: "Energy Heat Network Primary losses per year (comma-separated) *",
                modelStateKeyCsv: "Request.Energy.EnergyHeatNetworkPrimaryLossesCsv",
                csv: energy.EnergyHeatNetworkPrimaryLossesCsv,
                setValues: vals => energy.EnergyHeatNetworkPrimaryLosses = vals,
                modelState);

            // ---- CHP inputs ----
            if (energy.ChpInputs != null)
            {
                for (int i = 0; i < energy.ChpInputs.Count; i++)
                {
                    var item = energy.ChpInputs[i];
                    var prefix = $"Request.Energy.ChpInputs[{i}]";

                    ValidateIntCsv("CHP useful heat", $"{prefix}.ChpUsefulHeatValueCsv",
                        item.ChpUsefulHeatValueCsv, vals => item.ChpUsefulHeatValues = vals, modelState);

                    ValidateIntCsv("CHP electricity generated", $"{prefix}.ChpElectricityGeneratedValueCsv",
                        item.ChpElectricityGeneratedValueCsv, vals => item.ChpElectricityGeneratedValues = vals, modelState);

                    ValidateIntCsv("CHP fuel used", $"{prefix}.ChpFuelUsedValueCsv",
                        item.ChpFuelUsedValueCsv, vals => item.ChpFuelUsedValues = vals, modelState);

                    ValidateIntCsv("CHP heat used for cooling", $"{prefix}.ChpHeatCoolingValueCsv",
                        item.ChpHeatCoolingValueCsv, vals => item.ChpHeatCoolingValues = vals, modelState);

                    ValidateIntCsv("CHP sleeving %", $"{prefix}.ChpSleevingPCentValueCsv",
                        item.ChpSleevingPCentValueCsv, vals => item.ChpSleevingPCentValues = vals, modelState);

                    // Max outputs are single numbers (already int?), if they ever come as CSV, add a validator here.
                    // You can also enforce ranges:
                    ValidateNumberRange(item.ChpMaxHeatOutput, 0, int.MaxValue,
                        $"{prefix}.ChpMaxHeatOutput", "CHP max heat output (kWh)", modelState);
                    ValidateNumberRange(item.ChpMaxElectricityOutput, 0, int.MaxValue,
                        $"{prefix}.ChpMaxElectricityOutput", "CHP max electricity output (kWe)", modelState);
                }
            }

            // ---- Heat Pump inputs ----
            if (energy.HeatPumpInputs != null)
            {
                for (int i = 0; i < energy.HeatPumpInputs.Count; i++)
                {
                    var item = energy.HeatPumpInputs[i];
                    var prefix = $"Request.Energy.HeatPumpInputs[{i}]";

                    ValidateIntCsv("HP useful heat generated", $"{prefix}.HpmUsefulHeatGeneratedValueCsv",
                        item.HpmUsefulHeatGeneratedValueCsv, vals => item.HpmUsefulHeatGeneratedValues = vals, modelState);

                    ValidateIntCsv("HP energy used", $"{prefix}.HpmEnergyUsedValueCsv",
                        item.HpmEnergyUsedValueCsv, vals => item.HpmEnergyUsedValues = vals, modelState);

                    ValidateIntCsv("HP useful cooling generated", $"{prefix}.HpmUsefulCoolingGeneratedValueCsv",
                        item.HpmUsefulCoolingGeneratedValueCsv, vals => item.HpmUsefulCoolingGeneratedValues = vals, modelState);

                    ValidateIntCsv("HP sleeving %", $"{prefix}.HpmSleevingPCentValueCsv",
                        item.HpmSleevingPCentValueCsv, vals => item.HpmSleevingPCentValues = vals, modelState);

                    ValidateNumberRange(item.HpmMaxHeatOutput, 0, int.MaxValue,
                        $"{prefix}.HpmMaxHeatOutput", "HP max heat output (kWh)", modelState);
                }
            }

            // ---- Heat Recovery inputs ----
            if (energy.RecoveredInputs != null)
            {
                for (int i = 0; i < energy.RecoveredInputs.Count; i++)
                {
                    var item = energy.RecoveredInputs[i];
                    var prefix = $"Request.Energy.RecoveredInputs[{i}]";

                    ValidateIntCsv("HR useful heat generated", $"{prefix}.HrwUsefulHeatGeneratedValueCsv",
                        item.HrwUsefulHeatGeneratedValueCsv, vals => item.HrwUsefulHeatGeneratedValues = vals, modelState);

                    ValidateIntCsv("HR heat used by cooling production", $"{prefix}.HrwHeatUsedByCoolingProductionValueCsv",
                        item.HrwHeatUsedByCoolingProductionValueCsv, vals => item.HrwHeatUsedByCoolingProductionValues = vals, modelState);

                    ValidateIntCsv("HR sleeving %", $"{prefix}.HrwSleevingPCentValueCsv",
                        item.HrwSleevingPCentValueCsv, vals => item.HrwSleevingPCentValues = vals, modelState);
                }
            }

            // ---- Boiler inputs ----
            if (energy.BoilerInputs != null)
            {
                for (int i = 0; i < energy.BoilerInputs.Count; i++)
                {
                    var item = energy.BoilerInputs[i];
                    var prefix = $"Request.Energy.BoilerInputs[{i}]";

                    ValidateIntCsv("Boiler useful heat generated", $"{prefix}.BlrUsefulHeatGeneratedValueCsv",
                        item.BlrUsefulHeatGeneratedValueCsv, vals => item.BlrUsefulHeatGeneratedValues = vals, modelState);

                    ValidateIntCsv("Boiler fuel used", $"{prefix}.BlrFuelUsedByValueCsv",
                        item.BlrFuelUsedByValueCsv, vals => item.BlrFuelUsedByValues = vals, modelState);

                    ValidateIntCsv("Boiler heat used for cooling production", $"{prefix}.BlrHeatUsedForCoolingProductionValueCsv",
                        item.BlrHeatUsedForCoolingProductionValueCsv, vals => item.BlrHeatUsedForCoolingProductionValues = vals, modelState);

                    ValidateIntCsv("Boiler sleeving %", $"{prefix}.BlrSleevingPCentValueCsv",
                        item.BlrSleevingPCentValueCsv, vals => item.BlrSleevingPCentValues = vals, modelState);

                    ValidateNumberRange(item.BlrMaxHeatOutput, 0, int.MaxValue,
                        $"{prefix}.BlrMaxHeatOutput", "Boiler max heat output (kWh)", modelState);
                }
            }
        }

        // ---------- Helper validators ----------

        private static void ValidateIntCsv(
            string display,
            string modelStateKeyCsv,
            string? csv,
            Action<List<int>> setValues,
            ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                modelState.AddModelError(modelStateKeyCsv, $"{display}: value is required.");
                return;
            }

            if (!TryParseCsv<int>(csv, int.TryParse, out var values, out var error))
            {
                modelState.AddModelError(modelStateKeyCsv, $"{display}: {error}");
                return;
            }

            setValues(values!);
        }

        private static void ValidateDecimalCsv(
            string display,
            string modelStateKeyCsv,
            string? csv,
            Action<List<decimal>> setValues,
            ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(csv))
                return; // optional (as per your screenshot). Remove if required.

            if (!TryParseCsv<decimal>(csv, (string s, out decimal v) =>
                    decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out v),
                out var values, out var error))
            {
                modelState.AddModelError(modelStateKeyCsv, $"{display}: {error}");
                return;
            }

            setValues(values!);
        }

        private static void ValidateNumberRange(int? value, int min, int max, string key, string display, ModelStateDictionary ms)
        {
            if (value is null)
            {
                ms.AddModelError(key, $"{display} is required.");
                return;
            }
            if (value < min || value > max)
            {
                ms.AddModelError(key, $"{display} must be between {min} and {max}.");
            }
        }

        /// <summary>
        /// Generic CSV parser that returns List&lt;T&gt; and a friendly error.
        /// Accepts commas and optional whitespace; empty tokens cause an error.
        /// </summary>
        private static bool TryParseCsv<T>(
            string csv,
            TryParseDelegate<T> tryParse,
            out List<T>? values,
            out string error)
        {
            values = new List<T>();
            error = string.Empty;

            var tokens = csv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                error = "No values found.";
                return false;
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                var s = tokens[i];
                if (!tryParse(s, out var val))
                {
                    error = $"Invalid number at position {i + 1}: '{s}'.";
                    values = null;
                    return false;
                }
                values.Add(val);
            }
            return true;
        }

        private delegate bool TryParseDelegate<T>(string s, out T value);



        // Builds the nested JSON object (background+energy) from the view model values.
        /// </summary>

        public static HNTAS.Api.Client.Model.CarbonCalculatorRequest FormRequestJSON(CarbonCalculatorViewModel m)
        {
            if (m == null || m.Request == null)
                throw new ArgumentNullException(nameof(m), "View model and its Request must not be null.");

            var vmReq = m.Request;
            var bg = vmReq.Background;
            var en = vmReq.Energy;

            // Helper local functions to deep-copy lists (avoid sharing references)
            static List<int> CopyIntList(List<int>? src) => src != null ? new List<int>(src) : new List<int>();
            static List<double>? ToDoubleList(List<decimal>? src) => src != null ? src.Select(d => (double)d).ToList() : null;

            // ---- Build payload ----
            var payload = new HNTAS.Api.Client.Model.CarbonCalculatorRequest
            {
                Background = new HNTAS.Api.Client.Model.Background
                {
                    DateWorkbookCompleted = bg.DateWorkbookCompleted,
                    NetworkStatus = bg.NetworkStatus,
                    NetworkServiceProvision = bg.NetworkServiceProvision,
                    Name = bg.Name,
                    NetworkID = bg.NetworkID,
                    NetworkName = bg.NetworkName,
                    NetworkNamePrevious = bg.NetworkNamePrevious,
                    HeatNetworkZone = bg.HeatNetworkZone,
                    AddressOfThePrimaryEnergyCentre = bg.AddressOfThePrimaryEnergyCentre,
                    PostcodeOfThePrimaryEnergyCentre = bg.PostcodeOfThePrimaryEnergyCentre,
                    NetworkOperator = bg.NetworkOperator,
                    NetworkOperatorContact = bg.NetworkOperatorContact,
                    ContactTelephone = bg.ContactTelephone,
                    ContactEmail = bg.ContactEmail,
                    DescriptionOfNetwork = bg.DescriptionOfNetwork,
                    DateOfInitialOperation = bg.DateOfInitialOperation,
                    CommissioningDate = bg.CommissioningDate
                },

                Energy = new HNTAS.Api.Client.Model.Energy
                {
                    // Scalars
                    YearCount = en.YearCount,
                    StartYear = en.StartYear,

                    // NUMBER arrays (decimal -> double)
                    EnergyHeatNetworkPrimaryLosses = ToDoubleList(en.EnergyHeatNetworkPrimaryLosses),

                    // Plant counts
                    ChpCount = en.ChpCount,
                    HeatPumpCount = en.HeatPumpCount,
                    RecoveredCount = en.RecoveredCount,
                    BoilerCount = en.BoilerCount,

                    // ---- CHP inputs ----
                    ChpInputs = (en.ChpInputs).Select(chp => new HNTAS.Api.Client.Model.ChpInput
                    {
                        ChpFuelTypeInput = chp.ChpFuelTypeInput,
                        ChpInstallationDateInput = chp.ChpInstallationDateInput,
                        ChpOperationalModeInput = chp.ChpOperationalModeInput, // keep actual value from model
                        ChpUsefulHeatValue = CopyIntList(chp.ChpUsefulHeatValues),
                        ChpUsefulHeatNotes = chp.ChpUsefulHeatNotes,
                        ChpElectricityGeneratedValue = CopyIntList(chp.ChpElectricityGeneratedValues),
                        ChpElectricityGeneratedNotes = chp.ChpElectricityGeneratedNotes,
                        ChpFuelUsedValue = CopyIntList(chp.ChpFuelUsedValues),
                        ChpFuelUsedNotes = chp.ChpFuelUsedNotes,
                        ChpHeatCoolingValue = CopyIntList(chp.ChpHeatCoolingValues),
                        ChpHeatCoolingNotes = chp.ChpHeatCoolingNotes,
                        ChpSleevingPCentValue = CopyIntList(chp.ChpSleevingPCentValues),
                        ChpSleevingPCentNotes = chp.ChpSleevingPCentNotes,
                        ChpMaxHeatOutput = chp.ChpMaxHeatOutput,
                        ChpMaxElectricityOutput = chp.ChpMaxElectricityOutput
                    }).ToList(),

                    // ---- Heat Pump inputs ----
                    HeatPumpInputs = (en.HeatPumpInputs).Select(hp => new HNTAS.Api.Client.Model.HeatPumpInput
                    {
                        HpmTypeFuelUsedInput = hp.HpmTypeFuelUsedInput,
                        HpmUsefulHeatGeneratedValue = CopyIntList(hp.HpmUsefulHeatGeneratedValues),
                        HpmUsefulHeatGeneratedNotes = hp.HpmUsefulHeatGeneratedNotes,
                        HpmEnergyUsedValue = CopyIntList(hp.HpmEnergyUsedValues),
                        HpmEnergyUsedNotes = hp.HpmEnergyUsedNotes,
                        HpmUsefulCoolingGeneratedValue = CopyIntList(hp.HpmUsefulCoolingGeneratedValues),
                        HpmUsefulCoolingGeneratedNotes = hp.HpmUsefulCoolingGeneratedNotes,
                        HpmSleevingPCentValue = CopyIntList(hp.HpmSleevingPCentValues),
                        HpmSleevingPCentNotes = hp.HpmSleevingPCentNotes,
                        HpmMaxHeatOutput = hp.HpmMaxHeatOutput
                    }).ToList(),

                    // ---- Heat Recovery inputs ----
                    RecoveredInputs = (en.RecoveredInputs).Select(hr => new HNTAS.Api.Client.Model.RecoveredInput
                    {
                        HrwHeatRecoverySourceInput = hr.HrwHeatRecoverySourceInput,
                        HrwUsefulHeatGeneratedValue = CopyIntList(hr.HrwUsefulHeatGeneratedValues),
                        HrwUsefulHeatGeneratedNotes = hr.HrwUsefulHeatGeneratedNotes,
                        HrwHeatUsedByCoolingProductionValue = CopyIntList(hr.HrwHeatUsedByCoolingProductionValues),
                        HrwHeatUsedByCoolingProductionNotes = hr.HrwHeatUsedByCoolingProductionNotes,
                        HrwSleevingPCentValue = CopyIntList(hr.HrwSleevingPCentValues),
                        HrwSleevingPCentNotes = hr.HrwSleevingPCentNotes,
                        HrwMaxHeatOutput = hr.HrwMaxHeatOutput,
                    }).ToList(),

                    // ---- Boiler inputs ----
                    BoilerInputs = (en.BoilerInputs).Select(b => new HNTAS.Api.Client.Model.BoilerInput
                    {
                        BlrTypeFuelUsedInput = b.BlrTypeFuelUsedInput,
                        BlrUsefulHeatGeneratedValue = CopyIntList(b.BlrUsefulHeatGeneratedValues),
                        BlrUsefulHeatGeneratedNotes = b.BlrUsefulHeatGeneratedNotes,
                        BlrFuelUsedByValue = CopyIntList(b.BlrFuelUsedByValues),
                        BlrFuelUsedByNotes = b.BlrFuelUsedByNotes,
                        BlrHeatUsedForCoolingProductionValue = CopyIntList(b.BlrHeatUsedForCoolingProductionValues),
                        BlrHeatUsedForCoolingProductionNotes = b.BlrHeatUsedForCoolingProductionNotes,
                        BlrSleevingPCentValue = CopyIntList(b.BlrSleevingPCentValues),
                        BlrSleevingPCentNotes = b.BlrSleevingPCentNotes,
                        BlrMaxHeatOutput = b.BlrMaxHeatOutput
                    }).ToList(),

                    // ---- Optional arrays/notes at Energy level ----
                    EppElectricityUsedForPumpingValue = ToDoubleList(en.EppElectricityUsedForPumpingValues),
                    EppElectricityUsedForPumpingNotes = en.EppElectricityUsedForPumpingNotes,
                    EppSleevingPCentNotes = en.EppSleevingPCentNotes
                }
            };

            return payload;
        }
    }
}
