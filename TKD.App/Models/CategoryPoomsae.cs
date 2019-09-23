using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKD.App.Models
{
    public class CategoryPoomsae
    {
        public int ID { get; set; }

        public int Round { get; set; }

        public int Index { get; set; }

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public int PoomsaeId { get; set; }

        public virtual Poomsae Poomsae { get; set; }
    }
}
