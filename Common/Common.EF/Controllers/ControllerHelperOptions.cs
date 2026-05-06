using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Controllers
{
    public class ControllerHelperOptions
    {
        public bool UseGlobalExceptionFilter { get; set; } = true;
        public bool UseValidationFilter { get; set; } = true;
        public bool UseLoggingFilter { get; set; } = true;
        public bool UsePerformanceFilter { get; set; } = false;
        public bool UseModelStateValidation { get; set; } = true;
    }
}
