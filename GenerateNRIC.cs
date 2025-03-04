using System;
using System.IO;
using System.Linq;
using Tricentis.Automation.AutomationInstructions.TestActions;
using Tricentis.Automation.Creation;
using Tricentis.Automation.Engines;
using Tricentis.Automation.Engines.Representations;
using Tricentis.Automation.Engines.SpecialExecutionTasks;
using Tricentis.Automation.Engines.SpecialExecutionTasks.Attributes;
using Tricentis.Automation.Interaction.SpecialExecutionTasks;

namespace AutomationExtensions
{
    [SpecialExecutionTaskName("GetNRIC")]
    internal class GetNRIC : SpecialExecutionTaskEnhanced
    {
        public GetNRIC(Validator validator)
            : base(validator) { }

        public override void ExecuteTask(ISpecialExecutionTaskTestAction testAction)
        {
            IParameter prefix;
            IParameter yearOfIssue;
            IParameter output;

            try
            {
                var validator = new SetParameterValidator(testAction);
                ActionMode[] inputOnly = { ActionMode.Input };
                ActionMode[] verifyBuffer = { ActionMode.Verify, ActionMode.Buffer };
                prefix = validator.Take("Prefix").Required().NotEmpty().Accepts(inputOnly).Run();
                yearOfIssue = validator.Take("Year Of Issue").Required().NotEmpty().Accepts(inputOnly).Run();
                output = validator.Take("Output").Required().NotEmpty().Accepts(verifyBuffer).Run();
            }
            catch (ParameterValidationException)
            {
                return;
            }

            String strPrefix = prefix.ValueAsString();
            char charPrefix = strPrefix.ToCharArray()[0];
            String strOutput = GenerateNRIC(charPrefix);

            HandleActualValue(testAction, output, strOutput);
        }

        static string GenerateNRIC(char prefix)
        {
            Random rand = new Random();

            // Generate a random 7-digit number
            int nricNumber = rand.Next(1000000, 9999999);

            // Calculate the check digit
            char checkDigit = CalculateCheckDigit(prefix, nricNumber);

            // Construct the final NRIC
            return $"{prefix}{nricNumber}{checkDigit}";
        }

        static char CalculateCheckDigit(char prefix, int number)
        {
            // NRIC weight factors
            int[] weights = { 2, 7, 6, 5, 4, 3, 2 };

            // Convert number to an array of digits
            int[] digits = number.ToString().ToCharArray().Select(c => c - '0').ToArray();

            // Compute weighted sum
            int weightedSum = 0;
            for (int i = 0; i < digits.Length; i++)
            {
                weightedSum += digits[i] * weights[i];
            }

            // Add offset based on prefix
            if (prefix == 'T' || prefix == 'G')
            {
                weightedSum += 4;  // Offset for T and G
            }

            // Compute remainder
            int remainder = weightedSum % 11;

            // NRIC Checksum letters
            string[] checksumLettersSFG = { "J", "Z", "I", "H", "G", "F", "E", "D", "C", "B", "A" };
            string[] checksumLettersTG = { "G", "Y", "I", "H", "G", "F", "E", "D", "C", "B", "A" };

            return (prefix == 'S' || prefix == 'F') ? checksumLettersSFG[remainder][0] : checksumLettersTG[remainder][0];
        }
    }
}