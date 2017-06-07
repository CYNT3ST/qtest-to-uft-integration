using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation_Foreman.Models
{
    public class qTestLinks
    {
        public qTestLinks()
        {
            link_id = Guid.NewGuid();
        }

        public Guid link_id { get; set; }
        public string rel { get; set; }
        public string href { get; set; }

    }
}