namespace TKD.App.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Category : IComparable<Category>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Category()
        {
            CurrentRound = 1;
            Contestants = new HashSet<Contestant>();
        }

        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        public bool IsFreestyle { get; set; }

        public int CurrentRound { get; set; }

        [ForeignKey(nameof(Poomsae11))]
        public int? Poomsae11ID { get; set; }

        [ForeignKey(nameof(Poomsae12))]
        public int? Poomsae12ID { get; set; }

        [ForeignKey(nameof(Poomsae21))]
        public int? Poomsae21ID { get; set; }

        [ForeignKey(nameof(Poomsae22))]
        public int? Poomsae22ID { get; set; }

        [ForeignKey(nameof(Poomsae31))]
        public int? Poomsae31ID { get; set; }

        [ForeignKey(nameof(Poomsae32))]
        public int? Poomsae32ID { get; set; }

        public virtual Poomsae Poomsae11 { get; set; }

        public virtual Poomsae Poomsae12 { get; set; }

        public virtual Poomsae Poomsae21 { get; set; }

        public virtual Poomsae Poomsae22 { get; set; }

        public virtual Poomsae Poomsae31 { get; set; }

        public virtual Poomsae Poomsae32 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contestant> Contestants { get; set; }

        public int CompareTo(Category obj) => obj.ToString().CompareTo(ToString());

        public override string ToString() => ShortName;
    }
}
