using System.ComponentModel.DataAnnotations;

namespace DarnTheLuck.ViewModels
{
    public class UseCodeViewModel
    {
        [Required(ErrorMessage = "Access Code is required")]
        public string Value { get; set; }
    }
}
