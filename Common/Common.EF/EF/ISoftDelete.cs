using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
