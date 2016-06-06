using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.ValidateObject.Tests
{    
    public class Model
    {
        
        [Required(ErrorMessage = "RequiredOne is required")]
        public string RequiredOne { get; set; }

        
        [StringLength(10, ErrorMessage = "Not Required should be at most 10 characters long")]
        public string NotRequired { get; set; }

        
        [Required(ErrorMessage = "ChildModel is required")]
        [ValidateObject]
        public ChildModel ChildModel { get; set; }

        [Required(ErrorMessage = "ChildModelList is required")]
        [ValidateObject]
        public List<ChildModel> ChildModelList { get; set; }
    }

    
    public class ChildModel
    {
        
        [Required(ErrorMessage = "RequiredValue is required")]
        public string RequiredValue { get; set; }
                
        public string NotRequiredValue { get; set; }

        [Required(ErrorMessage = "SubChildModel is required")]
        [ValidateObject]
        public SubChildModel SubChildModel { get; set; }

        [Required(ErrorMessage = "SubChildModelList is required")]
        [ValidateObject]
        public List<SubChildModel> SubChildModelList { get; set; }
    }

    public class SubChildModel
    {

        [Required(ErrorMessage = "RequiredValue is required")]
        public string RequiredValue { get; set; }


        public string NotRequiredValue { get; set; }

    }
}
