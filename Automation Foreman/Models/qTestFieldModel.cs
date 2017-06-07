using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation_Foreman.Models
{
    public class qTestFieldModel
    {
        public qTestFieldModel()
        {
            field_id = Guid.NewGuid();
        }

        public Guid field_id { get; set; }
        public List<qTestLinks> links { get; set; }
        public string id { get; set; }
        public string attribute_type { get; set; }
        public string label { get; set; }
        public string required { get; set; }
        public string constrained { get; set; }
        public string order { get; set; }
        public List<qTestAllowedValueModel> allowed_values { get; set; }
        public string multiple { get; set; }
        public string data_type { get; set; }
        public string searchable { get; set; }
        public string free_text_search { get; set; }
        public string search_key { get; set; }
        public string system_field { get; set; }
        public string original_name { get; set; }
        public string is_active { get; set; }
        public string project_id { get; set; }
    }
}