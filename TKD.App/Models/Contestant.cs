namespace TKD.App.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Contestant : IComparable<Contestant>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Contestant()
        {
            Scores = new HashSet<Score>();
        }

        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        public string TrackPath { get; set; }

        public int TeamId { get; set; }

        public int CategoryId { get; set; }

        public bool Active { get; set; }

        public virtual Category Category { get; set; }

        public virtual Team Team { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Score> Scores { get; set; }

        public int CompareTo(Contestant other) => other.ToString().CompareTo(ToString());

        public override string ToString() => $"{Name} {Surname}";
    }
}
