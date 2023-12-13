using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlikonDAO.Models
{
    public class FieldQueryElements
    {
        public string LogicOperator { get; set; }
        public List<FieldQuery> Fields { get; set; }
    }
}
