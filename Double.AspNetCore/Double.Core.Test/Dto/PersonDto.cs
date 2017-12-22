using Double.Core.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Double.Core.Test.Dto
{
    public class PersonDto : EntityDto
    {
        [Required(ErrorMessage ="姓名不能为空")]
        public string Name { get; set; }

        [Range(1, 10, ErrorMessage ="id介于1~10")]
        public new int Id { get; set; }

        public int? AssignedPersonId { get; set; }

    
    }
}
