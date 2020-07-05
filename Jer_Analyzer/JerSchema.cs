using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jer_Analyzer
{
    class JerSchema
    {
        private string name;
        private string dir_name;
        private string dec_name;
        private List<string> keyword;

        public JerSchema(string name, string dir_name, string dec_name,string keyword = null)
        {
            this.Name = name;
            this.Dir_name = dir_name;
            this.Dec_name = dec_name;
            this.keyword = keyword?.Split(',').ToList();
        }

        public string Name { get => name; set => name = value; }
        public string Dir_name { get => dir_name; set => dir_name = value; }
        public string Dec_name { get => dec_name; set => dec_name = value; }
        public List<string> Keyword { get => keyword; set => keyword = value; }
    }
}
