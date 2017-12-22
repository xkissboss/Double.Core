using System;
using System.Collections.Generic;
using System.Text;

namespace Double.Core.Application.Services.Dto
{
    public interface IEntityDto<TPrimaryKey>
    {
        TPrimaryKey Id { get; set; }
    }

    public interface IEntityDto : IEntityDto<int>
    {

    }
}
