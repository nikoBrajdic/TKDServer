namespace TKD.App.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    public partial class Category : IComparable<Category>, IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Category()
        {
            CurrentRound = 1;
            Contestants = new HashSet<Contestant>();
            CategoryPoomsaes = new HashSet<CategoryPoomsae>();
        }

        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        public bool IsFreestyle { get; set; }

        public int CurrentRound { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contestant> Contestants { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CategoryPoomsae> CategoryPoomsaes { get; set; }

        public int CompareTo(Category obj) => obj.ToString().CompareTo(ToString());

        public override string ToString() => ShortName;

        private CategoryPoomsae GetPoomsae(int round, int index) =>
            MainWindow.Context.CategoryPoomsaes.Local.FirstOrDefault(ss => ss.Round == round && ss.Index == index && ss.Category == this);

        private void SetPoomsae(int round, int index, string poomsaeName)
        {
            var score = GetPoomsae(round, index);
            var poomsae = MainWindow.Context.Poomsaes.Local.FirstOrDefault(p => p.Name == poomsaeName);
            if (score != null)
            {
                score.Poomsae = poomsae;
            }
            else
            {
                MainWindow.Context.CategoryPoomsaes.Local.Add(new CategoryPoomsae
                {
                    Round = round,
                    Index = index,
                    Category = this,
                    Poomsae = poomsae
                });
            }
            MainWindow.Context.SaveChangesAsync();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Poomsae).ShortName;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return MainWindow.Context.Poomsaes.Local.FirstOrDefault(p => p.Name == value as string);
            }
            catch
            {
                return "";
            }
        }

        public string Poomsae11 { get => GetPoomsae(1, 1).Poomsae.ToString(); set => SetPoomsae(1, 1, value); }
        public string Poomsae12 { get => GetPoomsae(1, 2).Poomsae.ToString(); set => SetPoomsae(1, 2, value); }
        public string Poomsae21 { get => GetPoomsae(2, 1).Poomsae.ToString(); set => SetPoomsae(2, 1, value); }
        public string Poomsae22 { get => GetPoomsae(2, 2).Poomsae.ToString(); set => SetPoomsae(2, 2, value); }
        public string Poomsae31 { get => GetPoomsae(3, 1).Poomsae.ToString(); set => SetPoomsae(3, 1, value); }
        public string Poomsae32 { get => GetPoomsae(3, 2).Poomsae.ToString(); set => SetPoomsae(3, 2, value); }

    }
}
