namespace TKD.App.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Poomsae
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Poomsae()
        {
            CategoryPoomsaes = new HashSet<CategoryPoomsae>();
        }

        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        [Required]
        public string Ordinal { get; set; }

        public int PoomsaeTypeId { get; set; }

        public virtual PoomsaeType PoomsaeType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CategoryPoomsae> CategoryPoomsaes { get; set; }

        public override string ToString() => Name;
    }
}
