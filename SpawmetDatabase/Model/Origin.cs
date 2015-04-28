using System.ComponentModel.DataAnnotations;

namespace SpawmetDatabase.Model
{
    public enum Origin : byte
    {
        [Display(Name="Produkcja")]
        Production = 0,

        [Display(Name="Zewnątrz")]
        Outside = 1,
    }
}
