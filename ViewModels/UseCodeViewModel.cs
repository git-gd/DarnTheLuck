using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DarnTheLuck.ViewModels
{
    public class UseCodeViewModel
    {
        [Required(ErrorMessage = "Access Code is required")]
        public string Value { get; set; }
    }
}
