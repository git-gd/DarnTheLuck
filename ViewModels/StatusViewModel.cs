using System.ComponentModel.DataAnnotations;

namespace DarnTheLuck.ViewModels
{
    public class StatusViewModel
    {
        [Required(ErrorMessage="Status Name Is Required")]
        public string Name { get; set; }
    }
}
