using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.Entities
{
    public class GenericInfo
    {
        [Key]
        public int GenericId { get; set; }

        [Required(ErrorMessage = "Must input Generic Name")]
        [StringLength(200)]
        public string GenericName { get; set; } = string.Empty;

        public string? Indication { get; set; } //On Which Disease working it?
        public string? SideEffects { get; set; } // Side-Effect
        public string? ContraIndication { get; set; } // Whose arenot eat?
        public string? DosageFormAdvice { get; set; } // Eating Advice
        public string? DrugClass { get; set; } // Drug Class (Example: Antibiotic)
        public string? PregnancyCategory { get; set; } // Is it Applyable on Pregnency ? write descrively

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}