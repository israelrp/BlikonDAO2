using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlikonDAO.Models
{
    public class QueryOperators
    {
        public List<FieldQuery> lstFieldsQuery { get; set; }
        public string LogicOperator { get; set; }
    }
}
