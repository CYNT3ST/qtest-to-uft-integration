using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation_Foreman.Models
{
    public class qTestAllowedValueModel
    {

        public qTestAllowedValueModel()
        {
            allowed_value_id = Guid.NewGuid();
        }

        public Guid allowed_value_id { get; set; }

        public List<qTestLinks> links { get; set; }
        public string value { get; set; }
        public string label { get; set; }
        public string order { get; set; }
        public string is_default { get; set; }
        public string is_active { get; set; }
    }
}