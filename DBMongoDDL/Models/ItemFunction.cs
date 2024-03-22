using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlikonDAO.Models
{
    public class ItemFunction
    {
        public string FunctionName { get; set; }
        public string? NewFieldName { get; set; }
        public string? Unit { get; set; }
        public List<ItemField> Expressions { get; set; }
    }
}
