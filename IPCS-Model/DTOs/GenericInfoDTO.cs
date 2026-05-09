using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    public class GenericInfoDTO
    {
        [Required(ErrorMessage = "Must input Generic Name")]
        public string GenericName { get; set; } = string.Empty;
        public string? Indication { get; set; }
        public string? SideEffects { get; set; }
        public string? ContraIndication { get; set; }
        public string? DosageFormAdvice { get; set; }
        public string? DrugClass { get; set; }
        public string? PregnancyCategory { get; set; }
        public bool IsActive { get; set; } = true;
    }
}