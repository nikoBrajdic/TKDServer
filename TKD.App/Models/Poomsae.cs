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
            Poomsaes11 = new HashSet<Category>();
            Poomsaes12 = new HashSet<Category>();
            Poomsaes21 = new HashSet<Category>();
            Poomsaes22 = new HashSet<Category>();
            Poomsaes31 = new HashSet<Category>();
            Poomsaes32 = new HashSet<Category>();
        }

        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        [Required]
        public string Ordinal { get; set; }

        public int PoomsaeTypeId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [InverseProperty(nameof(Category.Poomsae11))]
        public virtual ICollection<Category> Poomsaes11 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [InverseProperty(nameof(Category.Poomsae12))]
        public virtual ICollection<Category> Poomsaes12 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [InverseProperty(nameof(Category.Poomsae21))]
        public virtual ICollection<Category> Poomsaes21 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [InverseProperty(nameof(Category.Poomsae22))]
        public virtual ICollection<Category> Poomsaes22 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [InverseProperty(nameof(Category.Poomsae31))]
        public virtual ICollection<Category> Poomsaes31 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [InverseProperty(nameof(Category.Poomsae32))]
        public virtual ICollection<Category> Poomsaes32 { get; set; }

        public virtual PoomsaeType PoomsaeType { get; set; }

        public override string ToString() => Name;
    }
}
